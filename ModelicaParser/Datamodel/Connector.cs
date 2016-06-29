using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using ModelicaChangeAnalyzer.Config;
using ModelicaChangeAnalyzer.Changes;

namespace ModelicaChangeAnalyzer.Datamodel
{
    public class Connector : ICloneable
    {
        // Backtracking
        private Element parentElement = null;
        private Element source = null;
        private Element target = null;

        // Attributes
        private String type = "";
        private String sourceCardinality = "";
        private String targetCardinality = "";
        private String uid = "";
        private String note = "";

        // Changes
        private int numOfChanges;
        private List<Change> changes = new List<Change>();

        #region Loading

        public Connector(string type, string sourceCardinality, string targetCardinality, string uid)
        {
            this.type = type;
            this.sourceCardinality = sourceCardinality;
            this.targetCardinality = targetCardinality;
            this.uid = uid;
        }

        public Connector(string type, string sourceCardinality, string targetCardinality, string uid, string note)
        {
            this.type = type;
            this.sourceCardinality = sourceCardinality;
            this.targetCardinality = targetCardinality;
            this.uid = uid;
            this.note = note;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion

        #region Calculate number of

        public int NumOfAllModifiableElements(bool RelevantOnly)
        {
            return 1;
        }

        #endregion

        #region Retrieve object

        public List<Change> GetChanges()
        {
            return new List<Change>(changes);
        }

        public string GetPath()
        {
            if(target == null)
                return parentElement.GetPath() + " -> null";
            return source.GetPath() + " -> " + target.GetPath();
        }

        #endregion

        #region Calculation

        public void ResetCalculation()
        {
            numOfChanges = 0;
            changes.Clear();
        }

        public bool IgnoreConector()
        {
            foreach (string str in ConfigReader.ExcludedConnectorTypes)
                if (type.Equals(str))
                    return true;

            return false;
        }

        public int CompareConnectors(Connector oldConnector, bool RelevantOnly, bool target)
        {
            if (RelevantOnly && IgnoreConector())
                return 0;

            string type = "(source)";
            if (target)
                type = "(target)";

            if (!Equals(SourceCardinality, oldConnector.SourceCardinality))
            {
                numOfChanges++;
                changes.Add(new Change("~ Source Cardinality: " + oldConnector.SourceCardinality + " -> " + SourceCardinality, false).AppendTabs(1));
            }

            if (!Equals(TargetCardinality, oldConnector.TargetCardinality))
            {

                numOfChanges++;
                changes.Add(new Change("~ Target Cardinality (" + UID + "): " + oldConnector.TargetCardinality + " -> " + TargetCardinality, false).AppendTabs(1));
            }

            if (((RelevantOnly && !ConfigReader.ExcludedAttributeNote) || !RelevantOnly) && !Equals(note, oldConnector.Note))
            {
                numOfChanges++;
                changes.Add(new Change("~ Note", false).AppendTabs(1));
            }

            if (numOfChanges > 0)
                changes.Insert(0, new Change("~ Connector " + type + " " + GetPath(), false));

            return numOfChanges;
        }

        #endregion

        #region Getters and setters

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string SourceCardinality
        {
            get { return sourceCardinality; }
            set { sourceCardinality = value; }
        }

        public string TargetCardinality
        {
            get { return targetCardinality; }
            set { targetCardinality = value; }
        }

        public string UID
        {
            get { return uid; }
            set { uid = value; }
        }

        public string Note
        {
            get { return note; }
            set { note = value; }
        }

        public Element ParentElement
        {
            get { return parentElement; }
            set { parentElement = value; }
        }

        public Element Source
        {
            get { return source; }
            set { source = value; }
        }

        public Element Target
        {
            get { return target; }
            set { target = value; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
        }

        #endregion
    }
}
