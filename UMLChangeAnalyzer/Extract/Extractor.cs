using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelicaChangeAnalyzer.Datamodel;
using System.Xml;

namespace ModelicaChangeAnalyzer.Extract
{
    class Extractor
    {
        private MainForm mainForm;
        private List<String> primitiveTypes;

        public Extractor(MainForm mainForm)
        {
            primitiveTypes = new List<String>();
            this.mainForm = mainForm;
        }

        public static MetaModel XMLtoMetamodel(string p)
        {
            throw new NotImplementedException();
        }

        #region Type parsers

        static MetaModel parseMetaModel(XmlDocument doc)
        {
            XmlNode metamodelNode = doc.GetElementsByTagName("metamodel").Item(0);
            string version = metamodelNode.Attributes["version"].Value;
            MetaModel metamodel = new MetaModel(version);
            XmlNodeList children = metamodelNode.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                Package package = parsePackage(children[i]);
                metamodel.AddPackage(package);
            }
            return metamodel;
        }
        static Package parsePackage(XmlNode elem)
        {
            string id = elem.Attributes["id"].Value;
            Package package = new Package(id);
            XmlAttribute noteAttribute = elem.Attributes["note"];
            if (noteAttribute != null)
                package.Note = noteAttribute.Value;

            XmlNodeList children = elem.ChildNodes;
            //TODO package children

            return package;
        }


        #endregion


    }
}
