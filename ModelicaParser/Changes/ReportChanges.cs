using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ModelicaChangeAnalyzer.Config;
using ModelicaChangeAnalyzer.Datamodel;
using ModelicaChangeAnalyzer.Extract;

namespace ModelicaChangeAnalyzer.Changes
{
    // creates a report with changes between multiple AUTOSAR meta-model releases
    public class ReportChanges
    {
        private MainForm form;
        private MetaModel model1 = null;
        private MetaModel model2 = null;
        private bool relevancy;             // based on the selected checkbox in the GUI
        private string[] releases;          // paths to all extracted releases
        private string reportPath;          // path to the "Report changes" report to be exported

        #region Generate report

        // constructor which automatically invokes the generation of the report
        public ReportChanges(MainForm form, bool relevancy)
        {
            this.form = form;
            this.relevancy = relevancy;

            try
            {
                bool validates = ConfigReader.Read(form.TextBoxNotRelevant1.Text, form);               // the path to the configuration file is provided in the GUI

                if (validates)  // if the XML config file is validated
                {
                    releases = new string[ConfigReader.ReportChangesReleases.Count];
                    reportPath = ConfigReader.ReportChangesPath;

                    for (int i = 0; i < ConfigReader.ReportChangesReleases.Count; i++)    // reading releases from the configuration file
                        releases[i] = ConfigReader.ReportChangesReleases[i];

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

        // generating the "Report changes" report
        private void GenerateReport()
        {
            StringBuilder sb = new StringBuilder();

            Results[][][] resultsArrayMatrix = new Results[form.RolesList.Items.Count][][];     // creation of one calculation results matrix (for each par of releases) for each role

            for (int i = 0; i < form.RolesList.Items.Count; i++)
            {
                resultsArrayMatrix[i] = new Results[releases.Length][];

                for (int j = 0; j < releases.Length; j++)
                {
                    resultsArrayMatrix[i][j] = new Results[releases.Length];

                    for (int k = 0; k < releases.Length; k++)
                        resultsArrayMatrix[i][j][k] = new Results();                     // creation of one calculation results matrix for each par of releases for each role
                }
            }

            for (int i = 0; i < releases.Length; i++)           // comparing each realease with each other
                for (int j = i + 1; j < releases.Length; j++)
                {
                    form.ListAdd("Comparing " + releases[i].Split('\\')[releases[i].Split('\\').Length - 1] + " and " + releases[j].Split('\\')[releases[j].Split('\\').Length - 1]);

                    model1 = Extractor.XMLtoMetamodel(releases[i]);
                    model2 = Extractor.XMLtoMetamodel(releases[j]);

                    for (int ind = 0; ind < resultsArrayMatrix.Length; ind++)     // foreach role
                    {
                        if (ind == 0)
                        {   // entire meta-model
                            CalculateResultsModel(model1, model2, resultsArrayMatrix[ind], i, j);
                            CalculateResultsModel(model1, model2, resultsArrayMatrix[ind], j, i);
                        }
                        /*else if (!ConfigReader.Roles[ind - 1].UtmOnly)      // M2 and M1 roles
                        {
                            filePaths = new string[ConfigReader.Roles[ind - 1].Packages.Count];     // array of packages for the analyzed role

                            for (int p = 0; p < ConfigReader.Roles[ind - 1].Packages.Count; p++)
                                filePaths[p] = ConfigReader.Roles[ind - 1].Packages[p];

                            CalculateResultsPackage(model1, model2, filePaths, resultsArrayMatrix[ind], i, j);
                        }
                        else if (ConfigReader.Roles[ind - 1].UtmOnly)       // UTM role
                        {
                            filePaths = new string[ConfigReader.Roles[ind - 1].Packages.Count];     // array of packages for the analyzed role

                            for (int p = 0; p < ConfigReader.Roles[ind - 1].Packages.Count; p++)
                                filePaths[p] = ConfigReader.Roles[ind - 1].Packages[p];
                        }*/
                    }
                }



            form.ListAdd("Printing report...");

            for (int ind = 0; ind < resultsArrayMatrix.Length; ind++)
                if (ind == 0)                                   // exporting results for the "entire meta-model"
                    CreateReport(resultsArrayMatrix[ind], "entire model", releases.Length, sb);
                /*else if (!ConfigReader.Roles[ind - 1].UtmOnly)  // exporting results for the M1 and M2 roles
                    CreateReport(resultsArrayMatrix[ind], ConfigReader.Roles[ind - 1].Name, releases.Length, sb);
                else if (ConfigReader.Roles[ind - 1].UtmOnly)   // exporting results for the UTM role
                    CreateReportUTM(resultsArrayMatrix[ind], ConfigReader.Roles[ind - 1].Name, releases.Length, sb);*/

            File.WriteAllText(reportPath + @"\Results_changes.csv", sb.ToString());

            form.ListAdd("Done!");
        }

        #endregion

        #region Calculare results

        // calculating results for the "entire meta-model"
        private void CalculateResultsModel(MetaModel model1, MetaModel model2, Results[][] resultsMatrix, int i, int j)
        {
            model1.ResetCalculation();
            model2.ResetCalculation();

            resultsMatrix[i][j].NumOfChanges = model2.CompareModels(model1, relevancy);
            resultsMatrix[i][j].NumOfChangedElements = model2.NumberOfModifiedElements() + model2.NumberOfAddedElements() + model2.NumberOfRemovedElements();
            resultsMatrix[i][j].NumOfModifiedElements = model2.NumberOfModifiedElements();
            resultsMatrix[i][j].NumOfAddedElements = model2.NumberOfAddedElements();
            resultsMatrix[i][j].NumOfRemovedElements = model2.NumberOfRemovedElements();

            resultsMatrix[i][j].NumOfChangedConnectors = model2.NumberOfModifiedConnectors() + model2.NumberOfAddedConnectors() + model2.NumberOfRemovedConnectors();
            resultsMatrix[i][j].NumOfModifiedConnectors = model2.NumberOfModifiedConnectors();
            resultsMatrix[i][j].NumOfAddedConnectors = model2.NumberOfAddedConnectors();
            resultsMatrix[i][j].NumOfRemovedConnectors = model2.NumberOfRemovedConnectors();

            resultsMatrix[i][j].NumOfChangedAttributes = model2.NumberOfModifiedAttributes() + model2.NumberOfAddedAttributes() + model2.NumberOfRemovedAttributes();
            resultsMatrix[i][j].NumOfModifiedAttributes = model2.NumberOfModifiedAttributes();
            resultsMatrix[i][j].NumOfAddedAttributes = model2.NumberOfAddedAttributes();
            resultsMatrix[i][j].NumOfRemovedAttributes = model2.NumberOfRemovedAttributes();
            resultsMatrix[i][j].NumOfChangedPackages = model2.NumberOfModifiedPackages() + model2.NumberOfAddedPackages() + model2.NumberOfRemovedPackages();
            resultsMatrix[i][j].NumOfModifiedPackages = model2.NumberOfModifiedPackages();
            resultsMatrix[i][j].NumOfAddedPackages = model2.NumberOfAddedPackages();
            resultsMatrix[i][j].NumOfRemovedPackages = model2.NumberOfRemovedPackages();
        }

        // calculating results for an array of packages
        private void CalculateResultsPackage(MetaModel model1, MetaModel model2, string[] packagePaths, Results[][] resultsMatrix, int i, int j)
        {
            foreach (string packagePath in packagePaths)
            {
                Package package1 = model1.FindPackageByPath(packagePath);
                Package package2 = model2.FindPackageByPath(packagePath);

                if (package1 == null)
                    package1 = new Package();

                if (package2 == null)
                    package2 = new Package();

                package1.ResetCalculation();
                package2.ResetCalculation();

                resultsMatrix[i][j].NumOfChanges += package2.ComparePackages(package1, relevancy);
                resultsMatrix[i][j].NumOfChangedElements += package2.NumberOfModifiedElements() + package2.NumberOfAddedElements() + package2.NumberOfRemovedElements();
                resultsMatrix[i][j].NumOfModifiedElements += package2.NumberOfModifiedElements();
                resultsMatrix[i][j].NumOfAddedElements += package2.NumberOfAddedElements();
                resultsMatrix[i][j].NumOfRemovedElements += package2.NumberOfRemovedElements();
                resultsMatrix[i][j].NumOfChangedAttributes += package2.NumberOfModifiedAttributes() + package2.NumberOfAddedAttributes() + package2.NumberOfRemovedAttributes();
                resultsMatrix[i][j].NumOfModifiedAttributes += package2.NumberOfModifiedAttributes();
                resultsMatrix[i][j].NumOfAddedAttributes += package2.NumberOfAddedAttributes();
                resultsMatrix[i][j].NumOfRemovedAttributes += package2.NumberOfRemovedAttributes();
                resultsMatrix[i][j].NumOfChangedPackages += package2.NumberOfModifiedSubPackages() + package2.NumberOfAddedSubPackages() + package2.NumberOfRemovedSubPackages();
                resultsMatrix[i][j].NumOfModifiedPackages += package2.NumberOfModifiedSubPackages();
                resultsMatrix[i][j].NumOfAddedPackages += package2.NumberOfAddedSubPackages();
                resultsMatrix[i][j].NumOfRemovedPackages += package2.NumberOfRemovedSubPackages();
            }
        }


        #endregion

        #region Create report

        // creating report for the "entire meta-model" and the M1/M2 roles
        private void CreateReport(Results[][] resultsMatrix, string category, int releases, StringBuilder sb)
        {
            string relevantText = "";

            if (relevancy)
                relevantText = " relevant ";
            else
                relevantText = " all ";

            form.ExportAdd("Number of " + category + relevantText + "changes", sb);

            string matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfChanges + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "changed elements", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfChangedElements + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "modified elements", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfModifiedElements + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "added elements", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfAddedElements + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "removed elements", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfRemovedElements + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "changed attributes", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfChangedAttributes + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "modified attributes", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfModifiedAttributes + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "added attributes", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfAddedAttributes + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "removed attributes", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfRemovedAttributes + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "changed packages", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfChangedPackages + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "modified packages", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfModifiedPackages + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "added packages", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfAddedPackages + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "removed packages", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfRemovedPackages + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("**************************************************", sb);
            form.ExportAdd("", sb);
        }

        // creating report for the UTM role
        private void CreateReportUTM(Results[][] resultsMatrix, string category, int releases, StringBuilder sb)
        {
            string relevantText = "";

            if (relevancy)
                relevantText = " relevant ";
            else
                relevantText = " all ";

            form.ExportAdd("Number of " + category + relevantText + "changes", sb);

            string matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfChanges + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "changed elements", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfChangedElements + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "modified elements", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfModifiedElements + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "added elements", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfAddedElements + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("Number of " + category + relevantText + "removed elements", sb);

            matrixRow = "";
            for (int i = 0; i < this.releases.Length; i++)
                matrixRow += ";" + this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1];
            form.ExportAdd(matrixRow, sb);

            for (int i = 0; i < releases; i++)
            {
                matrixRow = this.releases[i].Split('\\')[this.releases[i].Split('\\').Length - 1] + ";";

                for (int j = 0; j < releases; j++)
                    matrixRow += resultsMatrix[i][j].NumOfRemovedElements + ";";

                form.ExportAdd(matrixRow, sb);
            }

            form.ExportAdd("", sb);
            form.ExportAdd("**************************************************", sb);
            form.ExportAdd("", sb);
        }

        #endregion
    }
}