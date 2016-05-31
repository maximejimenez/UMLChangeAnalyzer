using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModelicaParser.Datamodel
{
    class MetaModel
    {
        // Attributes
        public String version;
        public List<Package> packages;

        // Changes

        // Packages
        List<Package> addedPackages = new List<Package>();
        List<Package> modifiedPackages = new List<Package>();
        List<Package> removedPackages = new List<Package>();

        //Elements
        List<Element> addedElements = new List<Element>();
        List<Element> modifiedElements = new List<Element>();
        List<Element> removedElements = new List<Element>();

        //Attributes
        List<Attribute> addedAttribute = new List<Attribute>();
        List<Attribute> modifiedAttribute = new List<Attribute>();
        List<Attribute> removedAttribute = new List<Attribute>();

        public MetaModel(string v){
            version = v;
            packages = new List<Package>();
        }

        public void AddPackage(Package package)
        {
            packages.Add(package);
        }

        public Element FindElement(string name)
        {
            Element result = null;
            // For imports
            if(name.Contains(".")){
                int firstPoint = name.IndexOf(".");
                string packageName = name.Substring(0, firstPoint);
                string qualifiedName = name.Substring(firstPoint + 1, name.Length - firstPoint - 1);

                //Console.WriteLine("packageName = " + packageName);
                //Console.WriteLine("qualifiedName = " + qualifiedName);
                foreach (Package package in packages)
                {
                    if(packageName == package.name){
                        package.FindElement(qualifiedName);
                    }
                }

            }
            else
            {
                foreach (Package package in packages)
                {
                    result = package.FindElement(name);
                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public void CompareWithMetamodel(MetaModel newMetamodel)
        {
            //TODO several packages

            Package oldPackage = packages.ElementAt(0);
            Package newPackage = newMetamodel.packages.ElementAt(0);

            // New uniontypes
            List<Element> oldUniontypes = new List<Element>(oldPackage.elements);
            Dictionary<string, Element> newUniontypes = new Dictionary<string, Element>();
            foreach(Element element in newPackage.elements)
            {
                newUniontypes.Add(element.name, element);
            }

            foreach (Element uniontype in oldUniontypes)
            {
                Element newUniontype;
                if(newUniontypes.TryGetValue(uniontype.name, out newUniontype)){
                    uniontype.Compare(newUniontype);
                    // Add to modifiedElement if not equals
                    newUniontypes.Remove(uniontype.name);
                    oldUniontypes.Remove(uniontype);
                }
                else
                {
                    // Deleted elements
                    removedElements.Add(uniontype);
                    oldUniontypes.Remove(uniontype);
                }
            }

            foreach(Element addedUniontype in newUniontypes.Values)
            {
                // Added elements
                addedElements.Add(addedUniontype);
            }
        }
    }
}
