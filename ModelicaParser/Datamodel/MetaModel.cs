using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModelicaParser.Datamodel
{
    class MetaModel : ISerializable
    {
        public String version;
        List<Package> packages;

        // Changes
        List<Package> addedPackages = new List<Package>();
        List<Package> modifiedPackages = new List<Package>();
        List<Package> removedPackages = new List<Package>();

        List<Element> addedElements = new List<Element>();
        List<Element> modifiedElements = new List<Element>();
        List<Element> removedElements = new List<Element>();

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

        public void FindElement(string name)
        {
            if(name.Contains(".")){
                //TODO Handle import / reference to other pckage element
            }
            else
            {
                foreach (Package package in packages)
                {

                }
            }
        }

        public void CompareWithMetamodel(MetaModel metamodel)
        {
            //TODO
        }
    }
}
