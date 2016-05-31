using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModelicaParser.Datamodel
{
    class Package
    {
        // Backtracking
        public MetaModel metamodel;

        // Attributes
        public String name;
        public List<Element> elements;
        public List<Package> subPackages;

        public Package(string n)
        {
            name = n;
            elements = new List<Element>();
            subPackages = new List<Package>();
        }

        public void AddPackage(Package package){
            subPackages.Add(package);
        }

        public void AddElement(Element uniontype)
        {
            elements.Add(uniontype);
        }

        public Element FindElement(string name)
        {
            // For imports
            if (name.Contains("."))
            {
                int firstPoint = name.IndexOf(".");
                string packageName = name.Substring(0, firstPoint);
                string qualifiedName = name.Substring(firstPoint + 1, name.Length - firstPoint - 1);

                //Console.WriteLine("packageName = " + packageName);
                //Console.WriteLine("qualifiedName = " + qualifiedName);
                foreach (Package package in subPackages)
                {
                    if (packageName == package.name)
                    {
                        package.FindElement(qualifiedName);
                    }
                }
            }
            else
            {
                foreach (Element element in elements)
                {
                    if (element.name == name)
                    {
                        return element;
                    }
                }
            }


            return null;
        }
    }
}
