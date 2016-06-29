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
        private static string currentPackage = ""; // Help for backtracking
        static Dictionary<string, List<Connector>> targetElements;
        static Dictionary<string, Element> declaredElements;
        static string[] Basetypes = new string[] { "Boolean", "Integer", "Real", "String" };

        public Extractor(MainForm mainForm)
        {
            this.mainForm = mainForm;
        }

        internal void ExtractModel(string p1, string p2, string version)
        {
            ModelicaToXML toXML = new ModelicaToXML();
            toXML.parse(p1, p2, version);
        }

        internal void ReleaseModel()
        {
            // TODO
            //throw new NotImplementedException();
        }

        internal static MetaModel XMLtoMetamodel(string p)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(p);
            targetElements = new Dictionary<string, List<Connector>>();
            declaredElements = new Dictionary<string, Element>();
            currentPackage = "";
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
                metamodel.AddPackage(package);
            }

            Dictionary<string, List<Connector>>.KeyCollection targetsName = targetElements.Keys;
            foreach (string targetName in targetsName)
            {
                Element target = null;
                if (declaredElements.TryGetValue(targetName, out target))
                {
                    //Console.WriteLine(target.GetPath() + " = " + targetName);
                    foreach (Connector connector in targetElements[targetName])
                    {
                        connector.Target = target;
                        Connector clone = (Connector)connector.Clone();
                        clone.ParentElement = target;
                        clone.Target = target;
                        clone.UID = connector.ParentElement.Name + "." + clone.UID + "R";                         // in order to not get the wrong connector
                        target.AddTargetConnector(clone);
                    }
                }
                else
                {
                    //Console.WriteLine(version + " : *** WARNING *** \t Can't find type : " + targetName);
                }
            }
            return metamodel;
        }
        static Package parsePackage(XmlNode elem)
        {
            string id = elem.Attributes["id"].Value;
            currentPackage = id;
            Package package = new Package(id);
            XmlAttribute noteAttribute = elem.Attributes["note"];
            if (noteAttribute != null)
                package.Note = noteAttribute.Value;

            XmlNodeList children = elem.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].Name == "uniontype")
                {
                    Element uniontype = parseUniontype(children[i], package);
                    uniontype.ParentPackage = package;
                    package.AddElement(uniontype);
                    declaredElements.Add(id+"."+uniontype.Name, uniontype);
                }
                else if (children[i].Name == "function")
                { // To handle connectors refering to functions
                    Element function = new Element("function", children[i].Attributes["id"].Value);
                    declaredElements.Add(id + "." + children[i].Attributes["id"].Value, function);
                }
                else
                {
                    // TODO : handle type alias (possibly in ModelicaToXML)
                }
            }

            return package;
        }

        static Element parseUniontype(XmlNode elem, Package package)
        {
            string id = elem.Attributes["id"].Value;
            Element uniontype = new Element("uniontype", id);
            XmlAttribute noteAttribute = elem.Attributes["note"];
            if (noteAttribute != null)
                uniontype.Note = noteAttribute.Value;

            XmlNodeList children = elem.ChildNodes;

            for (int i = 0; i < children.Count; i++)
            {
                Element record = parseRecord(children[i]);
                record.ParentElement = uniontype;
                //Fixing elements 
                //uniontype.AddChild(record);
                package.AddElement(record);
            }

            return uniontype;
        }
        static Element parseRecord(XmlNode elem)
        {
            string id = elem.Attributes["id"].Value;
            Element record = new Element("record", id);
            XmlAttribute noteAttribute = elem.Attributes["note"];
            if (noteAttribute != null)
                record.Note = noteAttribute.Value;

            XmlNodeList children = elem.ChildNodes;

            for (int i = 0; i < children.Count; i++)
            {
                XmlAttributeCollection attributes = children[i].Attributes;
                string type = attributes["type"].Value;
                string name = attributes["name"].Value;
                string maxMultiplicity = attributes["maxMultiplicity"].Value;
                string minMultiplicity = attributes["minMultiplicity"].Value;
                XmlAttribute fieldAttributeNote = attributes["note"];

                if (Basetypes.Contains<string>(type))
                {
                    ModelicaChangeAnalyzer.Datamodel.Attribute attribute = new ModelicaChangeAnalyzer.Datamodel.Attribute(type, name, maxMultiplicity, minMultiplicity);
                    if (fieldAttributeNote != null)
                        attribute.Note = fieldAttributeNote.Value;
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
                    if (fieldAttributeNote != null)
                        c.Note = fieldAttributeNote.Value;
                    c.ParentElement = record;
                    c.Source = record;
                    record.AddSourceConnector(c);
                    if (!type.Contains("."))
                    {
                        type = currentPackage + "." + type;
                    }
                    if (!targetElements.ContainsKey(type))
                    {
                        targetElements.Add(type, new List<Connector>());
                    }
                    targetElements[type].Add(c);
                }
            }

            return record;
        }

        #endregion


    }
}
