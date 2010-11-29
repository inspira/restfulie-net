using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestfulieClient.resources;
using System.Net;

namespace RestfulieBasecamp
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkCredential credentials = new NetworkCredential("a442677db3a54386f53a5f4e622445e05f97dee7", "X");

            string API_GetProjects = "https://inspiradev.basecamphq.com/projects.xml";
            Console.WriteLine("### Getting project list ###");
            dynamic projects = Restfulie.At(API_GetProjects, credentials).Get();

            Console.WriteLine(" > First project in list: {0}", projects.project[0].name);

            int projectID = projects.project[0].id;
            Console.WriteLine("### Getting project details ( project_id: + {0}) + ###", projectID);

            string API_GetProject = "https://inspiradev.basecamphq.com/projects/" + projectID + ".xml";
            dynamic project = Restfulie.At(API_GetProject, credentials).Get();

            Console.WriteLine(" > Project loaded from ID ({0}): {1}", projectID, project.name);

            Console.ReadLine();
        }
    }
}
