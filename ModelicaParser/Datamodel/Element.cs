using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using ModelicaParser.Config;
using ModelicaParser.Changes;

namespace ModelicaParser.Datamodel
{
    public class Element
    {
        // Backtracking
        private Package parentPackage = null;
        private Element parentElement = null;

        // Attributes
        private String type = "";
        private String name = "";
        private String note = "";
        private List<Element> children = new List<Element>();
        private List<Connector> sourceConnectors = new List<Connector>();
        private List<Connector> targetConnectors = new List<Connector>();
        private List<Attribute> attributes = new List<Attribute>();

        // Changes
        private int numOfChanges;
        private List<MMChange> changes = new List<MMChange>();
        private List<Element> modifiedElements = new List<Element>();
        private List<Element> removedElements = new List<Element>();
        private List<Element> addedElements = new List<Element>();
        private List<Attribute> modifiedAttributes = new List<Attribute>();
        private List<Attribute> removedAttributes = new List<Attribute>();
        private List<Attribute> addedAttributes = new List<Attribute>();
        private List<Connector> modifiedConnectors = new List<Connector>();
        private List<Connector> removedConnectors = new List<Connector>();
        private List<Connector> addedConnectors = new List<Connector>();

        #region Loading

        public Element(string type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public Element(string type, string name, string note)
        {
            this.type = type;
            this.name = name;
            this.note = note;
        }

        public void AddChild(Element element)
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

        public override string ToString()
        {
            string ret = "Element " + name + "\n";

            foreach (Attribute attr in attributes)
                ret += attr.ToString();

            foreach (Connector conn in sourceConnectors)
                ret += conn.ToString();
            foreach (Connector conn in targetConnectors)
                ret += conn.ToString();

            return ret;
        }

        #endregion

        #region Calculate number of


        public int NumberOfElements()
        {
            int numberOfElements = 1;

            foreach(Element elem in Children)
                numberOfElements += elem.NumberOfElements();

            return numberOfElements;
        }

        public int NumberOfAttributes(bool relevantOnly)
        {
            if (relevantOnly && IgnoreElement())
                return 0;

            int numberOfAttributes = 0;

            foreach (Attribute attr in attributes)
                if (!(relevantOnly && attr.IgnoreAttribute()))
                    numberOfAttributes++;

            foreach (Element elem in children)
                if (!(relevantOnly && elem.IgnoreElement()))
                    numberOfAttributes += elem.NumberOfAttributes(relevantOnly);

            return numberOfAttributes;
        }

        public int NumberOfConnectors(bool relevantOnly)
        {
            if (relevantOnly && IgnoreElement())
                return 0;

            int numberOfConnectors = sourceConnectors.Count + targetConnectors.Count;

            foreach (Element elem in children)
                if (!(relevantOnly && elem.IgnoreElement()))
                    numberOfConnectors += elem.NumberOfConnectors(relevantOnly);

            return numberOfConnectors;
        }

        public int NumOfAllModifiableElements(bool RelevantOnly)
        {
            int modifiableElems = 1;

            foreach (Element child in children)
            {
                modifiableElems += child.NumOfAllModifiableElements(RelevantOnly);
            }

            foreach (Attribute attr in attributes)
            {
                if (RelevantOnly && attr.IgnoreAttribute())
                    continue;

                modifiableElems += attr.NumOfAllModifiableElements(RelevantOnly);
            }

            foreach (Connector conn in sourceConnectors)
            {
                if (RelevantOnly && conn.IgnoreConector())
                    continue;

                modifiableElems += conn.NumOfAllModifiableElements(RelevantOnly);
            }

            foreach (Connector conn in targetConnectors)
            {
                if (RelevantOnly && conn.IgnoreConector())
                    continue;

                modifiableElems += conn.NumOfAllModifiableElements(RelevantOnly);
            }

            return modifiableElems;
        }

        public int NumberOfAddedElements()
        {
            int numberOfAddedElements = addedElements.Count;
            foreach (Element child in children)
                numberOfAddedElements += child.NumberOfAddedElements();
            return numberOfAddedElements;
        }

        public int NumberOfModifiedElements()
        {
            int numberOfModifiedElements = modifiedElements.Count;
            foreach (Element child in children)
                numberOfModifiedElements += child.NumberOfModifiedElements();
            return numberOfModifiedElements;
        }

        public int NumberOfRemovedElements()
        {
            int numberOfRemovedElements = removedElements.Count;
            foreach (Element child in children)
                numberOfRemovedElements += child.NumberOfRemovedElements();
            return numberOfRemovedElements;
        }

        public int NumberOfModifiedConnectors()
        {
            int numberOfModifiedConnectors = modifiedConnectors.Count;

            foreach (Element elem in modifiedElements)
                numberOfModifiedConnectors += elem.NumberOfModifiedConnectors();

            return numberOfModifiedConnectors;
        }

        public int NumberOfAddedConnectors()
        {
            int numberOfAddedConnectors = addedConnectors.Count;

            foreach (Element elem in AddedElements)
                numberOfAddedConnectors += elem.NumberOfConnectors(false);

            foreach (Element elem in ModifiedElements)
                numberOfAddedConnectors += elem.NumberOfAddedConnectors();

            return numberOfAddedConnectors;
        }

        public int NumberOfRemovedConnectors()
        {
            int numberOfRemovedConnectors = removedConnectors.Count;

            foreach (Element elem in RemovedElements)
                numberOfRemovedConnectors += elem.NumberOfConnectors(false);

            foreach (Element elem in ModifiedElements)
                numberOfRemovedConnectors += elem.NumberOfAddedConnectors();

            return numberOfRemovedConnectors;
        }

        public int NumberOfModifiedAttributes()
        {
            int numberOfModifiedAttributes = modifiedAttributes.Count;

            foreach (Element elem in modifiedElements)
                numberOfModifiedAttributes += elem.NumberOfModifiedAttributes();

            return numberOfModifiedAttributes;
        }

        public int NumberOfAddedAttributes()
        {
            int numberOfAddedAttributes = addedAttributes.Count;

            foreach (Element elem in addedElements)
                numberOfAddedAttributes += elem.NumberOfAttributes(false);

            foreach (Element elem in modifiedElements)
                numberOfAddedAttributes += elem.NumberOfAddedAttributes();

            return numberOfAddedAttributes;
        }

        public int NumberOfRemovedAttributes()
        {
            int numberOfRemovedAttributes = removedAttributes.Count;

            foreach (Element elem in removedElements)
                numberOfRemovedAttributes += elem.NumberOfAttributes(false);

            foreach (Element elem in modifiedElements)
                numberOfRemovedAttributes += elem.NumberOfRemovedAttributes();

            return numberOfRemovedAttributes;
        }

        #endregion

        #region Retrieve object

        public void printAsAddedElement(List<MMChange> listOfChanges, int indent)
        {
            listOfChanges.Add(new MMChange("+ Element: " + GetPath(), true).AppendTabs(indent++));
            foreach (Attribute addedAttribute in Attributes)
            {
                listOfChanges.Add(new MMChange("+ Attribute: " + addedAttribute.GetPath(), true).AppendTabs(indent));
            }
            foreach (Connector sourceConnector in SourceConnectors)
            {
                listOfChanges.Add(new MMChange("+ Connector (source): " + sourceConnector.GetPath(), true).AppendTabs(indent));
            }
            foreach (Connector targetConnector in TargetConnectors)
            {
                listOfChanges.Add(new MMChange("+ Connector (target):" + targetConnector.GetPath(), true).AppendTabs(indent));
            }
            foreach (Element child in Children)
            {
                child.printAsAddedElement(listOfChanges, indent);
            }
        }

        public void printAsModifiedElement(List<MMChange> listOfChanges, int indent)
        {
            listOfChanges.Add(new MMChange("~ Element: " + GetPath(), true).AppendTabs(indent++));

            foreach (Attribute attr in addedAttributes)
            {
                listOfChanges.Add(new MMChange("+ Attribute :" + attr.GetPath(), true).AppendTabs(indent));
            }
            foreach (Attribute attr in removedAttributes)
            {
                listOfChanges.Add(new MMChange("- Attribute :" + attr.GetPath(), true).AppendTabs(indent));

            }
            foreach (Attribute attr in modifiedAttributes)
            {
                listOfChanges.Add(new MMChange("~ Attribute :" + attr.GetPath(), true).AppendTabs(indent));
                foreach (MMChange chng in attr.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(indent + 1));
            }



            foreach (Connector conn in addedConnectors)
            {
                listOfChanges.Add(new MMChange("+ Connector :" + conn.GetPath(), true).AppendTabs(indent));
            }
            foreach (Connector conn in removedConnectors)
            {
                listOfChanges.Add(new MMChange("- Connector :" + conn.GetPath(), true).AppendTabs(indent));
            }
            foreach (Connector conn in modifiedConnectors)
            {
                listOfChanges.Add(new MMChange("~ Connector :" + conn.GetPath(), true).AppendTabs(indent));
                foreach (MMChange chng in conn.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(indent + 1));
            }

            foreach (Element child in addedElements)
            {
                child.printAsAddedElement(listOfChanges, indent);
            }
            foreach (Element child in removedElements)
            {
                child.printAsRemovedElement(listOfChanges, indent);
            }
            foreach (Element child in modifiedElements)
            {
                child.printAsModifiedElement(listOfChanges, indent);
            }
        }

        public void printAsRemovedElement(List<MMChange> listOfChanges, int indent)
        {
            listOfChanges.Add(new MMChange("- Element: " + GetPath(), true).AppendTabs(indent++));
            foreach (Attribute addedAttribute in Attributes)
            {
                listOfChanges.Add(new MMChange("- Attribute: " + addedAttribute.GetPath(), true).AppendTabs(indent));
            }
            foreach (Connector sourceConnector in SourceConnectors)
            {
                listOfChanges.Add(new MMChange("- Connector (source): " + sourceConnector.GetPath(), true).AppendTabs(indent));
            }
            foreach (Connector targetConnector in TargetConnectors)
            {
                listOfChanges.Add(new MMChange("- Connector (target):" + targetConnector.GetPath(), true).AppendTabs(indent));
            }
            foreach (Element child in Children)
            {
                child.printAsAddedElement(listOfChanges, indent);
            }
        }

        // retrieves all changes
        public List<MMChange> GetChanges()
        {
            List<MMChange> listOfChanges = new List<MMChange>(changes);

            foreach (Attribute attr in attributes)
            {
                //if (attr.NumOfChanges != 0)
                //    listOfChanges.Add(new MMChange("~ Attribute " + attr.GetPath(), true).AppendTabs(1));

                foreach (MMChange chng in attr.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(1));
            }

            foreach (Connector conn in sourceConnectors)
            {
                //if (conn.NumOfChanges != 0)
                //    listOfChanges.Add(new MMChange("~ Connector " + conn.GetPath(), true).AppendTabs(1));

                foreach (MMChange chng in conn.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(1));
            }

            foreach (Connector conn in targetConnectors)
            {
                //if (conn.NumOfChanges != 0)
                //    listOfChanges.Add(new MMChange("~ Connector " + conn.GetPath(), true).AppendTabs(1));

                foreach (MMChange chng in conn.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(1));
            }

            foreach (Element child in children)
            {
                //if (child.NumOfChanges != 0)
                //    listOfChanges.Add(new MMChange("~ Element " + child.GetPath(), true).AppendTabs(1));

                foreach (MMChange chng in child.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(1));
            }

            return listOfChanges;
        }

        // adds all elements of the element to a list
        public List<Element> GetAllElements(List<Element> list)
        {
            foreach (Element child in children)
                list.Add(child);

            return list;
        }

        // adds all modified attributes of the element to a list
        public List<Element> GetAllModifiedElements(List<Element> list)
        {
            foreach (Element elem in modifiedElements)
                list.Add(elem);

            return list;
        }

        // adds all added attributes of the element to a list
        public List<Element> GetAllAddedElements(List<Element> list)
        {
            foreach (Element elem in addedElements)
                list.Add(elem);

            return list;
        }

        // adds all removed attributes of the element to a list
        public List<Element> GetAllRemovedElements(List<Element> list)
        {
            foreach (Element elem in removedElements)
                list.Add(elem);

            return list;
        }

        public void GetAllConnectors(List<Connector> list)
        {
            foreach (Connector conn in sourceConnectors)
                list.Add(conn);

            foreach (Connector conn in targetConnectors)
                list.Add(conn);

            foreach (Element child in children)
                child.GetAllConnectors(list);
        }

        public void GetAllModifiedConnectors(List<Connector> list)
        {
            foreach (Connector conn in modifiedConnectors)
                list.Add(conn);

            foreach (Element elem in modifiedElements)
                elem.GetAllModifiedConnectors(list);
        }

        public void GetAllRemovedConnectors(List<Connector> list)
        {
            foreach (Connector conn in removedConnectors)
                list.Add(conn);

            foreach (Element elem in removedElements)
                elem.GetAllConnectors(list);

            foreach (Element elem in modifiedElements)
                elem.GetAllRemovedConnectors(list);
        }

        public void GetAllAddedConnectors(List<Connector> list)
        {
            foreach (Connector conn in addedConnectors)
                list.Add(conn);

            foreach (Element elem in addedElements)
                elem.GetAllConnectors(list);

            foreach (Element elem in modifiedElements)
                elem.GetAllAddedConnectors(list);
        }

        // adds all attributes of the element to a list
        public List<Attribute> GetAllAttributes(List<Attribute> list)
        {
            foreach (Attribute attr in attributes)
                list.Add(attr);

            foreach (Element elem in Children)
                elem.GetAllAttributes(list);

            return list;
        }

        // adds all modified attributes of the element to a list
        public List<Attribute> GetAllModifiedAttributes(List<Attribute> list)
        {
            foreach (Attribute attr in modifiedAttributes)
                list.Add(attr);

            foreach (Element elem in modifiedElements)
                elem.GetAllModifiedAttributes(list);

            return list;
        }

        // adds all added attributes of the element to a list
        public List<Attribute> GetAllAddedAttributes(List<Attribute> list)
        {
            foreach (Attribute attr in addedAttributes)
                list.Add(attr);

            foreach (Element elem in addedElements)
                elem.GetAllAttributes(list);

            foreach (Element elem in modifiedElements)
                elem.GetAllAddedAttributes(list);

            return list;
        }

        // adds all removed attributes of the element to a list
        public List<Attribute> GetAllRemovedAttributes(List<Attribute> list)
        {
            foreach (Attribute attr in removedAttributes)
                list.Add(attr);

            foreach (Element elem in removedElements)
                elem.GetAllAttributes(list);

            foreach (Element elem in modifiedElements)
                elem.GetAllRemovedAttributes(list);

            return list;
        }

        public Attribute GetAttribute(string name)
        {
            foreach (Attribute attribute in attributes)
                if (attribute.Name.Equals(name))
                    return attribute;

            return null;
        }

        public Element GetChild(string name)
        {
            foreach(Element child in children){
                if (child.Name == name)
                    return child;
            }
            return null;
        }

        public Connector GetSourceConnector(string uid)
        {
            foreach (Connector connector in sourceConnectors)
                if (connector.UID.Equals(uid))
                    return connector;

            return null;
        }

        public Connector GetTargetConnector(string uid)
        {
            foreach (Connector connector in targetConnectors)
                if (connector.UID.Equals(uid))
                    return connector;

            return null;
        }

        // retrieves the path to the element
        public string GetPath()
        {
            string path = name;

            Element elem = parentElement;
            Package pack = parentPackage;


            while (elem != null)
            {
                pack = elem.ParentPackage;
                path = elem.Name + "." + path;
                elem = elem.ParentElement;
            }

            while (pack != null)
            {
                path = pack.Name + "." + path;
                pack = pack.ParentPackage;
            }

            return path;
        }

        #endregion

        #region Calculation

        // reseting calculation elements for the next calculation
        public void ResetCalculation()
        {
            numOfChanges = 0;
            changes.Clear();
            modifiedElements.Clear();
            removedElements.Clear();
            addedElements.Clear();
            modifiedAttributes.Clear();
            removedAttributes.Clear();
            addedAttributes.Clear();
            modifiedConnectors.Clear();
            removedConnectors.Clear();
            addedConnectors.Clear();

            foreach (Attribute attr in attributes)
                attr.ResetCalculation();

            foreach (Connector conn in sourceConnectors)
                conn.ResetCalculation();

            foreach (Connector conn in targetConnectors)
                conn.ResetCalculation();

            foreach (Element child in children)
                child.ResetCalculation();
        }

        // checks whether the element shall be ignored if relevance policy is enabled
        public bool IgnoreElement()
        {
            foreach (string str in ConfigReader.ExcludedElementTypes)
                if (type.Equals(str))
                    return true;

            return false;
        }

        // compares two elements in two releases
        public int CompareElements(Element oldElement, bool RelevantOnly)
        {
            if (RelevantOnly && IgnoreElement())
                return 0;

            int numOfChanges = 0;

            if (!type.Equals(oldElement.Type))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Type: " + oldElement.Type + " -> " + type, false));
            }

            if (!name.Equals(oldElement.Name))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Name: " + oldElement.Name + " -> " + name, false));
            }

