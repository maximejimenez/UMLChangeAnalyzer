using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModelicaParser.Datamodel
{
    class Attribute //: IEquatable<Attribute>
    {
        // Backtracking
        private Element parentElement;

        // Attributes
        private String name = "";
        private String note = "";
        private String type = "";
        private String upperBound = "";
        private String lowerBound = "";

        // Changes
        private int numOfChanges;
        private List<MMChange> changes = new List<MMChange>();

        #region Loading

        public Attribute(string type, string name, string upperBound, string lowerBound)
        {
            this.type = type;
            this.name = name;
            this.upperBound = upperBound;
            this.lowerBound = lowerBound;
        }

        #endregion

        #region Calculate number of

        public int NumOfAllModifiableElements(bool RelevantOnly)
        {
            return 1;
        }

        #endregion

        #region Retrieve object

        public List<MMChange> GetChanges()
        {
            return new List<MMChange>(changes);
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

            if (!Equals(name, oldAttribute.Name))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Name: " + oldAttribute.Name + " -> " + name, false));
            }

            if (((RelevantOnly && !ConfigReader.ExcludedAttributeNote) || !RelevantOnly) && !Equals(note, oldAttribute.Note))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Note", false));
            }

            if (!Equals(type, oldAttribute.Type))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Type: " + oldAttribute.Type + " -> " + type, false));
            }

            if (!Equals(lowerBound, oldAttribute.LowerBound))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Lower Bound: " + oldAttribute.LowerBound + " -> " + lowerBound, false));
            }

            if (!Equals(upperBound, oldAttribute.UpperBound))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Upper Bound: " + oldAttribute.UpperBound + " -> " + upperBound, false));
            }

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

        public List<MMChange> Changes
        {
            get { return changes; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
        }


        #endregion

        /*
        public bool Equals(Attribute attribute)
        {
            if (attribute == null)
                return false;

            else
                return Equals(attribute);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            Attribute attribute = obj as Attribute;
            if (attribute == null)
                return false;
            else
                return Equals(attribute);
        }
        
        public static bool operator ==(Attribute attr1, Attribute attr2)
        {
            if (attr1 == null || attr2 == null)
                return false;

            return attr1.type == attr2.type 
                && attr1.name == attr2.name 
                && attr1.upperBound == attr2.upperBound 
                && attr1.lowerBound == attr2.lowerBound;
        }
        */

    }
}
