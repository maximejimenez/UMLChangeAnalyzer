using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;
//using SharpSvn;
using System.Threading;
using System.Text.RegularExpressions;
using ModelicaChangeAnalyzer.Datamodel;
using ModelicaChangeAnalyzer.Config;
using ModelicaChangeAnalyzer.Changes;
using ModelicaChangeAnalyzer.Extract;

namespace ModelicaChangeAnalyzer
{
    public class MainForm : System.Windows.Forms.Form
    {
        private MetaModel model1 = null;                 // Model1 loaded and used for comparing
        private MetaModel model2 = null;                 // Model2 loaded and used for comparing
        private List<Results> resultsList;            // list of calculation results for each role
        private bool featuresLoaded = false;            // indicates whether the features are loaded
        private string selectedRole = "entire model";   // name for the selected role
        private string modelPath1 = "";
        private string modelPath2 = "";
        private string RFCs = "";                       // copies the RFCs before the feature calculation which will be used throughout the feature calculations

        // constructing the form
        public MainForm()
        {
            InitializeComponent();
            rolesList.SelectedIndex = 0;                    // selecting the first role in the ComboBox
        }

        [STAThread]
        static void Main()
        {
            Application.Run(new MainForm());
        }

        #region Accessing GUI components

        // clearing the text area for presenting the results
        public void ClearListBox()
        {
            richTextBox1.Clear();
            richTextBox1.Refresh();
        }

        // enabling/disabling all buttons
        public void EnableButtons(bool enable)
        {
            if (config.InvokeRequired)
            {
                EnableButtonsCallback d = new EnableButtonsCallback(EnableButtons);
                this.Invoke(d, new object[] { enable });
            }
            else
                config.Enabled = enable;

            if (extract.InvokeRequired)
            {
                EnableButtonsCallback d = new EnableButtonsCallback(EnableButtons);
                this.Invoke(d, new object[] { enable });
            }
            else
                extract.Enabled = enable;

            if (load.InvokeRequired)
            {
                EnableButtonsCallback d = new EnableButtonsCallback(EnableButtons);
                this.Invoke(d, new object[] { enable });
            }
            else
                load.Enabled = enable;

            /*if (features.InvokeRequired)
            {
                EnableButtonsCallback d = new EnableButtonsCallback(EnableButtons);
                this.Invoke(d, new object[] { enable });
            }
            else
                features.Enabled = enable;
            */

            if (extractMultiple.InvokeRequired)
            {
                EnableButtonsCallback d = new EnableButtonsCallback(EnableButtons);
                this.Invoke(d, new object[] { enable });
            }
            else
                extractMultiple.Enabled = enable;

            if (reportChanges.InvokeRequired)
            {
                EnableButtonsCallback d = new EnableButtonsCallback(EnableButtons);
                this.Invoke(d, new object[] { enable });
            }
            else
                reportChanges.Enabled = enable;

            if (reportMetrics.InvokeRequired)
            {
                EnableButtonsCallback d = new EnableButtonsCallback(EnableButtons);
                this.Invoke(d, new object[] { enable });
            }
            else
                reportMetrics.Enabled = enable;

            /*if (reportFeatures.InvokeRequired)
            {
                EnableButtonsCallback d = new EnableButtonsCallback(EnableButtons);
                this.Invoke(d, new object[] { enable });
            }
            else
                reportFeatures.Enabled = enable;
            */
            if (compare.InvokeRequired)
            {
                EnableButtonsCallback d = new EnableButtonsCallback(EnableButtons);
                this.Invoke(d, new object[] { enable });
            }
        }

        // delegate for enabling/disabling all buttons
        delegate void EnableButtonsCallback(bool enable);

        // enabling/disabling compare button
        public void EnableCompare(bool enable)
        {
            if (compare.InvokeRequired)
            {
                EnableCompareCallback d = new EnableCompareCallback(EnableCompare);
                this.Invoke(d, new object[] { enable });
            }
            else
                compare.Enabled = enable;
        }

        // delegate for enabling/disabling compare button
        delegate void EnableCompareCallback(bool enable);

        // enabling/disabling metrics check boxes related to their inavailability for features (used by Load and Feature buttons)
        public void EnableCheckBox(bool enableFeatures)
        {
            if (enableFeatures)
            {
                if (checkBoxSizeMetrics.InvokeRequired)
                {
                    EnableCheckBoxCallback d = new EnableCheckBoxCallback(EnableCheckBox);
                    this.Invoke(d, new object[] { enableFeatures });
                }
                else
                {
                    checkBoxSizeMetrics.Checked = false;
                    checkBoxSizeMetrics.Enabled = false;
                }

                if (checkBoxModifiedPacks.InvokeRequired)
                {
                    EnableCheckBoxCallback d = new EnableCheckBoxCallback(EnableCheckBox);
                    this.Invoke(d, new object[] { enableFeatures });
                }
                else
                {
                    checkBoxModifiedPacks.Checked = false;
                    checkBoxModifiedPacks.Enabled = false;
                }

                if (checkBoxAddedPacks.InvokeRequired)
                {
                    EnableCheckBoxCallback d = new EnableCheckBoxCallback(EnableCheckBox);
                    this.Invoke(d, new object[] { enableFeatures });
                }
                else
                {
                    checkBoxAddedPacks.Checked = false;
                    checkBoxAddedPacks.Enabled = false;
                }

                if (checkBoxRemovedPacks.InvokeRequired)
                {
                    EnableCheckBoxCallback d = new EnableCheckBoxCallback(EnableCheckBox);
                    this.Invoke(d, new object[] { enableFeatures });
                }
                else
                {
                    checkBoxRemovedPacks.Checked = false;
                    checkBoxRemovedPacks.Enabled = false;
                }
            }
            else
            {
                if (checkBoxSizeMetrics.InvokeRequired)
                {
                    EnableCheckBoxCallback d = new EnableCheckBoxCallback(EnableCheckBox);
                    this.Invoke(d, new object[] { enableFeatures });
                }
                else
                    checkBoxSizeMetrics.Enabled = true;

                if (checkBoxModifiedPacks.InvokeRequired)
                {
                    EnableCheckBoxCallback d = new EnableCheckBoxCallback(EnableCheckBox);
                    this.Invoke(d, new object[] { enableFeatures });
                }
                else
                    checkBoxModifiedPacks.Enabled = true;

                if (checkBoxAddedPacks.InvokeRequired)
                {
                    EnableCheckBoxCallback d = new EnableCheckBoxCallback(EnableCheckBox);
                    this.Invoke(d, new object[] { enableFeatures });
                }
                else
                    checkBoxAddedPacks.Enabled = true;

                if (checkBoxRemovedPacks.InvokeRequired)
                {
                    EnableCheckBoxCallback d = new EnableCheckBoxCallback(EnableCheckBox);
                    this.Invoke(d, new object[] { enableFeatures });
                }
                else
                    checkBoxRemovedPacks.Enabled = true;
            }
        }