            if (((RelevantOnly && !ConfigReader.ExcludedAttributeNote) || !RelevantOnly) && !Equals(note, oldElement.Note))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Note", false));
            }

            // checking if the attribute is changed or added in the new model
            foreach (Attribute attribute in attributes)
            {
                if (RelevantOnly && attribute.IgnoreAttribute())
                    continue;

                Attribute oldAttribute = oldElement.GetAttribute(attribute.Name);

                int num = 0;

                // checking if the attribute is added to the new model
                if (oldAttribute == null)
                {
                    numOfChanges += attribute.NumOfAllModifiableElements(RelevantOnly);
                    addedAttributes.Add(attribute);
                    //changes.Add(new MMChange("+ Attribute " + attribute.GetPath(), false));
                }
                // checking if the attribute is changed in the new model
                else if ((num = attribute.CompareAttributes(oldAttribute, RelevantOnly)) != 0)
                {
                    numOfChanges += num;
                    modifiedAttributes.Add(attribute);
                }
            }

            // checking if the attribute is removed in the new model
            foreach (Attribute oldAttribute in oldElement.Attributes)
            {
                if (RelevantOnly && oldAttribute.IgnoreAttribute())
                    continue;

                Attribute attribute = GetAttribute(oldAttribute.Name);

                if (attribute == null)
                {
                    numOfChanges += oldAttribute.NumOfAllModifiableElements(RelevantOnly);
                    removedAttributes.Add(oldAttribute);
                    //changes.Add(new MMChange("- Attribute " + oldAttribute.GetPath(), false).AppendTabs(1));
                }
            }

