using System;
using System.Linq;
using System.Xml.Linq;
using System.Dynamic;
using System.Reflection;
using RestfulieClient.service;
using System.Globalization;
using System.Collections.Generic;

namespace RestfulieClient.resources
{
    public class DynamicXmlResource : DynamicObject
    {
        private StringValueConverter converter = new StringValueConverter();

        public HttpRemoteResponse WebResponse { get; private set; }
        public IRemoteResourceService RemoteResourceService { get; private set; }
        public NumberFormatInfo NumberFormatInfo { get; set; }
        private XElement _xmlRepresentation;
        public XElement XmlRepresentation
        {
            get
            {
                if (_xmlRepresentation == null)
                {
                    if (this.WebResponse.HasNoContent())
                        return null;
                    else
                        _xmlRepresentation = XElement.Parse(this.WebResponse.Content);
                }
                return _xmlRepresentation;
            }
        }

        public DynamicXmlResource(HttpRemoteResponse response)
        {
            this.WebResponse = response;
            this.NumberFormatInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
        }

        public DynamicXmlResource(XElement element)
        {
            this._xmlRepresentation = element;
        }

        public DynamicXmlResource(HttpRemoteResponse response, IRemoteResourceService remoteService)
            : this(response)
        {
            this.RemoteResourceService = remoteService;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string fieldName = binder.Name.Replace("_", "-").ToLower();
            result = this.ParseXmlElement(fieldName);
            return result != null ? true : false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            object value = this.GetValueFromAttributeName(binder.Name, "href");
            if (value == null)
                throw new ArgumentException(string.Format("There is not method defined with name:", binder.Name));

            DynamicXmlResource resource = (DynamicXmlResource)this.InvokeRemoteResource(value.ToString(), binder.Name);

            if (resource.WebResponse.HasNoContent())
            {
                result = this.XmlRepresentation;
                this.UpdateWebResponse(resource.WebResponse);
            }
            else
            {
                result = resource;
            }
            return result != null ? true : false;
        }

        private object ParseXmlElement(string fieldName)
        {
            List<XElement> elements = XmlRepresentation.Elements(fieldName).ToList();

            if (elements.Count == 1)
            {
                XElement firstElement = elements[0];
                return this.GetValueFromXmlElement(firstElement);
            }
            else
            {
                List<object> objects = new List<object>();
                foreach (XElement element in elements)
                {
                    objects.Add(this.GetValueFromXmlElement(element));
                }
                return objects;
            }

        }

        private object InvokeRemoteResource(string url, string transitionName)
        {
            try
            {
                Type remoteResourceServiceType = this.RemoteResourceService.GetType();
                return remoteResourceServiceType.InvokeMember("Execute",
                                                                BindingFlags.InvokeMethod |
                                                                BindingFlags.Public |
                                                                BindingFlags.Instance,
                                                                null, this.RemoteResourceService, new Object[] { url, transitionName });
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("Error invoke remote resource method {0}.", ex.Message));
            }
        }

        private object GetValueFromXmlElement(XElement element)
        {
            if (element != null)
            {
                if (element.HasElements)
                {
                    return new DynamicXmlResource(this.WebResponse);
                }
                else
                {
                    object result = this.converter.TransformText(element.Value).WithNumberFormatInfo(this.NumberFormatInfo).ToValue();
                    return result;
                }
            }
            return null;
        }

        private XElement GetFirstElementWithName(string name)
        {
            XElement firstElement = XmlRepresentation.Descendants(name).FirstOrDefault();

            return firstElement;
        }

        private object GetValueFromAttributeName(string name, string attributeName)
        {
            foreach (XElement element in XmlRepresentation.Elements())
            {
                XAttribute attribute = element.Attributes().Where(attr => attr.Name == "rel").SingleOrDefault();
                if ((attribute != null) && (attribute.Value.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    XAttribute attrib = element.Attributes().Where(attr => attr.Name == attributeName).SingleOrDefault();
                    return attrib.Value;
                }
            }
            return null;
        }

        private void UpdateWebResponse(HttpRemoteResponse response)
        {
            this.WebResponse = response;
        }
    }
}
