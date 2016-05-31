using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModelicaParser.Datamodel
{
    class Connector : ICloneable
    {
        // Backtracking
        private Element source = null;
        private Element target = null;

        // Attributes
        private String type = "";
        private String sourceCardinality = "";
        private String targetCardinality = "";
        private String UID;

        // Changes
        //TODO

        #region Constructors

        public Connector()
        {
            this.UID = generateUID(10);
        }

        public Connector(string type, string sourceCardinality, string targetCardinality)
        {
            this.type = type;
            this.sourceCardinality = sourceCardinality;
            this.targetCardinality = targetCardinality;
            this.UID = generateUID(10);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
        private static string generateUID(int length)
        {
            const string alphanumericCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                                  "abcdefghijklmnopqrstuvwxyz" +
                                                  "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(alphanumericCharacters, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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




        #endregion
    }
}