            // checking if the source connector is changed or added in the new model
            foreach (Connector connector in SourceConnectors)
            {
                if (RelevantOnly && connector.IgnoreConector())
                    continue;

                Connector oldConnector = oldElement.GetSourceConnector(connector.UID);

                int num = 0;

                // checking if the connector is added to the new model
                if (oldConnector == null)
                {
                    numOfChanges += connector.NumOfAllModifiableElements(RelevantOnly);
                    addedConnectors.Add(connector);
                }

                // checking if the connector is changed in the new model
                else if ((num = connector.CompareConnectors(oldConnector, RelevantOnly)) != 0)
                {
                    numOfChanges += num;
                    modifiedConnectors.Add(connector);
                }
            }

            // checking if the source connector is removed in the new model
            foreach (Connector oldConnector in oldElement.SourceConnectors)
            {
                if (RelevantOnly && oldConnector.IgnoreConector())
                    continue;

                Connector connector = GetSourceConnector(oldConnector.UID);

                if (connector == null)
                {
                    numOfChanges += oldConnector.NumOfAllModifiableElements(RelevantOnly);
                    removedConnectors.Add(oldConnector);
                }
            }

            // checking if the target connector is changed or added in the new model
            foreach (Connector connector in TargetConnectors)
            {
                if (RelevantOnly && connector.IgnoreConector())
                    continue;

                Connector oldConnector = oldElement.GetTargetConnector(connector.UID);

                int num = 0;

                // checking if the connector is added to the new model
                if (oldConnector == null)
                {
                    numOfChanges += connector.NumOfAllModifiableElements(RelevantOnly);
                    addedConnectors.Add(connector);
                }

                // checking if the connector is changed in the new model
                else if ((num = connector.CompareConnectors(oldConnector, RelevantOnly)) != 0)
                {
                    numOfChanges += num;
                    modifiedConnectors.Add(connector);
                }
            }

