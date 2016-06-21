using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ModelicaParser.Datamodel;
using ModelicaParser.Config;
using ModelicaParser.Extract;

namespace ModelicaParser.Changes
{
    // creates a report with metrics results for multiple AUTOSAR meta-model releases
    public class ReportMetrics
    {
        private MainForm form;
        private MetaModel model = null;
        private bool relevancy;                     // based on the selected checkbox in the GUI
        private string[] releases;                  // paths to all extracted releases
        private string reportPath;                  // path to the "Report changes" report to be exported

        #region Generate report

        // constructor which automatically invokes the generation of the report
        public ReportMetrics(MainForm form, bool relevancy)
        {
            this.form = form;
            this.relevancy = relevancy;

            try
            {
                bool validates = ConfigReader.Read(form.TextBoxNotRelevant1.Text, form);        // the path to the configuration file is provided in the GUI

                if (validates)  // if the XML config file is validated
                {
                    releases = new string[ConfigReader.ReportMetricsReleases.Count];
                    reportPath = ConfigReader.ReportMetricsPath;

                    for (int i = 0; i < ConfigReader.ReportMetricsReleases.Count; i++)    // reading releases from the configuration file
                        releases[i] = ConfigReader.ReportMetricsReleases[i];

                    form.ListAdd("Release paths successfully read.");

                    GenerateReport();       // generating report
                }
                else
                    form.ListAdd("Config file not read.");
            }
            catch (Exception exp)
            {
                form.ListAdd(exp.ToString());
            }
        }

        // generating the "Report metrics" report
        private void GenerateReport()
        {
            StringBuilder sb = new StringBuilder();

            MMResults[][] resultsMatrix = new MMResults[form.RolesList.Items.Count][];     // creation of one calculation results array (for each release) for each role

            for (int i = 0; i < form.RolesList.Items.Count; i++)
            {
                resultsMatrix[i] = new MMResults[releases.Length];

                for (int j = 0; j < releases.Length; j++)
                    resultsMatrix[i][j] = new MMResults();               // creation of one calculation results array for each release for each role
            }

            for (int i = 0; i < releases.Length; i++)       // for each release
            {
                form.ListAdd("Calculating " + releases[i].Split('\\')[releases[i].Split('\\').Length - 1]);

                string[] filePaths;

                model = MM_Extractor.XMLtoMetamodel(releases[i]);

                for (int ind = 0; ind < resultsMatrix.Length; ind++)     // for each role
                {
                    if (ind == 0)           // entire meta-model
                        CalculateResultsModel(model, resultsMatrix[ind], i);
                    /*else if (!ConfigReader.Roles[ind - 1].UtmOnly)      // M2 and M1 roles
                    {
                        filePaths = new string[ConfigReader.Roles[ind - 1].Packages.Count];     // array of packages for the analyzed role

                        for (int p = 0; p < ConfigReader.Roles[ind - 1].Packages.Count; p++)
                            filePaths[p] = ConfigReader.Roles[ind - 1].Packages[p];

                        CalculateResultsPackage(model, filePaths, resultsMatrix[ind], i);
                    }
                    else if (ConfigReader.Roles[ind - 1].UtmOnly)       // UTM role
                    {
                        filePaths = new string[ConfigReader.Roles[ind - 1].Packages.Count];

                        for (int p = 0; p < ConfigReader.Roles[ind - 1].Packages.Count; p++)    // array of packages for the analyzed role
                            filePaths[p] = ConfigReader.Roles[ind - 1].Packages[p];

                        CalculateResultsUTM(model, filePaths, resultsMatrix[ind], i);
                    }*/
                }
            }

            form.ListAdd("Printing report...");

            for (int ind = 0; ind < resultsMatrix.Length; ind++)
                if (ind == 0)                                                                                           // exporting results for the "entire meta-model"
                    CreateReportM2(resultsMatrix[ind], "entire model", releases.Length, sb);
               /* else if (!ConfigReader.Roles[ind - 1].UtmOnly && !ConfigReader.Roles[ind - 1].Model.Contains("M1"))     // exporting results for the M2 roles
                    CreateReportM2(resultsMatrix[ind], ConfigReader.Roles[ind - 1].Name, releases.Length, sb);
                else if (!ConfigReader.Roles[ind - 1].UtmOnly && ConfigReader.Roles[ind - 1].Model.Contains("M1"))      // exporting results for the M1 roles
                    CreateReportM1(resultsMatrix[ind], ConfigReader.Roles[ind - 1].Name, releases.Length, sb);
                else if (ConfigReader.Roles[ind - 1].UtmOnly)                                                           // exporting results for the UTM role
                    CreateReportUTM(resultsMatrix[ind], ConfigReader.Roles[ind - 1].Name, releases.Length, sb);*/

            File.WriteAllText(reportPath + @"\Results_metrics.csv", sb.ToString());

            form.ListAdd("Done!");
        }

        #endregion

        #region Calculate results

