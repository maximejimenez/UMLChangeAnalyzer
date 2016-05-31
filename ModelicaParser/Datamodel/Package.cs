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
        //public MetaModel metamodel;
        private Package parentPackage = null;

        // Attributes
        private String name = "";
        private List<Element> elements =new List<Element>();
        private List<Package> subPackages = new List<Package>();

        // Changes
        //TODO

        #region Constructors

        public Package(string name)
        {
            this.name = name;
        }

        #endregion

        #region Getters and setters

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<Element> Elements
        {
            get { return elements; }
        }

        public void AddPackage(Package package){
            subPackages.Add(package);
        }

        public void AddElement(Element uniontype)
        {
            elements.Add(uniontype);
        }

        #endregion

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
                    if (element.Name == name)
                    {
                        return element;
                    }
                }
            }


            return null;
        }
    }
}
