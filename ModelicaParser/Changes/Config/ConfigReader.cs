using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace ModelicaChangeAnalyzer.Config
{
        // parser of the config XML file
        public static class ConfigReader
        {
            private static MainForm form;
            private static string resultsPath = "";
            private static List<string> excludedPackageNames = new List<string>();
            private static List<string> excludedConnectorTypes = new List<string>();
            private static List<string> excludedElementTypes = new List<string>();
            private static List<string[]> extractMultipleReleases = new List<string[]>();
            private static List<string> reportChangesReleases = new List<string>();
            private static string reportChangesPath = "";
            private static List<string> reportMetricsReleases = new List<string>();
            private static string reportMetricsPath = "";
            private static bool excludedElementNote = false;
            private static bool excludedAttributeNote = false;
            private static bool excludedConnectorNote = false;
            private static bool excludedCaseSensitivity = false;
            private static bool validates = true;
            private static string extractPath = "";

            #region Read

            // parsing the config XML file
            public static bool Read(string filePath, MainForm form)
            {
                // reseting everything in case a config file has been previously read
                ConfigReader.form = form;
                resultsPath = "";
                excludedPackageNames = new List<string>();
                excludedConnectorTypes = new List<string>();
                excludedElementTypes = new List<string>();
                extractMultipleReleases = new List<string[]>();
                reportChangesReleases = new List<string>();
                reportChangesPath = "";
                reportMetricsReleases = new List<string>();
                reportMetricsPath = "";
                excludedElementNote = false;
                excludedAttributeNote = false;
                excludedConnectorNote = false;
                excludedCaseSensitivity = false;
                //roles = new List<ARRole>();
                validates = true;

                // setting the schema validation settings
                XmlReaderSettings settings = new XmlReaderSettings();
                /*settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;     // schema defined in the "schemaLocation" attribute
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;*/

                // registering the validation error/warning event handler
                settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

                // creating the XML reader object.
                XmlReader reader = XmlReader.Create(filePath, settings);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "EXCLUDE-CASE-SENSITIVITY":
                                reader.Read();
                                if (reader.Value.Equals("true"))
                                    excludedCaseSensitivity = true;
                                else if (reader.Value.Equals("false"))
                                    excludedCaseSensitivity = false;
                                break;
                            case "PACKAGE-NAME":
                                reader.Read();
                                excludedPackageNames.Add(reader.Value);
                                break;
                            case "ELEMENT-TYPE":
                                reader.Read();
                                excludedElementTypes.Add(reader.Value);
                                break;
                            case "ELEMENT-NOTE":
                                reader.Read();
                                if (reader.Value.Equals("true"))
                                    excludedElementNote = true;
                                else if (reader.Value.Equals("false"))
                                    excludedElementNote = false;
                                break;
                            case "ATTRIBUTE-NOTE":
                                reader.Read();
                                if (reader.Value.Equals("true"))
                                    excludedAttributeNote = true;
                                else if (reader.Value.Equals("false"))
                                    excludedAttributeNote = false;
                                break;
                            case "CONNECTOR-TYPE":
                                reader.Read();
                                excludedConnectorTypes.Add(reader.Value);
                                break;
                            case "CONNECTOR-NOTE":
                                reader.Read();
                                if (reader.Value.Equals("true"))
                                    excludedConnectorNote = true;
                                else if (reader.Value.Equals("false"))
                                    excludedConnectorNote = false;
                                break;
                            case "ROLE":
                                XmlReader subTree = reader.ReadSubtree();   // reading sub-tags from the tag "ROLE"
                                break;
                            case "RESULTS_PATH":    // path used for generating reports from GUI analysis
                                reader.Read();
                                resultsPath = reader.Value;
                                break;
                            case "EXTRACT_MULTIPLE":
                                if (reader.GetAttribute("EXTRACT_PATH") != null)
                                    extractPath = reader.GetAttribute("EXTRACT_PATH");   // reading path to generated reports
                                subTree = reader.ReadSubtree();   // reading sub-tags from the tag "ROLE"

                                while (subTree.Read())
                                    if (subTree.NodeType == XmlNodeType.Element && subTree.Name.Equals("RELEASE"))
                                    {                                   // reading one release
                                        //read version
                                        string version = subTree.GetAttribute("VERSION");
                                        subTree.Read();
                                        extractMultipleReleases.Add(new string[] { subTree.Value, version });
                                    }

                                break;
                            case "REPORT_CHANGES":
                                if (reader.GetAttribute("REPORT_PATH") != null)
                                    reportChangesPath = reader.GetAttribute("REPORT_PATH");   // reading path to generated reports

                                subTree = reader.ReadSubtree();   // reading sub-tags from the tag "ROLE"

                                while (subTree.Read())
                                    if (subTree.NodeType == XmlNodeType.Element && subTree.Name.Equals("RELEASE"))
                                    {                                   // reading one release
                                        subTree.Read();
                                        reportChangesReleases.Add(subTree.Value);
                                    }

                                break;
                            case "REPORT_METRICS":
                                if (reader.GetAttribute("REPORT_PATH") != null)
                                    reportMetricsPath = reader.GetAttribute("REPORT_PATH");   // reading path to generated reports

                                subTree = reader.ReadSubtree();   // reading sub-tags from the tag "ROLE"

                                while (subTree.Read())
                                    if (subTree.NodeType == XmlNodeType.Element && subTree.Name.Equals("RELEASE"))
                                    {                                   // reading one release
                                        subTree.Read();
                                        reportMetricsReleases.Add(subTree.Value);
                                    }

                                break;
                        }
                    }
                }

                reader.Close();

                return validates;
            }

            // validation error/warning event handler 
            private static void ValidationCallBack(object sender, ValidationEventArgs args)
            {
                if (args.Severity == XmlSeverityType.Warning)
                    form.ListAdd("WARNING in line " + args.Exception.LineNumber + " :" + args.Message);
                else
                    form.ListAdd("ERROR in line " + args.Exception.LineNumber + " :" + args.Message);
                
                validates = false;
            }

            #endregion

            #region Getters

            public static string ResultsPath
            {
                get { return ConfigReader.resultsPath; }
            }

            public static string ExtractPath
            {
                get { return ConfigReader.extractPath; }
            }

            public static List<string> ExcludedPackageNames
            {
                get { return ConfigReader.excludedPackageNames; }
            }

            public static List<string> ExcludedConnectorTypes
            {
                get { return ConfigReader.excludedConnectorTypes; }
            }

            public static List<string> ExcludedElementTypes
            {
                get { return ConfigReader.excludedElementTypes; }
            }

            public static bool ExcludedElementNote
            {
                get { return ConfigReader.excludedElementNote; }
            }

            public static bool ExcludedAttributeNote
            {
                get { return ConfigReader.excludedAttributeNote; }
            }

            public static bool ExcludedConnectorNote
            {
                get { return ConfigReader.excludedConnectorNote; }
            }

            public static bool ExcludedCaseSensitivity
            {
                get { return ConfigReader.excludedCaseSensitivity; }
            }

            public static List<string[]> ExtractMultipleReleases
            {
                get { return ConfigReader.extractMultipleReleases; }
            }

            public static List<string> ReportChangesReleases
            {
                get { return ConfigReader.reportChangesReleases; }
            }

            public static List<string> ReportMetricsReleases
            {
                get { return ConfigReader.reportMetricsReleases; }
            }

            public static string ReportChangesPath
            {
                get { return ConfigReader.reportChangesPath; }
            }

            public static string ReportMetricsPath
            {
                get { return ConfigReader.reportMetricsPath; }
            }

            #endregion
        }
}