        // calculating results for the "entire meta-model"
        private void CalculateResultsModel(MetaModel model, MMResults[] resultsArray, int i)
        {
            resultsArray[i].NumOfElementsMod1 = model.NumberOfElements(relevancy);
            resultsArray[i].NumOfAttributesMod1 = model.NumberOfAttributes(relevancy);
            resultsArray[i].NumOfPackagesMod1 = model.NumberOfPackages(relevancy);
        }

        // calculating results for an array of packages
        private void CalculateResultsPackage(MetaModel model, string[] packagePaths, MMResults[] resultsArray, int i)
        {
            foreach (string packagePath in packagePaths)
            {
                Package package = model.FindPackageByPath(packagePath);
                    
                if (package == null)
                    package = new Package();

                resultsArray[i].NumOfElementsMod1 += package.NumberOfElements(relevancy);
                resultsArray[i].NumOfAttributesMod1 += package.NumberOfAttributes(relevancy);
                resultsArray[i].NumOfPackagesMod1 += package.NumberOfPackages(relevancy);
            }
        }

        // calculating results for the UTM role
        /*private void CalculateResultsUTM(MetaModel model, string[] packagePaths, MMResults[] resultsArray, int i)
        {
            foreach (string packagePath in packagePaths)
            {
                Package package = model.FindPackageByPath(packagePath);

                if (package == null)
                    package = new Package();

                List<EA_TaggedValue> UTMElems = package.GetAllUTMElements(relevancy);

                resultsArray[i].NumOfElementsMod1 += UTMElems.Count;

                foreach (EA_TaggedValue taggUTM in UTMElems)        // foreach tagged value
                    foreach (EA_TaggedValue taggOBS in taggUTM.ParentElement.TaggedValues)  // foreach parent element's tagged value
                        if (taggOBS.Name.Equals("atp.Status") && taggOBS.TagValue.Equals("obsolete"))   // check if there exists an obsolete tagged value
                            resultsArray[i].NumOfObsoleteElementsMod1++;
            }
        }*/

        #endregion

        #region Create report

        // creating report for the M2 roles including the entire meta-model
        private void CreateReportM2(MMResults[] resultsArray, string category, int releases, StringBuilder sb)
        {
            string relevantText = "";

            if (relevancy)
                relevantText = " relevant ";
            else
                relevantText = " all ";

            form.ExportAdd("Number of" + relevantText + "elements of " + category, sb);

            string array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            array = "";
            for (int i = 0; i < releases; i++)
                array += resultsArray[i].NumOfElementsMod1 + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Number of" + relevantText + "attributes of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            array = "";
            for (int i = 0; i < releases; i++)
                array += resultsArray[i].NumOfAttributesMod1 + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Number of" + relevantText + "packages of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            array = "";
            for (int i = 0; i < releases; i++)
                array += resultsArray[i].NumOfPackagesMod1 + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Number of" + relevantText + "obsolete elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Average dept of inheritance of" + relevantText + "elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Henry and Kafura complexity of" + relevantText + "elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Fan-in complexity of" + relevantText + "elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Fan-out complexity of" + relevantText + "elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Fan-in-out complexity of" + relevantText + "elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Package coupling of" + relevantText + "elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Package cohesion of" + relevantText + "elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Coupling between objects of" + relevantText + "elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Cohesion ration of" + relevantText + "elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("**************************************************", sb);
            form.ExportAdd("", sb);
        }

        // creating report for the M1 roles
        private void CreateReportM1(MMResults[] resultsArray, string category, int releases, StringBuilder sb)
        {
            string relevantText = "";

            if (relevancy)
                relevantText = " relevant ";
            else
                relevantText = " all ";

            form.ExportAdd("Number of" + relevantText + "elements of " + category, sb);

            string array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            array = "";
            for (int i = 0; i < releases; i++)
                array += resultsArray[i].NumOfElementsMod1 + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Number of" + relevantText + "packages of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            array = "";
            for (int i = 0; i < releases; i++)
                array += resultsArray[i].NumOfPackagesMod1 + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Number of" + relevantText + "obsolete elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("**************************************************", sb);
            form.ExportAdd("", sb);
        }

        // creating report for the UTM role
        private void CreateReportUTM(MMResults[] resultsArray, string category, int releases, StringBuilder sb)
        {
            string relevantText = "";

            if (relevancy)
                relevantText = " relevant ";
            else
                relevantText = " all ";

            form.ExportAdd("Number of" + relevantText + "elements of " + category, sb);

            string array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            array = "";
            for (int i = 0; i < releases; i++)
                array += resultsArray[i].NumOfElementsMod1 + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("Number of" + relevantText + "obsolete elements of " + category, sb);

            array = "";
            for (int i = 0; i < this.releases.Length; i++)
                array += this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";
            form.ExportAdd(array, sb);

            form.ExportAdd("", sb);
            form.ExportAdd("**************************************************", sb);
            form.ExportAdd("", sb);
        }

        #endregion
    }
}