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
        public Element source;
        public Element target;

        // Attributes
        public String type;
        public String sourceCardinality;
        public String targetCardinality;
        public String UID;

        public Connector(string t, string sC, string tC)
        {
            type = t;
            sourceCardinality = sC;
            targetCardinality = tC;
            UID = generateUID(10);
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
    }
}
