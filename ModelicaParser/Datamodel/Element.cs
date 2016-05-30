using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModelicaParser.Datamodel
{
    class Element
    {
        // Backtracking
        public Package package = null;
        public Element parent = null;

        // Attributes
        public String type;
        public String name;
        public List<Element> children;
        public List<Connector> sourceConnectors;
        public List<Connector> targetConnectors;
        public List<Attribute> attributes;

        public Element(string t, string n)
        {
            type = t;
            name = n;
            children = new List<Element>();
            sourceConnectors = new List<Connector>();
            targetConnectors = new List<Connector>();
            attributes = new List<Attribute>();
        }

        public void AddChildren(Element elem)
        {
            children.Add(elem);
        }

        public void AddAttribute(Attribute attribute)
        {
            attributes.Add(attribute);
        }

        public void AddSourceConnector(Connector sourceConnector)
        {
            sourceConnectors.Add(sourceConnector);
        }

        public void AddTargetConnector(Connector targetConnector)
        {
            targetConnectors.Add(targetConnector);
        }

        public void Compare(Element newElement)
        {
            //TODO
        }
    }
}
