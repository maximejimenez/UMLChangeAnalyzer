using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelicaParser.Datamodel;
using System.Xml;

namespace ModelicaParser
{
    class MM_Extractor
    {
        private MainForm mainForm;
        static Dictionary<string, List<Connector>> targetElements;
        static Dictionary<string, Element> declaredElements;
        static string[] Basetypes = new string[] { "Boolean", "Integer", "Real", "String" };

        public MM_Extractor(MainForm mainForm)
        {
            this.mainForm = mainForm;
        }

        internal void ExtractModel(string p1, string p2)
        {
            ModelicaToXML toXML = new ModelicaToXML();
            string xml = toXML.parse(p1);
            System.IO.File.WriteAllText(p2, xml);
        }

        internal void ReleaseModel()
        {
            throw new NotImplementedException();
        }

        internal static MetaModel XMLtoMetamodel(string p)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(p);
            targetElements = new Dictionary<string, List<Connector>>();
            declaredElements = new Dictionary<string, Element>();
            return parseMetaModel(doc);
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
                //package.metamodel = metamodel;
                metamodel.AddPackage(package);
            }

            Dictionary<string, List<Connector>>.KeyCollection targetsName = targetElements.Keys;
            foreach (string targetName in targetsName)
            {
                Element target = null;
                if (declaredElements.TryGetValue(targetName, out target))
                {
                    foreach (Connector connector in targetElements[targetName])
                    {
                        Connector clone = (Connector)connector.Clone();
                        clone.ParentElement = target;
                        clone.Target = target;
                        clone.UID = connector.ParentElement.Name + "::" + clone.UID + "R";                         // in order to not get the wrong connector
                        //Console.WriteLine("Connector (" + connector.ParentElement.Name + "::" + connector.UID + ") " + connector.SourceCardinality + " : " + connector.TargetCardinality + " / " + "Clone (" + clone.UID + ") " + clone.SourceCardinality + " : " + clone.TargetCardinality);
                        target.AddTargetConnector(clone);
                    }
                }
                else
                {
                    //Console.WriteLine("*** WARNING *** \t Can't find type : " + targetName);
                }
            }
            return metamodel;
        }
        static Package parsePackage(XmlNode elem)
        {
            string id = elem.Attributes["id"].Value;
            Package package = new Package(id);
            XmlNodeList children = elem.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].Name == "uniontype")
                {
                    Element uniontype = parseUniontype(children[i]);
                    uniontype.ParentPackage = package;
                    package.AddElement(uniontype);
                    declaredElements.Add(uniontype.Name, uniontype);
                }
                else
                {
                    // TODO : handle type alias (possibly in ModelicaToXML)
                }
            }

            return package;
        }
        static Element parseUniontype(XmlNode elem)
        {
            string id = elem.Attributes["id"].Value;
            Element uniontype = new Element("uniontype", id);
            XmlNodeList children = elem.ChildNodes;

            for (int i = 0; i < children.Count; i++)
            {
                Element record = parseRecord(children[i]);
                record.ParentElement = uniontype;
                uniontype.AddChild(record);
            }

            return uniontype;
        }
        static Element parseRecord(XmlNode elem)
        {
            string id = elem.Attributes["id"].Value;
            Element record = new Element("record", id);
            XmlNodeList children = elem.ChildNodes;

            for (int i = 0; i < children.Count; i++)
            {
                XmlAttributeCollection attributes = children[i].Attributes;
                string type = attributes["type"].Value;
                string name = attributes["name"].Value;
                string maxMultiplicity = attributes["maxMultiplicity"].Value;
                string minMultiplicity = attributes["minMultiplicity"].Value;

                if (Basetypes.Contains<string>(type))
                {
                    ModelicaParser.Datamodel.Attribute attribute = new ModelicaParser.Datamodel.Attribute(type, name, maxMultiplicity, minMultiplicity);
                    attribute.ParentElement = record;
                    record.AddAttribute(attribute);
                }
                else
                {
                    string targetMultiplicity = "";
                    if(minMultiplicity == maxMultiplicity){
                        targetMultiplicity = minMultiplicity;
                    }else{
                        targetMultiplicity = minMultiplicity + ".." + maxMultiplicity;
                    }
                    Connector c = new Connector("Association", "1", targetMultiplicity, name);
                    c.ParentElement = record;
                    c.Source = record;
                    record.AddSourceConnector(c);
                    if (!targetElements.ContainsKey(type))
                    {
                        targetElements.Add(type, new List<Connector>());
                    }
                    targetElements[type].Add(c);
                }
            }
            //Console.WriteLine(record.Name + " (" + record.Attributes.Count+")");

            return record;
        }

        #endregion


    }
}
