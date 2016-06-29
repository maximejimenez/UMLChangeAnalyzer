using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ModelicaChangeAnalyzer.Changes
{
    public class Change : TreeNode
    {
        private string description;
        private bool printOnly;

        public Change(string description, bool printOnly)
        {
            this.printOnly = printOnly;
            this.description = description;
        }

        public Change AppendTabs(int numOfTabs)
        {
            string tabs = "";

            for (int i = 0; i < numOfTabs; i++)
                tabs += "\t";

            Change retChange = new Change(tabs + description, printOnly);

            return retChange;
        }

        public override string ToString()
        {
            return description;
        }

        #region Getters and setters

        public string Description
        {
            get { return description; }
        }

        public bool PrintOnly
        {
            get { return printOnly; }
        }

        #endregion
    }
}
