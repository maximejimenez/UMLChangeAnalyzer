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
            // for other packages
            /*if(name.Contains(".")){
                int firstPoint = name.IndexOf(".");
            }
            else
            {*/
            foreach (Element element in elements)
            {
                if(element.name == name){
                    return element;
                }
            }

            foreach (Package package in subPackages)
            {
                package.FindElement(name);
            }
            return null;
        }
    }
}
