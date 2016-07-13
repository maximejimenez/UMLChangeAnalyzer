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

        private static int numberOfAssociation = 0;
        private static int numberOfConnector = 0;
        private static int numberOfAttribute = 0;

        public Extractor(MainForm mainForm)
        {
            this.mainForm = mainForm;
        }

        public static MetaModel XMLtoMetamodel(string p)
        {
            numberOfAssociation = 0;
            numberOfAttribute = 0;
            numberOfConnector = 0;
            MetaModel metamodel = null;
            doc = new XmlDocument();
            primitiveTypes = new Dictionary<String, String>();
            declaredConnector = new Dictionary<String, Connector>();

            if (Directory.Exists(p))
            {
                string[] paths = Directory.GetFiles(p);
                string path = paths[0];
                doc.Load(path);

                XmlNode cmofPackageNode = doc.GetElementsByTagName("cmof:Package").Item(0);
                extractPrimitiveTypes(cmofPackageNode);

                /*for (int i = 1; i < paths.Length; i++)
                {
                    path = paths[i];
                    doc.Load(path);
                    cmofPackageNode = doc.GetElementsByTagName("cmof:Package").Item(0);
                    package = parseCmofPackage(cmofPackageNode);
                    metamodel.AddPackage(package);
                }*/
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
                
                /*for (int i = 1; i < paths.Length; i++)
                {
                    path = paths[i];
                    doc.Load(path);
                    cmofPackageNode = doc.GetElementsByTagName("cmof:Package").Item(0);
                    package = parseCmofPackage(cmofPackageNode);
                    metamodel.AddPackage(package);
                }*/
            }

            //TODO OwnedEnd

            Console.WriteLine("numberOfConnector = " + numberOfConnector);
            Console.WriteLine("numberOfAttribute = " + numberOfAttribute);
            Console.WriteLine("numberOfAssociation (package) = " + numberOfAssociation);

            Console.WriteLine("Declared connectors with a missing end :");
            foreach (string key in declaredConnector.Keys)
            {
                Console.WriteLine("\t " + key);
            }
            Console.WriteLine("TOTAL = " + declaredConnector.Count);

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
            return cmofPackage;
        }

        static Package parsePackage(XmlNode packageNode)
        {
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
                        package.AddSubPackage(sub);
                        break;
                    case "cmof:Enumeration":
                    case "cmof:PrimitiveType":
                        /*String primitiveTypeId = child.Attributes["xmi:id"].Value;
                        String primitiveTypeName = child.Attributes["name"].Value;
                        primitiveTypes.Add(primitiveTypeId, primitiveTypeName);*/
                        break;
                    case "cmof:Class":
                        Element element = parseElement(child);
                        package.AddElement(element);
                        break;
                    case "cmof:PackageImport":
                        break;

                    case "cmof:Association":
                        numberOfAssociation++;
                        /*String id = child.Attributes["xmi:id"].Value;
                        associationInPackage.Add(id);*/
                        /*if(!propertyWithAssociation.Contains(id)){
                            Console.WriteLine("Association not extract ID" + id);
                        }*/
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
                numberOfConnector++;
            }
            else if(primitiveTypes.ContainsKey(typeIdentifier))
            {
                UMLChangeAnalyzer.Datamodel.Attribute attr = new UMLChangeAnalyzer.Datamodel.Attribute(uid, type, name, upperBound, lowerBound, note);
                parent.AddAttribute(attr);
                numberOfAttribute++;
            }
            else
            {
                Console.WriteLine("\t \t Primitive type expected but : " + type);
            }
        }

        static String parseComment(XmlNode commentNode)
        {
            XmlNodeList children = commentNode.ChildNodes;
            String comment = "";
            for (int i = 0; i < children.Count; i++)
            {
                XmlNode child = children.Item(i);
                String tagName = child.Name;
                switch (tagName)
                {
                    case "body":
                        comment = child.Value;
                        break;
                    default:
                        Console.WriteLine("\t Unexpected : " + tagName + " (" + child.Name + ")");
                        break;
                }
            }
            return comment;
        }

        #endregion


    }
}
