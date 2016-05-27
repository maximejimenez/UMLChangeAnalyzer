using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelicaParser.Datamodel;
using System.Xml;

namespace ModelicaParser.Parsers
{
    class XMLToDatamodel
    {
        static List<MetaModel> metamodels = new List<MetaModel>();
        //static Dictionary<string, >
        // Table for Element connect target
        static string[] Basetypes = new string[] { "Boolean", "Integer", "Real", "String" };

        static void Main(string[] args)
        {
            for (int i = 2; i <= 6; i++)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(@"C:\Users\maxime\Desktop\XmlModelica\Absyn-1.9." + i + ".xml");

                MetaModel metamodel = parseMetaModel(doc);
                metamodels.Add(metamodel);
                
                Console.WriteLine("Absyn-1.9." + i + " parsing to MetaDataModel sucessful");
            }
            Console.ReadKey();
        }

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
            XmlNodeList children = elem.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].Name == "uniontype")
                {
                    Element uniontype = parseUniontype(children[i]);
                    package.AddElement(uniontype);
                }
                else
                {
                    // type alias, might need to be handled
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
                uniontype.AddChildren(record);
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
                    record.AddAttribute(attribute);
                }
                else
                {
                    //TODO
                    //Element element = parseUniontype(children[i]);

                    Connector c = new Connector("Association", "1", minMultiplicity+".."+maxMultiplicity); //UID initialized
                    record.AddSourceConnector(c);
                    //go deeper ?
                    //record.AddElement(element);
                    // MAYBE store connection and do it at the end
                }
            }

            return record;
        }



    }
}
