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
        public String name;
        List<Element> elements;
        List<Package> subPackages;

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
    }
}
