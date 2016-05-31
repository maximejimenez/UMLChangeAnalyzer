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
        private Package parentPackage = null;
        private Element parentElement = null;

        // Attributes
        private String type = "";
        private String name = "";
        private List<Element> children = new List<Element>();
        private List<Connector> sourceConnectors = new List<Connector>();
        private List<Connector> targetConnectors = new List<Connector>();
        private List<Attribute> attributes = new List<Attribute>();

        // Changes
        //TODO

        #region Constructors

        public Element(string type, string name)
        {
            this.type = type;
            this.name = name;
        }

        #endregion

        public void Compare(Element newElement)
        {
            //TODO
        }

        #region Getters and setters

        public Package ParentPackage
        {
            get { return parentPackage; }
            set { parentPackage = value; }
        }

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

        public void AddChildren(Element element)
        {
            children.Add(element);
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

        #endregion
    }
}