        // delegate for enabling/disabling metrics check boxes
        delegate void EnableCheckBoxCallback(bool enableFeatures);

        // enabling the relevancy button
        public void EnableRelevancy(bool enable)
        {
            if (compare.InvokeRequired)
            {
                EnableRelevancyCallback d = new EnableRelevancyCallback(EnableRelevancy);
                this.Invoke(d, new object[] { enable });
            }
            else
                checkBoxRelevancePolicy.Enabled = enable;
        }

        // delegate for enabling the relevancy button
        delegate void EnableRelevancyCallback(bool enable);

        #endregion

        #region Printing and exporting results

        // adding the results to the text area
        public void ListAdd(string msg)
        {
            if (richTextBox1.InvokeRequired)
            {
                ListAddCallback d = new ListAddCallback(ListAdd);
                this.Invoke(d, new object[] { msg });
            }
            else
            {
                richTextBox1.AppendText(msg + "\n");
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                richTextBox1.Refresh();
            }
        }

        // delegate for // adding the results to the text area
        delegate void ListAddCallback(string msg);

        // adding the results to the text area and exporting them to a file
        private void ListExportAdd(string msg, StringBuilder sb)
        {
            if (richTextBox1.InvokeRequired)
            {
                ListExportAddCallback d = new ListExportAddCallback(ListExportAdd);
                this.Invoke(d, new object[] { msg, sb });
            }
            else
            {
                richTextBox1.AppendText(msg + "\n");
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                richTextBox1.Refresh();

                string[] parts = msg.Split('\t');
                string result = "";

                for (int i = 0; i < parts.Length - 1; i++)
                    result += ";";
                result += parts[parts.Length - 1];

                sb.AppendLine(result);
            }
        }

        // delegate for adding the results to the text area and exporting them to a file
        delegate void ListExportAddCallback(string msg, StringBuilder sb);

        // adding the results of listing changes to the text area and exporting them to a file
        private void ListExportAddChanges(string num, string msg, StringBuilder sb)
        {
            if (richTextBox1.InvokeRequired)
            {
                ListExportAddChangesCallback d = new ListExportAddChangesCallback(ListExportAddChanges);
                this.Invoke(d, new object[] { num, msg, sb });
            }
            else
            {
                richTextBox1.AppendText(msg + "\n");
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                richTextBox1.Refresh();

                string[] parts = msg.Split('\t');
                string result = "";

                for (int i = 0; i < parts.Length - 1; i++)
                    result += ";";
                result += parts[parts.Length - 1];

                sb.AppendLine(num + ";" + result);
            }
        }

        // delegate for adding the results of listing changes to the text area and exporting them to a file
        delegate void ListExportAddChangesCallback(string num, string msg, StringBuilder sb);

        // exporting the results to a file
        public void ExportAdd(string msg, StringBuilder sb)
        {
            if (richTextBox1.InvokeRequired)
            {
                ExportAddCallback d = new ExportAddCallback(ExportAdd);
                this.Invoke(d, new object[] { msg, sb });
            }
            else
            {
                string[] parts = msg.Split('\t');
                string result = "";

                for (int i = 0; i < parts.Length - 1; i++)
                    result += ";";
                result += parts[parts.Length - 1];

                sb.AppendLine(result);
            }
        }

        // delegate for exporting the results to a file
        delegate void ExportAddCallback(string msg, StringBuilder sb);

        #endregion

        #region Windows events and threads for the events

        // disposing the main window
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        // resizing the main window
        private void EA_Dump_Form_Resize(object sender, System.EventArgs e)
        {
            richTextBox1.Width = ClientSize.Width - richTextBox1.Left - 10;
            richTextBox1.Height = ClientSize.Height - 335;
        }

        // event different role selected
        private void rolesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxModifiedPacks.Enabled = true;
            checkBoxAddedPacks.Enabled = true;
            checkBoxRemovedPacks.Enabled = true;
            checkBoxModifiedAttrs.Enabled = true;
            checkBoxAddedAttrs.Enabled = true;
            checkBoxRemovedAttrs.Enabled = true;

            // disabling options for feature calculations
            if (featuresLoaded)
            {
                checkBoxSizeMetrics.Enabled = false;
                checkBoxModifiedPacks.Enabled = false;
                checkBoxRemovedPacks.Enabled = false;
                checkBoxAddedPacks.Enabled = false;
            }
        }

        // reading the config file event (work done in a separate thread)
        private void config_Click(object sender, EventArgs e)
        {
            ClearListBox();

            Thread t = new Thread(read_Click_Function);
            t.IsBackground = true;
            t.Start();

        }

        // extraction event (work done in a separate thread)
        private void extract_Click(object sender, System.EventArgs e)
        {
            ClearListBox();

            Thread t = new Thread(extract_Click_Function);
            t.IsBackground = true;
            t.Start();
        }