            // checking if the target connector is removed in the new model
            foreach (Connector oldConnector in oldElement.TargetConnectors)
            {
                if (RelevantOnly && oldConnector.IgnoreConector())
                    continue;

                Connector connector = GetTargetConnector(oldConnector.UID);

                if (connector == null)
                {
                    numOfChanges += oldConnector.NumOfAllModifiableElements(RelevantOnly);
                    removedConnectors.Add(oldConnector);
                }
            }

            // checking if the element is changed or added in the new model
            foreach (Element child in children)
            {
                Element oldSubElement = oldElement.GetChild(child.Name);
                int num = 0;
                // checking if the element is added to the new model
                if (oldSubElement == null)
                {
                    numOfChanges += child.NumOfAllModifiableElements(RelevantOnly);
                    addedElements.Add(child);
                }

                // checking if the element is changed in the new model
                else if((num = child.CompareElements(oldSubElement, RelevantOnly)) != 0)
                {
                   numOfChanges += num;
                   modifiedElements.Add(oldSubElement);
                    
                }
            }

            // checking if the element is removed to the new model
            foreach (Element elem in oldElement.children)
            {
                Element element = GetChild(elem.Name);

                if (elem == null)
                {
                    numOfChanges += oldElement.NumOfAllModifiableElements(RelevantOnly);
                    removedElements.Add(oldElement);
                }
            }

