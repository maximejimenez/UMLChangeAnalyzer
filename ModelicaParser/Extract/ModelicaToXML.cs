using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ModelicaChangeAnalyzer.Extract
{
    class ModelicaToXML
    {
        private const string SEMICOLON = ";";
        private const string UNIONTYPE = "uniontype";
        private const string UTID = "uniontypeID";
        private const string RECORD = "record";
        private const string RECORDID = "recordID";
        private const string EQUALS = "=";
        private const string PUBLIC = "public";
        private const string PROTECTED = "protected";
        private const string IMPORT = "import";
        private const string ENCAPSULATED = "encapsulated";
        private const string PACKAGE = "package";
        private const string TYPE = "type";
        private const string END = "end";
        private const string STRING = "String";
        private const string INTEGER = "Integer";
        private const string BOOLEAN = "Boolean";
        private const string SOURCEINFO = "SourceInfo";
        private const string UTYPE = "UndefinedType";
        private const string DTYPE = "DefinedType";
        private const string ID = "Identifier";
        private const string ALIAS = "alias";
        private const string LIST = "list";
        private const string ARRAY = "array";
        private const string TUPLE = "tuple";
        private const string OPTION = "Option";
        private const string FUNCTION = "function";
        private const string CONSTANT = "constant";

        private static XmlDocument doc;

        #region Public

        public ModelicaToXML() {}

        public void parse(string mmPath, string xmlPath, string version)
        {
            doc = new XmlDocument();
            XmlElement root = doc.CreateElement("metamodel");
            root.SetAttribute("version", version);
            doc.AppendChild(root);

            if(Directory.Exists(mmPath)){
                string[] paths = Directory.GetFiles(mmPath);
                foreach(string filePath in paths){
                    Console.WriteLine(filePath);
                    string text = File.ReadAllText(filePath, Encoding.UTF8);
                    IEnumerator<string> e = getTokens(text).GetEnumerator();
                    handlePackage(root, e);
                }
            }
            
            string xml = prettyXMLString(root);
            System.IO.File.WriteAllText(xmlPath, xml);
        }

        #endregion

        // Internal functions

        #region Utils

        private static string prettyXMLString(XmlElement dom)
        {
            string xml = dom.OuterXml;
            try
            {
                var str = XDocument.Parse(xml);
                xml = str.ToString();
            }
            catch (Exception) { }
            return xml;
        }

        private static List<string> getTokens(string text)
        {
            List<string> tokens = new List<string>();
            string[] WS = new string[] { " ", "\n", "\t", "\r" };

            text = removeSingleLineCommments(text);
            string[] textWithoutSpace = text.Split(WS, StringSplitOptions.RemoveEmptyEntries);
            tokens = splitSemicolonToken(textWithoutSpace);
            tokens = cleanMetaModelicaCode(tokens);

            string tmp = string.Join(" ", tokens.ToArray());
            string[] tokensArray = tmp.Split(WS, StringSplitOptions.RemoveEmptyEntries);
            tokens = handleMultipleDeclaration(tokensArray);
            tokens = noteAsSingleToken(new List<string>(tokens));

            return tokens;
        }

        private static string removeSingleLineCommments(string text)
        {
            Boolean comments = false;
            string text2 = "";
            for (int i = 0; i < text.Length - 1; i++)
            {
                if (text[i] == '/' && text[i + 1] == '/')
                {
                    comments = true;
                }
                if (!comments)
                {
                    text2 += text[i];
                }
                if (text[i] == '\n')
                {
                    comments = false;
                }
            }
            return text2;
        }

        private static List<string> splitSemicolonToken(string[] textWithoutSpace)
        {
            List<string> tokens = new List<string>();
            foreach (string element in textWithoutSpace)
            {
                if (element.EndsWith(";"))
                {
                    tokens.Add(element.Substring(0, element.Length - 1));
                    tokens.Add(";");
                }
                else
                {
                    tokens.Add(element);
                }
            }
            return tokens;
        }

        private static List<string> cleanMetaModelicaCode(List<string> tokens)
        {
            List<string> tokenWithoutComments = new List<string>();
            Boolean comments = false;
            foreach (string token in tokens)
            {
                if (token.StartsWith("/*"))
                {
                    comments = true;
                }
                if (!comments)
                {
                    tokenWithoutComments.Add(token);
                }
                if (token.EndsWith("*/"))
                {
                    comments = false;
                }
            }
            return tokenWithoutComments;
        }

        private static List<string> handleMultipleDeclaration(string[] tokensArray)
        {
            List<string> tokens = new List<string>();
            for (int i = 0; i < tokensArray.Length; i++)
            {
                if (!tokensArray[i].StartsWith("list") && !tokensArray[i].StartsWith("tuple") && tokensArray[i].EndsWith(","))
                {
                    tokens.Add(tokensArray[i].Substring(0, tokensArray[i].Length - 1));
                    tokens.Add(";");
                    tokens.Add(tokensArray[i - 1]);
                }
                else
                {
                    tokens.Add(tokensArray[i]);
                }
            }

            return tokens;
        }

        private static List<string> noteAsSingleToken(List<string> tokens)
        {
            List<string> result = new List<string>();
            bool comment = false;
            string newToken = "";
            foreach (string token in tokens)
            {
                if (comment)
                {
                    if (token.EndsWith("\""))
                    {
                        comment = false;
                        newToken += token;
                        result.Add(newToken);
            }
                    else
            {
                        newToken += token;
            }
                }
                else
            {
                    if ((token.StartsWith("\"") && token.EndsWith("\"") && token.Length > 1 )|| !token.StartsWith("\""))
                    {
                        result.Add(token);
            }
                    else
            {
                        comment = true;
                        newToken = token;
                    }
                }
            }

            return result;
        }

        #endregion

        #region DOMHelpers

        private static XmlElement createElementWithID(IEnumerator<string> e)
        {
            XmlElement elem = doc.CreateElement(e.Current);
            e.MoveNext();
            elem.SetAttribute("id", e.Current);
            return elem;
            }

        private static XmlElement createElementWithName(IEnumerator<string> e)
            {
            XmlElement elem = doc.CreateElement(e.Current);
            e.MoveNext();
            elem.SetAttribute("name", e.Current);
            return elem;
        }

        #endregion

        #region Handlers

        private static void handlePackage(XmlElement parent, IEnumerator<string> e)
        {
            Boolean encapsulated = false;
                e.MoveNext();
            if (e.Current == ENCAPSULATED)
            {
                encapsulated = true;
                e.MoveNext();
            }
            if (e.Current == PACKAGE)
            {
                XmlElement package = createElementWithID(e);
                parent.AppendChild(package);
                if (encapsulated)
                    package.SetAttribute(ENCAPSULATED, "true");
                e.MoveNext();
                if (e.Current.StartsWith("\"") && e.Current.EndsWith("\""))
                {
                    package.SetAttribute("note", e.Current.Substring(1, e.Current.Length - 2));
                    e.MoveNext();
                }
                handleInsidePackage(package, e, "");
            }
            else
            {
                Console.WriteLine("Package not handled");
            }
        }

        private static void handleInsidePackage(XmlElement parent, IEnumerator<string> e, string visibility)
        {
            do
            {
                if (e.Current == PUBLIC)
                {
                    visibility = PUBLIC;
                e.MoveNext();
                    handleInsidePackage(parent, e, visibility);
                }
                else if (e.Current == PROTECTED)
                {
                    visibility = PROTECTED;
                e.MoveNext();
                    handleInsidePackage(parent, e, visibility);
                }
                else if (e.Current == IMPORT)
                {
                    handleImport(parent, e, visibility);
                }
                else if (e.Current == TYPE)
                {
                    handleType(parent, e, visibility);
                }
                else if (e.Current == UNIONTYPE)
                {
                    handleUniontype(parent, e, visibility);
                }
                else if (e.Current == FUNCTION)
                {
                    handleFunction(parent, e, visibility);
                }
                else if (e.Current == CONSTANT)
                {
                    handleConstant(parent, e, visibility);
            }
                /*else
            {
                    Console.WriteLine("Unexpected token : " + e.Current);
                }*/
            } while (e.MoveNext() && e.Current != END);
        }

        private static void handleImport(XmlElement parent, IEnumerator<string> e, string visibility)
                {
            XmlElement import = createElementWithID(e);
            if (visibility != "")
                import.SetAttribute("visibility", visibility);
            parent.AppendChild(import);
            e.MoveNext();
                }

        private static void handleType(XmlElement parent, IEnumerator<string> e, string visibility)
        {
            XmlElement type = createElementWithName(e);
            if (visibility != "")
                type.SetAttribute("visibility", visibility);
            parent.AppendChild(type);
            e.MoveNext();
            e.MoveNext();
            type.SetAttribute("aliasFor", e.Current);
            e.MoveNext();
            if (e.Current.StartsWith("\"") && e.Current.EndsWith("\""))
            {
                type.SetAttribute("note", e.Current.Substring(1, e.Current.Length - 2));
                e.MoveNext();
            }
        }

        private static void handleUniontype(XmlElement parent, IEnumerator<string> e, string visibility)
        {
            XmlElement uniontype = createElementWithID(e);
            parent.AppendChild(uniontype);
            if (visibility != "")
                uniontype.SetAttribute("visibility", visibility);
                    e.MoveNext();
            if (e.Current.StartsWith("\"") && e.Current.EndsWith("\""))
            {
                uniontype.SetAttribute("note", e.Current.Substring(1, e.Current.Length - 2));
                    e.MoveNext();
                }
            do
            {
                handleRecord(uniontype, e);
                e.MoveNext();
            } while (e.Current != END);
            e.MoveNext();
                e.MoveNext();
            }

        private static void handleRecord(XmlElement parent, IEnumerator<string> e)
        {
            XmlElement record = createElementWithID(e);
            parent.AppendChild(record);
            e.MoveNext();
            if (e.Current.StartsWith("\"") && e.Current.EndsWith("\""))
            {
                record.SetAttribute("note", e.Current.Substring(1, e.Current.Length - 2));
                e.MoveNext();
            }

            if(e.Current != END){
                do
            {
                    handleField(record, e);
                    e.MoveNext();
                } while (e.Current != END);
            }
            e.MoveNext();
            e.MoveNext();
            }

        private static void handleField(XmlElement parent, IEnumerator<string> e)
        {
            XmlElement field = doc.CreateElement("field");
            handleFieldType(field, e);
            e.MoveNext();
            field.SetAttribute("name", e.Current);
            e.MoveNext();
            if (e.Current.StartsWith("\"") && e.Current.EndsWith("\""))
            {
                field.SetAttribute("note", e.Current.Substring(1, e.Current.Length - 2));
                e.MoveNext();
            }else if(e.Current == "="){
                e.MoveNext();
                field.SetAttribute("value", e.Current);
                e.MoveNext();
            }
            parent.AppendChild(field);
            }

        private static void handleFunction(XmlElement parent, IEnumerator<string> e, string visibility)
            {
            XmlElement function = createElementWithID(e);
            if (visibility != "")
                function.SetAttribute("visibility", visibility);
            parent.AppendChild(function);
            string functionName = e.Current;
            while (e.MoveNext() && e.Current != functionName) ; // Skip Function
                e.MoveNext();
        }

        private static void handleConstant(XmlElement parent, IEnumerator<string> e, string visibility)
        {
            XmlElement constant = doc.CreateElement(CONSTANT);
            if (visibility != "")
                constant.SetAttribute("visibility", visibility);
            parent.AppendChild(constant);
            e.MoveNext();
            constant.SetAttribute("type", e.Current);
            e.MoveNext();
            constant.SetAttribute("name", e.Current);
            e.MoveNext();
            e.MoveNext();
            constant.SetAttribute("value", e.Current);
            e.MoveNext();
            // TODO : Maybe note to handle
        }

        private static void handleFieldType(XmlElement field, IEnumerator<string> e)
        {
            if (e.Current.StartsWith(OPTION))
            {
                field.SetAttribute("minMultiplicity", "0");
                field.SetAttribute("maxMultiplicity", "1");
                contentTypeOption(field, e);

            }
            else if (e.Current.StartsWith(ARRAY))
            {
                field.SetAttribute("minMultiplicity", "0");
                field.SetAttribute("maxMultiplicity", "*");
                contentTypeArray(field, e);
            }
            else if (e.Current.StartsWith(LIST))
            {
                field.SetAttribute("minMultiplicity", "0");
                field.SetAttribute("maxMultiplicity", "*");
                contentTypeList(field, e);
            }
            else if (e.Current.StartsWith(TUPLE))
            {
                field.SetAttribute("minMultiplicity", "0");
                field.SetAttribute("maxMultiplicity", "*");
                contentTypeTuple(field, e);
            }
            else
            {
                field.SetAttribute("type", e.Current);
                field.SetAttribute("minMultiplicity", "1");
                field.SetAttribute("maxMultiplicity", "1");
            }
        }

        private static void contentTypeArray(XmlElement field, IEnumerator<string> e)
        {
            if (e.Current.EndsWith(">"))
            {
                string contentType = e.Current.Substring(6, e.Current.Length - 7);
                field.SetAttribute("type", contentType);
            }
            else
            {
                string contentType = e.Current.Substring(6);
                e.MoveNext();
                contentType += e.Current;
                field.SetAttribute("type", contentType);
            }
        }

        private static void contentTypeList(XmlElement field, IEnumerator<string> e)
        {
            if (e.Current.EndsWith(">"))
            {
                string contentType = e.Current.Substring(5, e.Current.Length - 6);
                field.SetAttribute("type", contentType);
            }
            else
            {
                string contentType = e.Current.Substring(5);
                e.MoveNext();
                contentType += e.Current;
                field.SetAttribute("type", contentType);
            }
        }


        private static void contentTypeTuple(XmlElement field, IEnumerator<string> e)
        {
            if (e.Current.EndsWith(">"))
            {
                string contentType = e.Current.Substring(6, e.Current.Length - 7);
                field.SetAttribute("type", contentType);
            }
            else
            {
                string contentType = e.Current.Substring(6);
                e.MoveNext();
                contentType += e.Current;
                field.SetAttribute("type", contentType);
            }

        }

        private static void contentTypeOption(XmlElement field, IEnumerator<string> e)
        {
            if (e.Current.EndsWith(">"))
            {
                string contentType = e.Current.Substring(7, e.Current.Length - 8);
                field.SetAttribute("type", contentType);
            }
            else
            {
                string contentType = e.Current.Substring(7);
                e.MoveNext();
                contentType += e.Current;
                field.SetAttribute("type", contentType);
            }
        }

        #endregion

    }
}