        // loading event (work done in a separate thread)
        private void load_Click(object sender, EventArgs e)
        {
            ClearListBox();
            modelPath1 = textBoxModel1.Text.Split('\\')[textBoxModel1.Text.Split('\\').Length - 1];
            modelPath2 = textBoxModel2.Text.Split('\\')[textBoxModel2.Text.Split('\\').Length - 1];

            Thread t = new Thread(load_Click_Function);
            t.IsBackground = true;
            t.Start();
        }

        // comparing event (work done in a separate thread)
        private void compare_Click(object sender, EventArgs e)
        {
            ClearListBox();
            selectedRole = "" + rolesList.SelectedItem;

            Thread t = new Thread(compare_Click_Function);
            t.IsBackground = true;
            t.Start();
        }

        // features calculation event (work done in a separate thread)
        private void features_Click(object sender, EventArgs e)
        {
            ClearListBox();
            RFCs = textBoxFeatures.Text;

            /*Thread t = new Thread(features_Click_Function);
            t.IsBackground = true;
            t.Start();*/
        }

        // extracting multiple event (work done in a separate thread)
        private void extractMultiple_Click(object sender, EventArgs e)
        {
            ClearListBox();

            Thread t = new Thread(extractMultiple_Click_Function);
            t.IsBackground = true;
            t.Start();
        }

        // reporting changes event (work done in a separate thread)
        private void reportChanges_Click(object sender, EventArgs e)
        {
            ClearListBox();

            Thread t = new Thread(reportChanges_Click_Function);
            t.IsBackground = true;
            t.Start();
        }

        // reporting metrics event (work done in a separate thread)
        private void reportMetrics_Click(object sender, EventArgs e)
        {
            ClearListBox();

            Thread t = new Thread(reportMetrics_Click_Function);
            t.IsBackground = true;
            t.Start();
        }

        // reporting feature calculation event (work done in a separate thread)
        private void reportFeatures_Click(object sender, EventArgs e)
        {
            ClearListBox();

            /*Thread t = new Thread(reportFeatures_Click_Function);
            t.IsBackground = true;
            t.Start();*/
        }

        // reading the config file
        private void read_Click_Function()
        {
            try
            {
                EnableButtons(false);   // disabling all buttons
                EnableCompare(false);

                bool validates = ConfigReader.Read(textBoxNotRelevant.Text, this);

                if (validates)  // if the XML config file is validated
                {
                    //AddRolesGUI(ConfigReader.Roles);
                    //EnableRelevancy(true);
                    ListAdd("Config file successfully read.");
                }
                else
                    ListAdd("Config file not read.");
    
                EnableButtons(true);   // enabling all buttons
            }
            catch (Exception exp)
            {
                ListAdd(exp.ToString());
                EnableButtons(true);   // enabling all buttons
                EnableRelevancy(false);
            }
        }

        // extraction of the meta-model
        private void extract_Click_Function()
        {
            try
            {
                EnableButtons(false);   // disabling all buttons
                EnableCompare(false);

                Extractor extractor = new Extractor(this);

                ListAdd("Dumping model...");
                string version = "1.9.X";
                extractor.ExtractModel(textBoxModelPath.Text, textBoxFilePath.Text, version);
                ListAdd("Model successfully dumped!");
                extractor.ReleaseModel();

                EnableButtons(true);   // enabling all buttons
            }
            catch (Exception exp)
            {
                ListAdd(exp.ToString());
                EnableButtons(true);   // enabling all buttons
            }
        }

        // loading two models and calculating results for each role
        private void load_Click_Function()
        {
            try
            {
                EnableButtons(false);   // disabling all buttons
                EnableCompare(false);
                EnableCheckBox(false);
                featuresLoaded = false;

                ListAdd("Loading model 1...");
                model1 = Extractor.XMLtoMetamodel(textBoxModel1.Text);

                ListAdd("Loading model 2...");
                model2 = Extractor.XMLtoMetamodel(textBoxModel2.Text);

                resultsList = new List<Results>();

                for (int i = 0; i < rolesList.Items.Count; i++)     // calculating results for each role
                {
                    ListAdd("Calculating role '" + rolesList.Items[i] + "'...");
                    Results results = new Results();

                    if (i == 0)     // in case of entire meta-model
                    {
                        model1.ResetCalculation();
                        model2.ResetCalculation();

                        results.CalculateModels(model1, model2, checkBoxRelevancePolicy.Checked);
                    }

                    resultsList.Add(results);
                }

                ListAdd("Calculation done.");

                EnableButtons(true);   // enabling all buttons
                EnableCompare(true);
            }
            catch (Exception exp)
            {
                ListAdd(exp.ToString());
                EnableButtons(true);   // enabling all buttons
                EnableCompare(false);
                EnableCheckBox(false);
            }
        }

