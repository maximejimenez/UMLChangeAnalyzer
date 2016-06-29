using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using ModelicaChangeAnalyzer.Changes;
using ModelicaChangeAnalyzer.Config;

namespace ModelicaChangeAnalyzer.Datamodel
{
    public class Attribute //: IEquatable<Attribute>
    {
        // Backtracking
        private Element parentElement;

        // Attributes
        private String type = "";
        private String name = "";
        private String upperBound = "";
        private String lowerBound = "";
        private String note = "";

        // Changes
        private int numOfChanges;
        private List<Change> changes = new List<Change>();

        #region Loading

        public Attribute(string type, string name, string upperBound, string lowerBound)
        {
            this.type = type;
            this.name = name;
            this.upperBound = upperBound;
            this.lowerBound = lowerBound;
        }

        public Attribute(string type, string name, string upperBound, string lowerBound, string note)
        {
            this.type = type;
            this.name = name;
            this.upperBound = upperBound;
            this.lowerBound = lowerBound;
            this.note = note;
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
            return parentElement.GetPath() + "::" + name;
        }

        #endregion

        #region Calculation

        public void ResetCalculation()
        {
            numOfChanges = 0;
            changes.Clear();
        }

        public bool IgnoreAttribute()
        {
            return false;
        }

        public int CompareAttributes(Attribute oldAttribute, bool RelevantOnly)
        {
            if (RelevantOnly && IgnoreAttribute())
                return 0;

            if (!Equals(type, oldAttribute.Type))
            {
                numOfChanges++;
                changes.Add(new Change("~ Type: " + oldAttribute.Type + " -> " + type, false).AppendTabs(1));
            }

            if (!Equals(name, oldAttribute.Name))
            {
                numOfChanges++;
                changes.Add(new Change("~ Name: " + oldAttribute.Name + " -> " + name, false).AppendTabs(1));
            }

            if (!Equals(lowerBound, oldAttribute.LowerBound))
            {
                numOfChanges++;
                changes.Add(new Change("~ Lower Bound: " + oldAttribute.LowerBound + " -> " + lowerBound, false).AppendTabs(1));
            }

            if (!Equals(upperBound, oldAttribute.UpperBound))
            {
                numOfChanges++;
                changes.Add(new Change("~ Upper Bound: " + oldAttribute.UpperBound + " -> " + upperBound, false).AppendTabs(1));
            }

            if (((RelevantOnly && !ConfigReader.ExcludedAttributeNote) || !RelevantOnly) && !Equals(note, oldAttribute.Note))
            {
                numOfChanges++;
                changes.Add(new Change("~ Note", false).AppendTabs(1));
            }

            if(numOfChanges > 0)
                changes.Insert(0, new Change("~ Attribute " + GetPath(), false));

            return numOfChanges;
        }

        #endregion

        #region Getters and setters

        public Element ParentElement
        {
            get { return parentElement; }
            set { parentElement = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string LowerBound
        {
            get { return lowerBound; }
            set { lowerBound = value; }
        }

        public string UpperBound
        {
            get { return upperBound; }
            set { upperBound = value; }
        }

        public string Note
        {
            get { return note; }
            set { note = value; }
        }

        public List<Change> Changes
        {
            get { return changes; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
        }

        #endregion

    }
}