            return numOfChanges;
        }

        #endregion

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

        public List<Element> Children
        {
            get { return children; }
            set { children = value; }
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

        public String Note
        {
            get { return note; }
            set { note = value; }
        }

        public List<Attribute> Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        public List<Connector> SourceConnectors
        {
            get { return sourceConnectors; }
            set { sourceConnectors = value; }
        }

        public List<Connector> TargetConnectors
        {
            get { return targetConnectors; }
            set { targetConnectors = value; }
        }

        public List<MMChange> Changes
        {
            get { return changes; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
        }

        public List<Element> ModifiedElements
        {
            get { return modifiedElements; }
        }

        public List<Element> RemovedElements
        {
            get { return removedElements; }
        }

        public List<Element> AddedElements
        {
            get { return addedElements; }
        }

        public List<Attribute> ModifiedAttributes
        {
            get { return modifiedAttributes; }
        }

        public List<Attribute> RemovedAttributes
        {
            get { return removedAttributes; }
        }

        public List<Attribute> AddedAttributes
        {
            get { return addedAttributes; }
        }

        public List<Connector> ModifiedConnectors
        {
            get { return modifiedConnectors; }
        }

        public List<Connector> RemovedConnectors
        {
            get { return removedConnectors; }
        }

        public List<Connector> AddedConnectors
        {
            get { return addedConnectors; }
        }

        #endregion

    }
}