        // event compare button click shall compare the models/packages/elemets based on the selected radio button and present calculation results based on the selected calculation options
        private void compare_Click_Function()
        {
            try
            {           // checking if something is selected for presentation
                if (!checkBoxSizeMetrics.Checked && !checkBoxChangeMetrics.Checked && !checkBoxAllChanges.Checked 
                    && !checkBoxModifiedElems.Checked && !checkBoxAddedElems.Checked && !checkBoxRemovedElems.Checked 
                    && !checkBoxModifiedPacks.Checked && !checkBoxAddedPacks.Checked && !checkBoxRemovedPacks.Checked 
                    && !checkBoxModifiedAttrs.Checked && !checkBoxAddedAttrs.Checked && !checkBoxRemovedAttrs.Checked)
                {
                    ListAdd("Nothing selected to be shown!");
                    return;
                }

                EnableButtons(false);   // disabling all buttons
                EnableCompare(false);

                StringBuilder sb = new StringBuilder();

                Results results = resultsList[0];    // getting the results for the selected role

                if (checkBoxSizeMetrics.Checked)
                {
                    ListExportAdd("********** SIZE METRICS **********", sb);
                    ListExportAdd("", sb);
                    ListExportAdd("Number of packages: " + results.NumOfPackagesMod1 + " -> " + results.NumOfPackagedMod2 + " (" + (results.NumOfPackagedMod2 - results.NumOfPackagesMod1) + ")", sb);
                    ListExportAdd("Number of elements: " + results.NumOfElementsMod1 + " -> " + results.NumOfElementsMod2 + " (" + (results.NumOfElementsMod2 - results.NumOfElementsMod1) + ")", sb);
                    ListExportAdd("Number of connectors: " + results.NumOfConnectorsMod1 + " -> " + results.NumOfConnectorsMod2 + " (" + (results.NumOfConnectorsMod2 - results.NumOfConnectorsMod1) + ")", sb);
                    ListExportAdd("Number of attributes: " + results.NumOfAttributesMod1 + " -> " + results.NumOfAttributesMod2 + " (" + (results.NumOfAttributesMod2 - results.NumOfAttributesMod1) + ")", sb);

                    ListExportAdd("", sb);
                }

                if (checkBoxChangeMetrics.Checked)
                {
                    ListExportAdd("********** CHANGE METRICS **********", sb);
                    ListExportAdd("", sb);
                    ListExportAdd("Number of changes: " + results.NumOfChanges, sb);

                    ListExportAdd("Number of changed packages: " + (results.NumOfModifiedPackages + results.NumOfAddedPackages + results.NumOfRemovedPackages), sb);
                    ListExportAdd("\tNumber of modified packages: " + results.NumOfModifiedPackages, sb);
                    ListExportAdd("\tNumber of added packages: " + results.NumOfAddedPackages, sb);
                    ListExportAdd("\tNumber of removed packages: " + results.NumOfRemovedPackages, sb);

                    ListExportAdd("Number of changed elements: " + (results.NumOfModifiedElements + results.NumOfAddedElements + results.NumOfRemovedElements), sb);
                    ListExportAdd("\tNumber of modified elements: " + results.NumOfModifiedElements, sb);
                    ListExportAdd("\tNumber of added elements: " + results.NumOfAddedElements, sb);
                    ListExportAdd("\tNumber of removed elements: " + results.NumOfRemovedElements, sb);


                    ListExportAdd("Number of changed connectors: " + (results.NumOfModifiedConnectors + results.NumOfAddedConnectors + results.NumOfRemovedConnectors), sb);
                    ListExportAdd("\tNumber of modified connectors: " + results.NumOfModifiedConnectors, sb);
                    ListExportAdd("\tNumber of added connectors: " + results.NumOfAddedConnectors, sb);
                    ListExportAdd("\tNumber of removed connectors: " + results.NumOfRemovedConnectors, sb);

                    ListExportAdd("Number of changed attributes: " + (results.NumOfModifiedAttributes + results.NumOfAddedAttributes + results.NumOfRemovedAttributes), sb);
                    ListExportAdd("\tNumber of modified attributes: " + results.NumOfModifiedAttributes, sb);
                    ListExportAdd("\tNumber of added attributes: " + results.NumOfAddedAttributes, sb);
                    ListExportAdd("\tNumber of removed attributes: " + results.NumOfRemovedAttributes, sb);
                    
                    ListExportAdd("", sb);
                }

                if (checkBoxAllChanges.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("All changes:", sb);

                    int x = 1;
                    foreach (Change chg in results.Changes)
                        if (chg.PrintOnly)
                            ListExportAddChanges("", chg.ToString(), sb);
                        else
                            ListExportAddChanges("" + x++, chg.ToString(), sb);
                    //ListExportAdd(results.Changes.Count+" changes", sb);
                    ListExportAdd("", sb);
                }

                if (checkBoxModifiedPacks.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("Modified packages:", sb);
                    ListExportAdd("", sb);

                    int i = 1;
                    foreach (Package pack in results.ModifiedPackages)
                        ListExportAdd(i++ + ": " + pack.GetPath(), sb);
                    ListExportAdd("", sb);
                }

                if (checkBoxAddedPacks.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("Added packages:", sb);
                    ListExportAdd("", sb);

                    int i = 1;
                    foreach (Package pack in results.AddedPackages)
                        ListExportAdd(i++ + ": " + pack.GetPath(), sb);
                    ListExportAdd("", sb);
                }

                if (checkBoxRemovedPacks.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("Removed packages:", sb);
                    ListExportAdd("", sb);

                    int i = 1;
                    foreach (Package pack in results.RemovedPackages)
                        ListExportAdd(i++ + ": " + pack.GetPath(), sb);
                    ListExportAdd("", sb);
                }

                if (checkBoxModifiedElems.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("Modified elements:", sb);
                    ListExportAdd("", sb);

                    int i = 1;
                    foreach (Element elem in results.ModifiedElements)
                        ListExportAdd(i++ + ": " + elem.GetPath(), sb);
                    ListExportAdd("", sb);
                }

                if (checkBoxAddedElems.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("Added elements:", sb);
                    ListExportAdd("", sb);

                    int i = 1;
                    foreach (Element elem in results.AddedElements)
                        ListExportAdd(i++ + ": " + elem.GetPath(), sb);
                    ListExportAdd("", sb);
                }

                if (checkBoxRemovedElems.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("Removed elements:", sb);
                    ListExportAdd("", sb);

                    int i = 1;
                    foreach (Element elem in results.RemovedElements)
                        ListExportAdd(i++ + ": " + elem.GetPath(), sb);
                    ListExportAdd("", sb);
                }

                if (checkBoxModifiedAttrs.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("Modified attributes:", sb);
                    ListExportAdd("", sb);

                    int i = 1;
                    foreach (ModelicaChangeAnalyzer.Datamodel.Attribute attr in results.ModifiedAttributes)
                        ListExportAdd(i++ + ": " + attr.GetPath(), sb);
                    ListExportAdd("", sb);
                }

                if (checkBoxAddedAttrs.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("Added attributes:", sb);
                    ListExportAdd("", sb);

                    int i = 1;
                    foreach (ModelicaChangeAnalyzer.Datamodel.Attribute attr in results.AddedAttributes)
                        ListExportAdd(i++ + ": " + attr.GetPath(), sb);
                    ListExportAdd("", sb);
                }

                if (checkBoxRemovedAttrs.Checked)
                {
                    ListExportAdd("********** CHANGES **********", sb);
                    ListExportAdd("Removed attributes:", sb);
                    ListExportAdd("", sb);

                    int i = 1;
                    foreach (ModelicaChangeAnalyzer.Datamodel.Attribute attr in results.RemovedAttributes)
                        ListExportAdd(i++ + ": " + attr.GetPath(), sb);
                    ListExportAdd("", sb);
                }

                string str = "";

                if (featuresLoaded)                                             // first row in the exported file in case of printing the resutls of feature calculation
                    str = "Feature RFCs: " + RFCs + "\n\n" + sb.ToString();
                else                                                            // first row in the exported file in case of printing the resutls of meta-model compare calculation
                    str = modelPath1 + " -> " + modelPath2 + "\n\n" + sb.ToString();

                if (ConfigReader.ResultsPath.Equals(""))        // if no results path is provided in the configuration file, saving the results on the default location
                    File.WriteAllText(Directory.GetCurrentDirectory() + @"\Results (" + selectedRole + ").csv", str);
                else                                                                                            // otherwise saving the results on the provided path in the config file
                    File.WriteAllText(ConfigReader.ResultsPath + @"\Results (" + selectedRole + ").csv", str); 

                EnableButtons(true);   // enabling all buttons
                EnableCompare(true);
            }
            catch (Exception exp)
            {
                ListAdd(exp.ToString());
                EnableButtons(true);   // enabling all buttons
                EnableCompare(true);
            }
        }

