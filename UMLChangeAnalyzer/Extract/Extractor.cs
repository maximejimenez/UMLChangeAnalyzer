using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMLChangeAnalyzer.Datamodel;
using System.Xml;
using System.IO;

namespace UMLChangeAnalyzer.Extract
{
    class Extractor
    {
        private MainForm mainForm;
        private static XmlDocument doc;
        private static Dictionary<String, String> primitiveTypes;
        private static Dictionary<String, Connector> declaredConnector;
        private static Dictionary<String, XmlNode> associationWithOwnedEnd;

        public Extractor(MainForm mainForm)
        {
            this.mainForm = mainForm;
        }

        public static MetaModel XMLtoMetamodel(string p)
        {
            MetaModel metamodel = null;
            doc = new XmlDocument();
            primitiveTypes = new Dictionary<String, String>();
            declaredConnector = new Dictionary<String, Connector>();
            associationWithOwnedEnd = new Dictionary<string, XmlNode>();

            if (Directory.Exists(p))
            {
                string[] paths = Directory.GetFiles(p);

                for (int i = 0; i < paths.Length; i++)
                {
                    string path = paths[i];
                    doc.Load(path);
                    XmlNode cmofPackageNode = doc.GetElementsByTagName("cmof:Package").Item(0);
                    extractPrimitiveTypes(cmofPackageNode);
                }
            }

            if (Directory.Exists(p))
            {
                string[] paths = Directory.GetFiles(p);
                string path = paths[0];
                doc.Load(path);

                XmlNode metamodelNode = doc.GetElementsByTagName("xmi:XMI").Item(0);
                string version = metamodelNode.Attributes["xmi:version"].Value;
                metamodel = new MetaModel(version);


                XmlNode cmofPackageNode = doc.GetElementsByTagName("cmof:Package").Item(0);
                Package package = parseCmofPackage(cmofPackageNode);
                metamodel.AddPackage(package);
                
                for (int i = 1; i < paths.Length; i++)
                {
                    path = paths[i];
                    
                    doc.Load(path);
                    Console.WriteLine(path);
                    cmofPackageNode = doc.GetElementsByTagName("cmof:Package").Item(0);
                    package = parseCmofPackage(cmofPackageNode);
                    metamodel.AddPackage(package);
                }
            }

            Console.WriteLine("OwnedEnd left : ");
            foreach (string id in associationWithOwnedEnd.Keys)
            {
                Console.WriteLine("\t" + id);
            }

            Console.WriteLine("Declared connectors with a missing end :");
            foreach (string key in declaredConnector.Keys)
            {
                Console.WriteLine("\t " + key);
            }

            return metamodel;
        }

        #region Type parsers

        static void extractPrimitiveTypes(XmlNode packageNode)
        {
            XmlNodeList subNodes = packageNode.ChildNodes;
            for (int i = 0; i < subNodes.Count; i++)
            {
                XmlNode child = subNodes.Item(i);
                String type = child.Attributes["xmi:type"].Value;

                if (type == "cmof:PrimitiveType" || type == "cmof:Enumeration")
                {
                    String primitiveTypeId = child.Attributes["xmi:id"].Value;
                    String primitiveTypeName = child.Attributes["name"].Value;
                    primitiveTypes.Add(primitiveTypeId, primitiveTypeName);
                }else if(type == "cmof:Package"){
                    extractPrimitiveTypes(child);
                }
            }
        }

        static Package parseCmofPackage(XmlNode cmofPackageNode)
        {
            String uid = cmofPackageNode.Attributes["xmi:id"].Value;
            String name = cmofPackageNode.Attributes["name"].Value;
            Package cmofPackage = new Package(uid, name);
            //create package and add it to mm
            XmlNodeList cmofSubPackages = cmofPackageNode.ChildNodes;
            for (int i = 0; i < cmofSubPackages.Count; i++)
            {
                XmlNode cmofSubPackage = cmofSubPackages.Item(i);
                Package subPack = parsePackage(cmofSubPackage);
                cmofPackage.AddSubPackage(subPack);
            }

            //test
            foreach (string id in associationWithOwnedEnd.Keys.ToList())
            {
                Connector conn;
                if (declaredConnector.TryGetValue(id, out conn))
                {
                    //retrieve cardinality
                    XmlNode node = associationWithOwnedEnd[id];
                    string lower = "1";
                    string upper = "1";

                    if (node.Attributes["lower"] != null)
                        lower = node.Attributes["lower"].Value;
                    if (node.Attributes["upper"] != null)
                        upper = node.Attributes["upper"].Value;
                    string cardinality = lower + ".." + upper;
                    if (lower == upper)
                        cardinality = "" + lower;

                    conn.TargetCardinality = cardinality;
                    declaredConnector.Remove(id);
                    associationWithOwnedEnd.Remove(id);
                }
            }

            return cmofPackage;
        }

