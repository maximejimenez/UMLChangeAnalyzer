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
        private String type = "";
        private String name = "";
        private String upperBound = "";
        private String lowerBound = "";

        // Changes
        //TODO

        #region Constructors

        public Attribute(string type, string name, string upperBound, string lowerBound)
        {
            this.type = type;
            this.name = name;
            this.upperBound = upperBound;
            this.lowerBound = lowerBound;
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


        /*public List<ARChange> Changes
        {
            get { return changes; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
        }*/


        #endregion

    }
}
