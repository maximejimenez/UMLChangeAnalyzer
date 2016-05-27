using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModelicaParser.Datamodel
{
    class Connector : ISerializable
    {
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
