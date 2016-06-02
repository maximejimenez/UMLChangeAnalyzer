using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ModelicaParser
{
    class ModelicaToXML
    {
        public const string SEMICOLON = ";";
        public const string UNIONTYPE = "uniontype";
        public const string UTID = "uniontypeID";
        public const string RECORD = "record";
        public const string RECORDID = "recordID";
        public const string EQUALS = "=";
        public const string PUBLIC = "public";
        public const string PROTECTED = "protected";
        public const string IMPORT = "import";
        public const string ENCAPSULATED = "encapsulated";
        public const string PACKAGE = "package";
        public const string TYPE = "type";
        public const string END = "end";
        public const string STRING = "String";
        public const string INTEGER = "Integer";
        public const string BOOLEAN = "Boolean";
        public const string SOURCEINFO = "SourceInfo";
        public const string UTYPE = "UndefinedType";
        public const string DTYPE = "DefinedType";
        public const string ID = "Identifier";
        public const string ALIAS = "alias";
        public const string LIST = "list";
        public const string OPTION = "Option";
        public static XmlDocument doc;

        static void Main(string[] args)
        {
            for (int i = 2; i <= 6; i++)
            {
                doc = new XmlDocument();
                string text = File.ReadAllText(@"C:\Users\maxime\Desktop\Modelica OMCompiler\OMCompiler-1.9."+ i +@"\Compiler\FrontEnd\Absyn.mo", Encoding.UTF8);
                List<string> tokens = getTokens(text);
                IEnumerator<string> e = tokens.GetEnumerator();

                XmlElement root = getXMLFromTokens(e, "1.9."+i);
                string xml = prettyXMLString(root);
                System.IO.File.WriteAllText(@"C:\Users\maxime\Desktop\TryXML\Absyn-1.9." + i + ".xml", xml);
                Console.WriteLine("XML export of version 1.9."+ i +" sucessful");
            }
        }

        private static XmlElement getXMLFromTokens(IEnumerator<string> e, string version)
        {
            XmlElement root = doc.CreateElement("metamodel");
            root.SetAttribute("version", version);
            doc.AppendChild(root);
            while (e.MoveNext())
            {
                convertToXML(root, e);
            }
            return root;
        }

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

            //foreach (string token in tokens)
            //{
            //    Console.WriteLine(token);
            //    Console.ReadKey();
            //}

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
                    if (text.Substring(i).StartsWith("/* \"From here down, only Absyn helper functions should be present."))
                    {
                        break;
                    }
                    text2 += text[i];
                }
                if (text[i] == '\n')
                {
                    comments = false;
                }
            }
            text2 += "end Absyn;";
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
                if (token == "/*")
                {
                    comments = true;
                }
                if (!comments)
                {
                    tokenWithoutComments.Add(token);
                }
                if (token == "*/")
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
                if (!tokensArray[i].StartsWith("list") && tokensArray[i].EndsWith(","))
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

            foreach (string token in result)
            {
                Console.WriteLine(token);
                Console.ReadKey();
            }

            return result;
        }

        private static String retrieveNote(IEnumerator<string> e)
        {
            e.MoveNext();
            if (e.Current.StartsWith("\"") && e.Current.EndsWith("\""))
            {
                return e.Current;
            }
            return null;
        }



        private static void convertToXML(XmlElement parent, IEnumerator<string> e)
        {
            XmlElement elem;
            string token = e.Current;
            if (token == ENCAPSULATED){
                // TODO maybe ignore
            }
            else if (token == PUBLIC)
            {
                //TODO
            }
            else if (token == PROTECTED)
            {
                // DO NOTHING
            }
            else if (token == IMPORT)
            {
                e.MoveNext();
            }
            else if (token == TYPE)
            {
                elem = doc.CreateElement("type");
                parent.AppendChild(elem);
                e.MoveNext();
                elem.SetAttribute("name", e.Current);
                e.MoveNext();
                e.MoveNext();
                elem.SetAttribute("aliasFor", e.Current);
                String note = retrieveNote(e);
                if (note != null)
                {
                    elem.SetAttribute("note", note);
                }
            }
            else if ((token == PACKAGE))
            {
                elem = createElementWithID(e);
                parent.AppendChild(elem);
                String note = retrieveNote(e);
                if (note != null)
                {
                    elem.SetAttribute("note", note);
                }

                while (e.MoveNext() && e.Current != END)
                {
                    convertToXML(elem, e);
                }

                e.MoveNext();
                e.MoveNext();
            }
            else if(token == UNIONTYPE)
            {
                elem = createElementWithID(e);
                parent.AppendChild(elem);
                String note = retrieveNote(e);
                if (note != null)
                {
                    elem.SetAttribute("note", note);
                }
                while (e.MoveNext() && e.Current != END)
                {
                    convertToXML(elem, e);
                }

                e.MoveNext();
            }else if((token == RECORD)){
                elem = createElementWithID(e);
                parent.AppendChild(elem);
                String note = retrieveNote(e);
                if (note != null)
                {
                    elem.SetAttribute("note", note);
                }
                while(e.MoveNext() && e.Current != END){
                    XmlElement field = doc.CreateElement("field");
                    handleType(field, e);
                    e.MoveNext();
                    field.SetAttribute("name", e.Current);
                    e.MoveNext();
                    elem.AppendChild(field);
                }

                e.MoveNext();
            }

            else if (token == END)
            {
                e.MoveNext();
            }
            else if (token == SEMICOLON)
            {
                //DO NOTHING
            }
            else if (token.StartsWith(OPTION))
            {
                //TODO
            }
            else if(token.StartsWith(LIST)){
                //TODO
            }
            else
            {
                string tkn = e.Current;
                e.MoveNext();

                string xml = doc.OuterXml;
                System.IO.File.WriteAllText(@"C:\Users\maxime\Desktop\TryXML\Log.xml", xml);
                throw new Exception("Unexpected token : " + tkn + "(" + e.Current + ")");
            }
        }

        private static XmlElement createElementWithID(IEnumerator<string> e)
        {
            XmlElement elem = doc.CreateElement(e.Current);
            e.MoveNext();
            elem.SetAttribute("id", e.Current);
            return elem;
        }

        private static void handleType(XmlElement field, IEnumerator<string> e)
        {
            if (e.Current.StartsWith(OPTION))
            {
                field.SetAttribute("minMultiplicity", "0");
                field.SetAttribute("maxMultiplicity", "1");
                contentTypeOption(field, e);

            }
            else if (e.Current.StartsWith(LIST))
            {
                field.SetAttribute("minMultiplicity", "0");
                field.SetAttribute("maxMultiplicity", "*");
                contentTypeList(field, e);
            }
            else
            {
                field.SetAttribute("type", e.Current);
                field.SetAttribute("minMultiplicity", "1");
                field.SetAttribute("maxMultiplicity", "1");
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
    }
}
