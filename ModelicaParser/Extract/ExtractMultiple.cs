using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ModelicaChangeAnalyzer;
using ModelicaChangeAnalyzer.Config;

namespace ModelicaChangeAnalyzer.Extract
{
    // extracting multiple AUTOSAR meta-model versions
    class ExtractMultiple
    {
        private MainForm form;
        private string[] versions;          // list of versions to be extracted
        private string[] releases;          // list of releases to be extracted (their paths)

        #region Extraction

        // creation of the extract multiple which automatically starts the extraction
        public ExtractMultiple(MainForm form)
        {
            this.form = form;

            try
            {
                bool validates = ConfigReader.Read(form.TextBoxNotRelevant1.Text, form);               // configuration for the extract multiple is done based on the config file whose path is provided in the GUI

                if (validates)  // if the XML config file is validated
                {
                    releases = new string[ConfigReader.ExtractMultipleReleases.Count];
                    versions = new string[ConfigReader.ExtractMultipleReleases.Count];

                    // TODO : handle full directory or multiple file from the front end compiler
                    for (int i = 0; i < ConfigReader.ExtractMultipleReleases.Count; i++)
                        releases[i] = ConfigReader.ExtractMultipleReleases[i][0];              // getting all release paths from the config file

                    for (int i = 0; i < ConfigReader.ExtractMultipleReleases.Count; i++)
                        versions[i] = ConfigReader.ExtractMultipleReleases[i][1];              // getting all release paths from the config file

                    form.ListAdd("Release paths successfully read.");

                    ExtractModels();            // automated extraction
                }
                else
                    form.ListAdd("Config file not read.");
            }
            catch (Exception exp)
            {
                form.ListAdd(exp.ToString());
            }
        }

        // extracting multiple meta-models
        private void ExtractModels()
        {
            for (int i = 0; i < releases.Length; i++)
            {
                string modelPath = releases[i];
                string version = versions[i];
                string filePath = Path.Combine(ConfigReader.ExtractPath, version + ".xml");

                if (File.Exists(filePath))      // the extraction is not done if the file with the same name exists
                    form.ListAdd("File " + filePath + " already exists.");
                else
                {
                    Extractor extractor = new Extractor(form);
                    form.ListAdd("Dumping " + version);
                    extractor.ExtractModel(modelPath, filePath, version);
                }
            }

            form.ListAdd("Done!");
        }

        #endregion
    }
}