using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using ModelicaChangeAnalyzer.Config;
using ModelicaChangeAnalyzer.Changes;

namespace ModelicaChangeAnalyzer.Datamodel
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
        private List<Connector> sourceConnectors = new List<Connector>();
        private List<Connector> targetConnectors = new List<Connector>();
        private List<Attribute> attributes = new List<Attribute>();

        // Changes
        private int numOfChanges;
        private List<Change> changes = new List<Change>();
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

        /*public void AddChild(Element element)
        {
            children.Add(element);
        }*/

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

        public int NumberOfAttributes(bool relevantOnly)
        {
            if (relevantOnly && IgnoreElement())
                return 0;

            int numberOfAttributes = 0;

            foreach (Attribute attr in attributes)
                if (!(relevantOnly && attr.IgnoreAttribute()))
                    numberOfAttributes++;

            return numberOfAttributes;
        }

        public int NumberOfConnectors(bool relevantOnly)
        {
            if (relevantOnly && IgnoreElement())
                return 0;

            return sourceConnectors.Count + targetConnectors.Count;
        }

        public int NumOfAllModifiableElements(bool RelevantOnly)
        {
            int modifiableElems = 1;

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

        public int NumberOfModifiedConnectors()
        {
            return modifiedConnectors.Count;
        }

        public int NumberOfAddedConnectors()
        {
            return addedConnectors.Count;
        }

        public int NumberOfRemovedConnectors()
        {
            return removedConnectors.Count;
        }

        public int NumberOfModifiedAttributes()
        {
            return modifiedAttributes.Count;
        }

        public int NumberOfAddedAttributes()
        {
            return addedAttributes.Count;
        }

        public int NumberOfRemovedAttributes()
        {
            return removedAttributes.Count;
        }

        #endregion

        #region Retrieve object

        // retrieves all changes
        public List<Change> GetChanges()
        {
            List<Change> listOfChanges = new List<Change>(changes);

            foreach (Attribute attr in attributes)
            {
                foreach (Change chng in attr.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(1));
            }

            foreach (Connector conn in sourceConnectors)
            {
                foreach (Change chng in conn.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(1));
            }

            foreach (Connector conn in targetConnectors)
            {
                foreach (Change chng in conn.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(1));
            }

            return listOfChanges;
        }

        public void GetAllConnectors(List<Connector> list)
        {
            foreach (Connector conn in sourceConnectors)
                list.Add(conn);

            foreach (Connector conn in targetConnectors)
                list.Add(conn);
        }

        public void GetAllModifiedConnectors(List<Connector> list)
        {
            foreach (Connector conn in modifiedConnectors)
                list.Add(conn);
        }

        public void GetAllRemovedConnectors(List<Connector> list)
        {
            foreach (Connector conn in removedConnectors)
                list.Add(conn);
        }

        public void GetAllAddedConnectors(List<Connector> list)
        {
            foreach (Connector conn in addedConnectors)
                list.Add(conn);
        }

        public List<Attribute> GetAllAttributes(List<Attribute> list)
        {
            foreach (Attribute attr in attributes)
                list.Add(attr);

            return list;
        }

        public List<Attribute> GetAllModifiedAttributes(List<Attribute> list)
        {
            foreach (Attribute attr in modifiedAttributes)
                list.Add(attr);

            return list;
        }

        public List<Attribute> GetAllAddedAttributes(List<Attribute> list)
        {
            foreach (Attribute attr in addedAttributes)
                list.Add(attr);

            return list;
        }

        public List<Attribute> GetAllRemovedAttributes(List<Attribute> list)
        {
            foreach (Attribute attr in removedAttributes)
                list.Add(attr);

            return list;
        }

        public Attribute GetAttribute(string name)
        {
            foreach (Attribute attribute in attributes)
                if (attribute.Name.Equals(name))
                    return attribute;

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

        public void ResetCalculation()
        {
            numOfChanges = 0;
            changes.Clear();
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
        }

        public bool IgnoreElement()
        {
            foreach (string str in ConfigReader.ExcludedElementTypes)
                if (type.Equals(str))
                    return true;

            return false;
        }

        public int CompareElements(Element oldElement, bool RelevantOnly)
        {
            if (RelevantOnly && IgnoreElement())
                return 0;

            if (!name.Equals(oldElement.Name))
            {
                numOfChanges++;
                changes.Add(new Change("~ Name: " + oldElement.Name + " -> " + name, false).AppendTabs(1));
            }

            if (((RelevantOnly && !ConfigReader.ExcludedElementNote) || !RelevantOnly) && !Equals(note, oldElement.Note))
            {
                numOfChanges++;
                changes.Add(new Change("~ Note", false).AppendTabs(1));
            }

            /*if (numOfChanges > 0 && parentPackage != null)
                parentPackage.ModifiedElements.Add(this);*/

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
                    changes.Add(new Change("+ Attribute " + attribute.GetPath(), false).AppendTabs(1));
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
                    changes.Add(new Change("- Attribute " + oldAttribute.GetPath(), false).AppendTabs(1));
                }
            }

            // checking if the connector is changed or added in the new model
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
                    changes.Add(new Change("+ Connector (source) " + connector.GetPath(), false).AppendTabs(1));
                }

                // checking if the connector is changed in the new model
                else if ((num = connector.CompareConnectors(oldConnector, RelevantOnly, false)) != 0)
                {
                    numOfChanges += num;
                    modifiedConnectors.Add(connector);
                }
            }

            // checking if the connector is removed in the new model
            foreach (Connector oldConnector in oldElement.SourceConnectors)
            {
                if (RelevantOnly && oldConnector.IgnoreConector())
                    continue;

                Connector connector = GetSourceConnector(oldConnector.UID);

                if (connector == null)
                {
                    numOfChanges += oldConnector.NumOfAllModifiableElements(RelevantOnly);
                    removedConnectors.Add(oldConnector);
                    changes.Add(new Change("- Connector (source) " + oldConnector.GetPath(), false).AppendTabs(1));
                }
            }

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
                    changes.Add(new Change("+ Connector (target) " + connector.GetPath(), false).AppendTabs(1));
                }

                // checking if the connector is changed in the new model
                else if ((num = connector.CompareConnectors(oldConnector, RelevantOnly, true)) != 0)
                {
                    numOfChanges += num;
                    modifiedConnectors.Add(connector);
                }
            }

            // checking if the connector is removed in the new model
            foreach (Connector oldConnector in oldElement.TargetConnectors)
            {
                if (RelevantOnly && oldConnector.IgnoreConector())
                    continue;

                Connector connector = GetTargetConnector(oldConnector.UID);

                if (connector == null)
                {
                    numOfChanges += oldConnector.NumOfAllModifiableElements(RelevantOnly);
                    removedConnectors.Add(oldConnector);
                    changes.Add(new Change("- Connector (target) " + oldConnector.GetPath(), false).AppendTabs(1));
                }
            }

            if (numOfChanges > 0)
                changes.Insert(0, new Change("~ Element " + GetPath(), false));

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

        public List<Change> Changes
        {
            get { return changes; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
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