        static Package parsePackage(XmlNode packageNode)
        {
            if (packageNode.Name == "packageMerge")
                return null;
            String uid = packageNode.Attributes["xmi:id"].Value;
            String name = packageNode.Attributes["name"].Value;
            Package package = new Package(uid, name);
            XmlNodeList children = packageNode.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                XmlNode child = children.Item(i);
                String type = child.Attributes["xmi:type"].Value;
                switch (type)
                {
                    case "cmof:Package":
                        Package sub = parsePackage(child);
                        sub.ParentPackage = package;
                        package.AddSubPackage(sub);
                        break;
                    case "cmof:Class":
                        Element element = parseElement(child);
                        element.ParentPackage = package;
                        package.AddElement(element);
                        break;
                    case "cmof:PackageImport":
                    case "cmof:Enumeration":
                    case "cmof:PrimitiveType":
                        break;
                    case "cmof:Association":
                        if (child.ChildNodes.Count != 0 && child.ChildNodes.Item(0).Name == "ownedEnd")
                        {
                            String associationId = child.Attributes["xmi:id"].Value;
                            associationWithOwnedEnd.Add(associationId, child.ChildNodes.Item(0));
                        }
                        else if (child.ChildNodes.Count != 0)
                        {
                            Console.WriteLine("unexpected child for Association : " + child.ChildNodes.Item(0).Name);
                        }
                        break;
                    default:
                        Console.WriteLine("\t Unexpected : " + type + " (" + child.Name + ")");
                        break;
                }
            }

            return package;
        }

        static Element parseElement(XmlNode elementNode)
        {
            String uid = elementNode.Attributes["xmi:id"].Value;
            String type = elementNode.Attributes["xmi:type"].Value;
            String name = elementNode.Attributes["name"].Value;
            Element element = new Element(uid, type, name);
            //Console.WriteLine("Element " + uid);
            XmlNodeList children = elementNode.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                XmlNode child = children.Item(i);
                type = child.Attributes["xmi:type"].Value;
                switch (type)
                {
                    case "cmof:Comment":
                        string note = parseComment(child);
                        element.Note = note;
                        break;
                    case "cmof:Property":
                        parseProperty(element, child);
                        break;
                    case "cmof:Operation":
                    case "cmof:Constraint":

                        break;
                    default:
                        Console.WriteLine("\t \t Unexpected : " + type + " (" + child.Name + ")");
                        break;
                }
            }
            return element;
        }

        static void parseProperty(Element parent, XmlNode attrNode)
        {
            string uid = attrNode.Attributes["xmi:id"].Value;
            string name = attrNode.Attributes["name"].Value;
            string type = "association";
            string lowerBound = "1";
            string upperBound = "1";
            string note = "";
            string typeIdentifier = attrNode.Attributes["type"].Value;

            if (attrNode.Attributes["lower"] != null)
                lowerBound = attrNode.Attributes["lower"].Value;
            if (attrNode.Attributes["upper"] != null)
                upperBound = attrNode.Attributes["upper"].Value;

            XmlNodeList children = attrNode.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                XmlNode child = children.Item(i);
                if (child.Attributes["xmi:type"].Value == "cmof:Comment")
                    note = parseComment(child);
            }


            XmlAttribute association = attrNode.Attributes["association"];
            if (association != null)
            {
                Connector conn;

                string cardinality = lowerBound + ".." + upperBound;
                if (lowerBound == upperBound)
                    cardinality = "" + lowerBound;

                if (declaredConnector.TryGetValue(association.Value, out conn))
                {
                    //Source
                    conn.SourceCardinality = cardinality;
                    conn.Target = parent;
                    Connector clone = (Connector) conn.Clone();
                    parent.AddTargetConnector(clone);
                    declaredConnector.Remove(association.Value);
                }
                else{
                    //Target
                    conn = new Connector(type, "1", cardinality, uid, note);
                    conn.ParentElement = parent;
                    conn.Source = parent;
                    parent.AddSourceConnector(conn);
                    declaredConnector.Add(association.Value, conn);
                }
            }
            else if(primitiveTypes.ContainsKey(typeIdentifier))
            {
                UMLChangeAnalyzer.Datamodel.Attribute attr = new UMLChangeAnalyzer.Datamodel.Attribute(uid, type, name, upperBound, lowerBound, note);
                attr.ParentElement = parent;
                parent.AddAttribute(attr);
            }
            else
            {
                Console.WriteLine("\t \t Primitive type expected but : " + type);
            }
        }

        static String parseComment(XmlNode commentNode)
        {
            XmlNodeList children = commentNode.ChildNodes;
            XmlNode child = children.Item(0);
            String comment = child.InnerText;

            return comment;
        }

        #endregion


    }
}
