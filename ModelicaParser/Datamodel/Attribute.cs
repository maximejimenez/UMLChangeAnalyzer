using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModelicaParser.Datamodel
{
    class Attribute
    {
        // Backtracking
        public Element parent;

        // Attributes
        public String type;
        public String name;
        public String upperBound;
        public String lowerBound;

        public Attribute(string t, string n, string u, string l)
        {
            type = t;
            name = n;
            upperBound = u;
            lowerBound = l;
        }

    }
}