        // extracting multiple meta-model releases
        private void extractMultiple_Click_Function()
        {
            EnableButtons(false);
            new ExtractMultiple(this);
            EnableButtons(true);
        }

        // reporting changes for multiple meta-model releases
        private void reportChanges_Click_Function()
        {
            EnableButtons(false);
            new ReportChanges(this, checkBoxRelevancePolicy.Checked);
            EnableButtons(true);
        }

        // reporting metrics for multiple meta-model releases
        private void reportMetrics_Click_Function()
        {
            EnableButtons(false);
            new ReportMetrics(this, checkBoxRelevancePolicy.Checked);
            EnableButtons(true);
        }

        #endregion

        #region Getters

        public TextBox TextBoxModelPath
        {
            get { return textBoxModelPath; }
        }

        public TextBox TextBoxFilePath
        {
            get { return textBoxFilePath; }
        }

        public TextBox TextBoxNotRelevant
        {
            get { return textBoxNotRelevant; }
        }

        public ComboBox RolesList
        {
            get { return rolesList; }
        }

        public TextBox TextBoxNotRelevant1
        {
            get { return textBoxNotRelevant; }
        }

        #endregion
        
        #region Windows Form Designer generated code

        private Container components = null;
        private Button extract;
        private Button compare;
        private Button load;
        private TextBox textBoxModelPath;
        private TextBox textBoxFilePath;
        private TextBox textBoxModel2;
        private TextBox textBoxModel1;
        private TextBox textBoxNotRelevant;
        private RichTextBox richTextBox1;
        private CheckBox checkBoxRelevancePolicy;
        private CheckBox checkBoxAllChanges;
        private CheckBox checkBoxModifiedPacks;
        private CheckBox checkBoxAddedPacks;
        private CheckBox checkBoxRemovedPacks;
        private CheckBox checkBoxModifiedElems;
        private CheckBox checkBoxRemovedElems;
        private CheckBox checkBoxAddedElems;
        private Label label1;
        private Label label2;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label9;
        private TextBox textBoxFeatures;
        private Label label10;
        private Label label11;
        private Label label12;
        private TextBox textBoxSvnModel;
        private Button features;
        private Label label13;
        private ComboBox rolesList;
        private Button config;
        private CheckBox checkBoxRemovedAttrs;
        private CheckBox checkBoxAddedAttrs;
        private CheckBox checkBoxModifiedAttrs;
        private Label label3;
        private Label label4;
        private CheckBox checkBoxSizeMetrics;
        private CheckBox checkBoxChangeMetrics;
        private Button reportChanges;
        private Button reportMetrics;
        private Button extractMultiple;
        private Label label14;
        private Button reportFeatures;
        private Label label8;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.textBoxModelPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.extract = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.compare = new System.Windows.Forms.Button();
            this.textBoxModel2 = new System.Windows.Forms.TextBox();
            this.textBoxModel1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.checkBoxRelevancePolicy = new System.Windows.Forms.CheckBox();
            this.load = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxNotRelevant = new System.Windows.Forms.TextBox();
            this.checkBoxAllChanges = new System.Windows.Forms.CheckBox();
            this.checkBoxModifiedPacks = new System.Windows.Forms.CheckBox();
            this.checkBoxAddedPacks = new System.Windows.Forms.CheckBox();
            this.checkBoxRemovedPacks = new System.Windows.Forms.CheckBox();
            this.checkBoxModifiedElems = new System.Windows.Forms.CheckBox();
            this.checkBoxRemovedElems = new System.Windows.Forms.CheckBox();
            this.checkBoxAddedElems = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxFeatures = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.textBoxSvnModel = new System.Windows.Forms.TextBox();
            this.features = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.rolesList = new System.Windows.Forms.ComboBox();
            this.config = new System.Windows.Forms.Button();
            this.checkBoxRemovedAttrs = new System.Windows.Forms.CheckBox();
            this.checkBoxAddedAttrs = new System.Windows.Forms.CheckBox();
            this.checkBoxModifiedAttrs = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBoxSizeMetrics = new System.Windows.Forms.CheckBox();
            this.checkBoxChangeMetrics = new System.Windows.Forms.CheckBox();
            this.reportChanges = new System.Windows.Forms.Button();
            this.reportMetrics = new System.Windows.Forms.Button();
            this.extractMultiple = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.reportFeatures = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxModelPath
            // 
            this.textBoxModelPath.Location = new System.Drawing.Point(84, 20);
            this.textBoxModelPath.Name = "textBoxModelPath";
            this.textBoxModelPath.Size = new System.Drawing.Size(449, 20);
            this.textBoxModelPath.TabIndex = 0;
            this.textBoxModelPath.Text = "C:\\Users\\maxime\\Desktop\\ModelicaResults\\Metamodels\\OMCompiler-1.9.2\\Compiler\\Fron" +
    "tEnd";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Model Path:";
            // 
            // extract
            // 
            this.extract.Location = new System.Drawing.Point(558, 20);
            this.extract.Name = "extract";
            this.extract.Size = new System.Drawing.Size(70, 46);
            this.extract.TabIndex = 4;
            this.extract.Text = "Extract";
            this.extract.Click += new System.EventHandler(this.extract_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "File Path:";
            // 
            // textBoxFilePath
            // 
            this.textBoxFilePath.Location = new System.Drawing.Point(84, 46);
            this.textBoxFilePath.Name = "textBoxFilePath";
            this.textBoxFilePath.Size = new System.Drawing.Size(449, 20);
            this.textBoxFilePath.TabIndex = 5;
            this.textBoxFilePath.Text = "C:\\Users\\maxime\\Desktop\\ModelicaResults\\XML\\1.9.2.xml";
            // 
            // compare
            // 
            this.compare.Location = new System.Drawing.Point(677, 106);
            this.compare.Name = "compare";
            this.compare.Size = new System.Drawing.Size(70, 46);
            this.compare.TabIndex = 7;
            this.compare.Text = "Compare";
            this.compare.Click += new System.EventHandler(this.compare_Click);
            // 
            // textBoxModel2
            // 
            this.textBoxModel2.Location = new System.Drawing.Point(84, 132);
            this.textBoxModel2.Name = "textBoxModel2";
            this.textBoxModel2.Size = new System.Drawing.Size(449, 20);
            this.textBoxModel2.TabIndex = 14;
            this.textBoxModel2.Text = "C:\\Users\\maxime\\Desktop\\ModelicaResults\\XML\\1.9.3.xml";
            // 
            // textBoxModel1
            // 
            this.textBoxModel1.Location = new System.Drawing.Point(84, 106);
            this.textBoxModel1.Name = "textBoxModel1";
            this.textBoxModel1.Size = new System.Drawing.Size(449, 20);
            this.textBoxModel1.TabIndex = 15;
            this.textBoxModel1.Text = "C:\\Users\\maxime\\Desktop\\ModelicaResults\\XML\\1.9.2.xml";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 109);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Model path 1:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 135);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Model path 2:";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 324);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(970, 352);
            this.richTextBox1.TabIndex = 21;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // checkBoxRelevancePolicy
            // 
            this.checkBoxRelevancePolicy.AutoSize = true;
            this.checkBoxRelevancePolicy.Enabled = false;
            this.checkBoxRelevancePolicy.Location = new System.Drawing.Point(551, 165);
            this.checkBoxRelevancePolicy.Name = "checkBoxRelevancePolicy";
            this.checkBoxRelevancePolicy.Size = new System.Drawing.Size(91, 17);
            this.checkBoxRelevancePolicy.TabIndex = 22;
            this.checkBoxRelevancePolicy.Text = "Relevant only";
            this.checkBoxRelevancePolicy.UseVisualStyleBackColor = true;
            // 
            // load
            // 
            this.load.Location = new System.Drawing.Point(558, 106);
            this.load.Name = "load";
            this.load.Size = new System.Drawing.Size(70, 46);
            this.load.TabIndex = 23;
            this.load.Text = "Load";
            this.load.Click += new System.EventHandler(this.load_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 287);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Config:";
            // 
            // textBoxNotRelevant
            // 
            this.textBoxNotRelevant.Location = new System.Drawing.Point(84, 284);
            this.textBoxNotRelevant.Name = "textBoxNotRelevant";
            this.textBoxNotRelevant.Size = new System.Drawing.Size(449, 20);
            this.textBoxNotRelevant.TabIndex = 24;
            this.textBoxNotRelevant.Text = "C:\\Users\\maxime\\Desktop\\ModelicaResults\\Config\\MM_Config.xml";
            // 
            // checkBoxAllChanges
            // 
            this.checkBoxAllChanges.AutoSize = true;
            this.checkBoxAllChanges.Location = new System.Drawing.Point(877, 232);
            this.checkBoxAllChanges.Name = "checkBoxAllChanges";
            this.checkBoxAllChanges.Size = new System.Drawing.Size(81, 17);
            this.checkBoxAllChanges.TabIndex = 26;
            this.checkBoxAllChanges.Text = "All changes";
            this.checkBoxAllChanges.UseVisualStyleBackColor = true;
            // 
            // checkBoxModifiedPacks
            // 
            this.checkBoxModifiedPacks.AutoSize = true;
            this.checkBoxModifiedPacks.Location = new System.Drawing.Point(666, 256);
            this.checkBoxModifiedPacks.Name = "checkBoxModifiedPacks";
            this.checkBoxModifiedPacks.Size = new System.Drawing.Size(98, 17);
            this.checkBoxModifiedPacks.TabIndex = 27;
            this.checkBoxModifiedPacks.Text = "Modified packs";
            this.checkBoxModifiedPacks.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddedPacks
            // 
            this.checkBoxAddedPacks.AutoSize = true;
            this.checkBoxAddedPacks.Location = new System.Drawing.Point(666, 272);
            this.checkBoxAddedPacks.Name = "checkBoxAddedPacks";
            this.checkBoxAddedPacks.Size = new System.Drawing.Size(89, 17);
            this.checkBoxAddedPacks.TabIndex = 28;
            this.checkBoxAddedPacks.Text = "Added packs";
            this.checkBoxAddedPacks.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemovedPacks
            // 
            this.checkBoxRemovedPacks.AutoSize = true;
            this.checkBoxRemovedPacks.Location = new System.Drawing.Point(666, 288);
            this.checkBoxRemovedPacks.Name = "checkBoxRemovedPacks";
            this.checkBoxRemovedPacks.Size = new System.Drawing.Size(104, 17);
            this.checkBoxRemovedPacks.TabIndex = 29;
            this.checkBoxRemovedPacks.Text = "Removed packs";
            this.checkBoxRemovedPacks.UseVisualStyleBackColor = true;
            // 
            // checkBoxModifiedElems
            // 
            this.checkBoxModifiedElems.AutoSize = true;
            this.checkBoxModifiedElems.Location = new System.Drawing.Point(774, 255);
            this.checkBoxModifiedElems.Name = "checkBoxModifiedElems";
            this.checkBoxModifiedElems.Size = new System.Drawing.Size(96, 17);
            this.checkBoxModifiedElems.TabIndex = 30;
            this.checkBoxModifiedElems.Text = "Modified elems";
            this.checkBoxModifiedElems.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemovedElems
            // 
            this.checkBoxRemovedElems.AutoSize = true;
            this.checkBoxRemovedElems.Location = new System.Drawing.Point(774, 288);
            this.checkBoxRemovedElems.Name = "checkBoxRemovedElems";
            this.checkBoxRemovedElems.Size = new System.Drawing.Size(102, 17);
            this.checkBoxRemovedElems.TabIndex = 32;
            this.checkBoxRemovedElems.Text = "Removed elems";
            this.checkBoxRemovedElems.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddedElems
            // 
            this.checkBoxAddedElems.AutoSize = true;
            this.checkBoxAddedElems.Location = new System.Drawing.Point(774, 272);
            this.checkBoxAddedElems.Name = "checkBoxAddedElems";
            this.checkBoxAddedElems.Size = new System.Drawing.Size(87, 17);
            this.checkBoxAddedElems.TabIndex = 31;
            this.checkBoxAddedElems.Text = "Added elems";
            this.checkBoxAddedElems.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(669, 232);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 13);
            this.label8.TabIndex = 33;
            this.label8.Text = "Show changes:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 225);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 37;
            this.label9.Text = "RfC No(s):";
            // 
            // textBoxFeatures
            // 
            this.textBoxFeatures.Enabled = false;
            this.textBoxFeatures.Location = new System.Drawing.Point(84, 222);
            this.textBoxFeatures.Name = "textBoxFeatures";
            this.textBoxFeatures.Size = new System.Drawing.Size(449, 20);
            this.textBoxFeatures.TabIndex = 36;
            this.textBoxFeatures.Text = "Not available";
            // 
            // label10
            // 
            this.label10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label10.Location = new System.Drawing.Point(12, 86);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(970, 2);
            this.label10.TabIndex = 39;
            // 
            // label11
            // 
            this.label11.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label11.Location = new System.Drawing.Point(12, 173);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(520, 2);
            this.label11.TabIndex = 40;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(9, 199);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(56, 13);
            this.label12.TabIndex = 42;
            this.label12.Text = "SVN path:";
            // 
            // textBoxSvnModel
            // 
            this.textBoxSvnModel.Enabled = false;
            this.textBoxSvnModel.Location = new System.Drawing.Point(84, 196);
            this.textBoxSvnModel.Name = "textBoxSvnModel";
            this.textBoxSvnModel.Size = new System.Drawing.Size(449, 20);
            this.textBoxSvnModel.TabIndex = 41;
            this.textBoxSvnModel.Text = "Not available";
            // 
            // features
            // 
            this.features.Enabled = false;
            this.features.Location = new System.Drawing.Point(558, 196);
            this.features.Name = "features";
            this.features.Size = new System.Drawing.Size(70, 46);
            this.features.TabIndex = 43;
            this.features.Text = "Features";
            this.features.Click += new System.EventHandler(this.features_Click);
            // 
            // label13
            // 
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label13.Location = new System.Drawing.Point(12, 255);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(630, 2);
            this.label13.TabIndex = 44;
            // 
            // rolesList
            // 
            this.rolesList.FormattingEnabled = true;
            this.rolesList.Items.AddRange(new object[] {
            "Entire model"});
            this.rolesList.Location = new System.Drawing.Point(769, 121);
            this.rolesList.Name = "rolesList";
            this.rolesList.Size = new System.Drawing.Size(207, 21);
            this.rolesList.TabIndex = 45;
            this.rolesList.SelectedIndexChanged += new System.EventHandler(this.rolesList_SelectedIndexChanged);
            // 
            // config
            // 
            this.config.Location = new System.Drawing.Point(558, 284);
            this.config.Name = "config";
            this.config.Size = new System.Drawing.Size(70, 20);
            this.config.TabIndex = 46;
            this.config.Text = "Read";
            this.config.Click += new System.EventHandler(this.config_Click);
            // 
            // checkBoxRemovedAttrs
            // 
            this.checkBoxRemovedAttrs.AutoSize = true;
            this.checkBoxRemovedAttrs.Location = new System.Drawing.Point(877, 288);
            this.checkBoxRemovedAttrs.Name = "checkBoxRemovedAttrs";
            this.checkBoxRemovedAttrs.Size = new System.Drawing.Size(95, 17);
            this.checkBoxRemovedAttrs.TabIndex = 49;
            this.checkBoxRemovedAttrs.Text = "Removed attrs";
            this.checkBoxRemovedAttrs.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddedAttrs
            // 
            this.checkBoxAddedAttrs.AutoSize = true;
            this.checkBoxAddedAttrs.Location = new System.Drawing.Point(877, 272);
            this.checkBoxAddedAttrs.Name = "checkBoxAddedAttrs";
            this.checkBoxAddedAttrs.Size = new System.Drawing.Size(80, 17);
            this.checkBoxAddedAttrs.TabIndex = 48;
            this.checkBoxAddedAttrs.Text = "Added attrs";
            this.checkBoxAddedAttrs.UseVisualStyleBackColor = true;
            // 
            // checkBoxModifiedAttrs
            // 
            this.checkBoxModifiedAttrs.AutoSize = true;
            this.checkBoxModifiedAttrs.Location = new System.Drawing.Point(877, 256);
            this.checkBoxModifiedAttrs.Name = "checkBoxModifiedAttrs";
            this.checkBoxModifiedAttrs.Size = new System.Drawing.Size(89, 17);
            this.checkBoxModifiedAttrs.TabIndex = 47;
            this.checkBoxModifiedAttrs.Text = "Modified attrs";
            this.checkBoxModifiedAttrs.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label3.Location = new System.Drawing.Point(651, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(2, 216);
            this.label3.TabIndex = 50;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(670, 186);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 51;
            this.label4.Text = "Show metrics:";
            // 
            // checkBoxSizeMetrics
            // 
            this.checkBoxSizeMetrics.AutoSize = true;
            this.checkBoxSizeMetrics.Location = new System.Drawing.Point(774, 186);
            this.checkBoxSizeMetrics.Name = "checkBoxSizeMetrics";
            this.checkBoxSizeMetrics.Size = new System.Drawing.Size(46, 17);
            this.checkBoxSizeMetrics.TabIndex = 52;
            this.checkBoxSizeMetrics.Text = "Size";
            this.checkBoxSizeMetrics.UseVisualStyleBackColor = true;
            // 
            // checkBoxChangeMetrics
            // 
            this.checkBoxChangeMetrics.AutoSize = true;
            this.checkBoxChangeMetrics.Location = new System.Drawing.Point(774, 232);
            this.checkBoxChangeMetrics.Name = "checkBoxChangeMetrics";
            this.checkBoxChangeMetrics.Size = new System.Drawing.Size(60, 17);
            this.checkBoxChangeMetrics.TabIndex = 57;
            this.checkBoxChangeMetrics.Text = "Metrics";
            this.checkBoxChangeMetrics.UseVisualStyleBackColor = true;
            // 
            // reportChanges
            // 
            this.reportChanges.Location = new System.Drawing.Point(785, 45);
            this.reportChanges.Name = "reportChanges";
            this.reportChanges.Size = new System.Drawing.Size(92, 23);
            this.reportChanges.TabIndex = 58;
            this.reportChanges.Text = "Report changes";
            this.reportChanges.UseVisualStyleBackColor = true;
            this.reportChanges.Click += new System.EventHandler(this.reportChanges_Click);
            // 
            // reportMetrics
            // 
            this.reportMetrics.Location = new System.Drawing.Point(887, 17);
            this.reportMetrics.Name = "reportMetrics";
            this.reportMetrics.Size = new System.Drawing.Size(92, 23);
            this.reportMetrics.TabIndex = 59;
            this.reportMetrics.Text = "Report metrics";
            this.reportMetrics.UseVisualStyleBackColor = true;
            this.reportMetrics.Click += new System.EventHandler(this.reportMetrics_Click);
            // 
            // extractMultiple
            // 
            this.extractMultiple.Location = new System.Drawing.Point(785, 16);
            this.extractMultiple.Name = "extractMultiple";
            this.extractMultiple.Size = new System.Drawing.Size(92, 23);
            this.extractMultiple.TabIndex = 60;
            this.extractMultiple.Text = "Extract multiple";
            this.extractMultiple.UseVisualStyleBackColor = true;
            this.extractMultiple.Click += new System.EventHandler(this.extractMultiple_Click);
            // 
            // label14
            // 
            this.label14.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label14.Location = new System.Drawing.Point(765, 10);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(2, 69);
            this.label14.TabIndex = 61;
            // 
            // reportFeatures
            // 
            this.reportFeatures.Enabled = false;
            this.reportFeatures.Location = new System.Drawing.Point(887, 46);
            this.reportFeatures.Name = "reportFeatures";
            this.reportFeatures.Size = new System.Drawing.Size(92, 23);
            this.reportFeatures.TabIndex = 62;
            this.reportFeatures.Text = "Report features";
            this.reportFeatures.UseVisualStyleBackColor = true;
            this.reportFeatures.Click += new System.EventHandler(this.reportFeatures_Click);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(994, 688);
            this.Controls.Add(this.reportFeatures);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.extractMultiple);
            this.Controls.Add(this.reportMetrics);
            this.Controls.Add(this.reportChanges);
            this.Controls.Add(this.checkBoxChangeMetrics);
            this.Controls.Add(this.checkBoxSizeMetrics);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBoxRemovedAttrs);
            this.Controls.Add(this.checkBoxAddedAttrs);
            this.Controls.Add(this.checkBoxModifiedAttrs);
            this.Controls.Add(this.config);
            this.Controls.Add(this.rolesList);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.features);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.textBoxSvnModel);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBoxFeatures);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.checkBoxRemovedElems);
            this.Controls.Add(this.checkBoxAddedElems);
            this.Controls.Add(this.checkBoxModifiedElems);
            this.Controls.Add(this.checkBoxRemovedPacks);
            this.Controls.Add(this.checkBoxAddedPacks);
            this.Controls.Add(this.checkBoxModifiedPacks);
            this.Controls.Add(this.checkBoxAllChanges);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxNotRelevant);
            this.Controls.Add(this.load);
            this.Controls.Add(this.checkBoxRelevancePolicy);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxModel1);
            this.Controls.Add(this.textBoxModel2);
            this.Controls.Add(this.compare);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxFilePath);
            this.Controls.Add(this.extract);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxModelPath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Modelica Change Analyzer (forked from ARCA)";
            this.TransparencyKey = System.Drawing.Color.Silver;
            this.Activated += new System.EventHandler(this.EA_Dump_Form_Resize);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.EA_Dump_Form_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}