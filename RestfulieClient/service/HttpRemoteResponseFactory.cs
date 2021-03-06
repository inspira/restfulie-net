﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace RestfulieClient.service
{
    public class HttpRemoteResponseFactory
    {
        public static HttpRemoteResponse GetRemoteResponse(HttpWebResponse webResponse)
        {
            HttpRemoteResponse response = new HttpRemoteResponse(webResponse.StatusCode,
                GetHeadersDictionaryFrom(webResponse.Headers),
                GetContentFromStream(webResponse.GetResponseStream()));

            webResponse.Close();
            return response; 
        }

        private static String GetContentFromStream(Stream ResponseStream)
        {
            BufferedStream stream = new BufferedStream(ResponseStream);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static Dictionary<string, string> GetHeadersDictionaryFrom(WebHeaderCollection headers)
        {
            string pattern = "\\s+";
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int i = 0; i < headers.Keys.Count; i++)
            {
                string key = headers.GetKey(i).Replace("-", " ").ToUpper();
                key = Regex.Replace(key, pattern, "");
                string value = headers.Get(headers.GetKey(i));
                //System.Console.WriteLine("Key => " + key + " Value => " + value);
                dictionary.Add(key, value);
            }
            return dictionary;
        }
    }
}
