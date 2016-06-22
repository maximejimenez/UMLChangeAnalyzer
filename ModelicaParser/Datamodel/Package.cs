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
    public class Package
    {
        // Backtracking
        //public MetaModel metamodel;
        private Package parentPackage = null;

        // Attributes
        private String name = "";
        private String note = "";
        private List<Element> elements =new List<Element>();
        private List<Package> subPackages = new List<Package>();


        // Changes
        private int numOfChanges;
        private List<MMChange> changes = new List<MMChange>();
        private List<Package> modifiedSubPackages = new List<Package>();
        private List<Package> removedSubPackages = new List<Package>();
        private List<Package> addedSubPackages = new List<Package>();
        private List<Element> modifiedElements = new List<Element>();
        private List<Element> removedElements = new List<Element>();
        private List<Element> addedElements = new List<Element>();

        #region Loading

        public Package()
        {

        }

        public Package(string name)
        {
            this.name = name;
        }

        public Package(string name, string note)
        {
            this.name = name;
            this.note = note;
        }

        public void AddSubPackage(Package package)
        {
            subPackages.Add(package);
        }

        public void AddElement(Element uniontype)
        {
            elements.Add(uniontype);
        }

        public override string ToString()
        {
            string ret = "Package " + name + "\n";

            foreach (Element elem in elements)
                ret += elem.ToString();

            foreach (Package pack in subPackages)
                ret += pack.ToString();

            return ret;
        }
        #endregion

        #region Calculate number of

        public int NumberOfElements(bool relevantOnly)
        {
            if (relevantOnly && IgnorePackage())
                return 0;

            int numOfElements = 0;

            foreach (Element elem in elements)
                if (!(relevantOnly && elem.IgnoreElement()))
                    numOfElements += elem.NumberOfElements();

            foreach (Package subPackage in subPackages)
                numOfElements += subPackage.NumberOfElements(relevantOnly);

            return numOfElements;
        }

        public int NumberOfAttributes(bool relevantOnly)
        {
            if (relevantOnly && IgnorePackage())
                return 0;

            int numberOfAttributes = 0;

            foreach (Element elem in elements)
                if (!(relevantOnly && elem.IgnoreElement()))
                    numberOfAttributes += elem.NumberOfAttributes(relevantOnly);

            foreach (Package pack in subPackages)
                numberOfAttributes += pack.NumberOfAttributes(relevantOnly);

            return numberOfAttributes;
        }

        public int NumberOfConnectors(bool relevantOnly)
        {
            if (relevantOnly && IgnorePackage())
                return 0;

            int numberOfConnectors = 0;

            foreach (Element elem in elements)
                if (!(relevantOnly && elem.IgnoreElement()))
                    numberOfConnectors += elem.NumberOfConnectors(relevantOnly);

            foreach (Package pack in subPackages)
                numberOfConnectors += pack.NumberOfConnectors(relevantOnly);

            return numberOfConnectors;
        }

        // calculates the number of modified elements of this package (from the list of changed elements) and all of its sub-packages
        public int NumberOfModifiedElements()
        {
            int numberOfChangedElements = modifiedElements.Count;

            foreach (Element elem in modifiedElements)
            {
                numberOfChangedElements += elem.NumberOfModifiedElements();
            }

            foreach (Package subPackage in modifiedSubPackages)
                numberOfChangedElements += subPackage.NumberOfModifiedElements();

            return numberOfChangedElements;
        }

        // calculates the number of added elements of this package and all of its sub-packages
        public int NumberOfAddedElements()
        {
            int numberOfAddedElements = 0;

            foreach(Element elem in modifiedElements){
                numberOfAddedElements += elem.NumberOfAddedElements();
            }

            foreach(Element elem in addedElements){
                numberOfAddedElements += elem.NumberOfElements();
            }

            foreach (Package subPack in addedSubPackages)
                numberOfAddedElements += subPack.NumberOfElements(false); // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package subPackage in modifiedSubPackages)
                numberOfAddedElements += subPackage.NumberOfAddedElements();

            return numberOfAddedElements;
        }

        // calculates the number of removed elements of this package and all of its sub-packages
        public int NumberOfRemovedElements()
        {
            int numberOfRemovedElements = 0;

            foreach (Element elem in modifiedElements)
            {
                numberOfRemovedElements += elem.NumberOfRemovedElements();
            }

            foreach (Element elem in removedElements)
            {
                numberOfRemovedElements += elem.NumberOfElements();
            }

            foreach (Package subPack in removedSubPackages)
                numberOfRemovedElements += subPack.NumberOfElements(false); // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package subPackage in modifiedSubPackages)
                numberOfRemovedElements += subPackage.NumberOfRemovedElements();

            return numberOfRemovedElements;
        }

        // calculates the total number of sub-packages in the packages and all of its sub-packages
        public int NumberOfPackages(bool relevantOnly)
        {
            if (relevantOnly && IgnorePackage())
                return 0;

            int numOfPackages = 1;

            foreach (Package pack in subPackages)
                if (!(relevantOnly && pack.IgnorePackage()))
                    numOfPackages += pack.NumberOfPackages(relevantOnly);

            return numOfPackages;
        }

        // calculates the number of modified sub-packages of this package and all of its sub-packages
        public int NumberOfModifiedSubPackages()
        {
            int numberOfModifiedSubPackages = 0;

            foreach (Package subPackage in modifiedSubPackages)
            {
                numberOfModifiedSubPackages++;
                numberOfModifiedSubPackages += subPackage.NumberOfModifiedSubPackages();
            }

            return numberOfModifiedSubPackages;
        }

        // calculates the number of added sub-packages of this package and all of its sub-packages
        public int NumberOfAddedSubPackages()
        {
            int numberOfAddedSubPackages = 0;

            foreach (Package subPackage in addedSubPackages)
            {
                numberOfAddedSubPackages += subPackage.NumberOfPackages(false); // false because we already excluded non relevant packages/elements, etc. during compare
            }

            foreach (Package subPackage in modifiedSubPackages)
                numberOfAddedSubPackages += subPackage.NumberOfAddedSubPackages();

            return numberOfAddedSubPackages;
        }

        // calculates the number of removed sub-packages of this package and all of its sub-packages
        public int NumberOfRemovedSubPackages()
        {
            int numberOfRemovedSubPackages = 0;

            foreach (Package subPackage in removedSubPackages)
            {
                numberOfRemovedSubPackages += subPackage.NumberOfPackages(false); // false because we already excluded non relevant packages/elements, etc. during compare
            }

            foreach (Package subPackage in modifiedSubPackages)
                numberOfRemovedSubPackages += subPackage.NumberOfRemovedSubPackages();

            return numberOfRemovedSubPackages;
        }

        public int NumberOfModifiedConnectors()
        {
            int numberOfModifiedConnectors = 0;

            foreach (Element elem in modifiedElements)
                numberOfModifiedConnectors += elem.NumberOfModifiedConnectors();

            foreach (Package subPackage in modifiedSubPackages)
                numberOfModifiedConnectors += subPackage.NumberOfModifiedConnectors();

            return numberOfModifiedConnectors;
        }

        public int NumberOfAddedConnectors()
        {
            int numberOfAddedConnectors = 0;

            foreach (Element elem in modifiedElements)
                numberOfAddedConnectors += elem.NumberOfAddedConnectors();

            foreach (Element elem in addedElements)
                numberOfAddedConnectors += elem.NumberOfConnectors(false);

            foreach (Package subPackage in modifiedSubPackages)
                numberOfAddedConnectors += subPackage.NumberOfAddedConnectors();

            foreach (Package package in addedSubPackages)
                numberOfAddedConnectors += package.NumberOfConnectors(false);

            return numberOfAddedConnectors;
        }

        public int NumberOfRemovedConnectors()
        {
            int numberOfRemovedConnectors = 0;

            foreach (Element elem in modifiedElements)
                numberOfRemovedConnectors += elem.NumberOfRemovedConnectors();

            foreach (Element elem in removedElements)
                numberOfRemovedConnectors += elem.NumberOfConnectors(false);

            foreach (Package subPackage in modifiedSubPackages)
                numberOfRemovedConnectors += subPackage.NumberOfRemovedConnectors();

            foreach (Package package in removedSubPackages)
                numberOfRemovedConnectors += package.NumberOfConnectors(false);    // false because we already excluded non relevant packages/elements, etc. during compare

            return numberOfRemovedConnectors;
        }

        // calculates the number of modified attributes of this package
        public int NumberOfModifiedAttributes()
        {
            int numberOfModifiedAttributes = 0;

            foreach (Element elem in modifiedElements)
                numberOfModifiedAttributes += elem.NumberOfModifiedAttributes();

            foreach (Package subPackage in modifiedSubPackages)
                numberOfModifiedAttributes += subPackage.NumberOfModifiedAttributes();

            return numberOfModifiedAttributes;
        }

        // calculates the number of added attributes of this package
        public int NumberOfAddedAttributes()
        {
            int numberOfAddedAttributes = 0;

            foreach (Element elem in modifiedElements)
            {
                numberOfAddedAttributes += elem.NumberOfAddedAttributes();
            }

            foreach (Element elem in addedElements)
            {
                numberOfAddedAttributes += elem.NumberOfAttributes(false);
            }

            foreach (Package subPackage in modifiedSubPackages)
                numberOfAddedAttributes += subPackage.NumberOfAddedAttributes();

            foreach (Package package in addedSubPackages)
                numberOfAddedAttributes += package.NumberOfAttributes(false);    // false because we already excluded non relevant packages/elements, etc. during compare

            return numberOfAddedAttributes;
        }

        // calculates the number of removed attributes of this package
        public int NumberOfRemovedAttributes()
        {
            int numberOfRemovedAttributes = 0;

            foreach (Element elem in modifiedElements)
                numberOfRemovedAttributes += elem.NumberOfRemovedAttributes();

            foreach (Element elem in removedElements)
                numberOfRemovedAttributes += elem.NumberOfAttributes(false);

            foreach (Package subPackage in modifiedSubPackages)
                numberOfRemovedAttributes += subPackage.NumberOfRemovedAttributes();

            foreach (Package package in removedSubPackages)
                numberOfRemovedAttributes += package.NumberOfAttributes(false);    // false because we already excluded non relevant packages/elements, etc. during compare

            return numberOfRemovedAttributes;
        }

        // calculates the number of elements which can be modified (used to calculate changes when adding/removing elements)
        public int NumOfAllModifiableElements(bool RelevantOnly)
        {
            int modifiableElems = 0;    // not 1 as we do not count introduction/removal of packages as changes

            foreach (Element elem in elements)
            {
                if (RelevantOnly && elem.IgnoreElement())
                    continue;

                modifiableElems += elem.NumOfAllModifiableElements(RelevantOnly);
            }

            foreach (Package subPack in subPackages)
            {
                if (RelevantOnly && subPack.IgnorePackage())
                    continue;

                modifiableElems += subPack.NumOfAllModifiableElements(RelevantOnly);
            }

            return modifiableElems;
        }

        #endregion

        #region Retrieve objects


        public void printAsAddedPackage(List<MMChange> listOfChanges, int indent)
        {
            listOfChanges.Add(new MMChange("+ Package: " + GetPath(), true).AppendTabs(indent++));
            foreach (Element elem in Elements)
            {
                elem.printAsAddedElement(listOfChanges, indent);
            }
        }

        public void printAsModifiedPackage(List<MMChange> listOfChanges, int indent)
        {
            listOfChanges.Add(new MMChange("~ Package: " + GetPath(), true).AppendTabs(indent++));

            foreach (MMChange chng in changes)
                listOfChanges.Add(chng.AppendTabs(indent));

            foreach (Element elem in addedElements)
            {
                elem.printAsAddedElement(listOfChanges, indent);
            }

            foreach (Element elem in modifiedElements)
            {
                elem.printAsModifiedElement(listOfChanges, indent);
            }

            foreach (Element elem in removedElements)
            {
                elem.printAsRemovedElement(listOfChanges, indent);
            }
        }

        public void printAsRemovedPackage(List<MMChange> listOfChanges, int indent)
        {
            listOfChanges.Add(new MMChange("- Package: " + GetPath(), true).AppendTabs(indent++));
            foreach (Element elem in Elements)
            {
                elem.printAsRemovedElement(listOfChanges, indent);
            }
        }


        // retrieves all changes in the package and all of its sub-packages
        public List<MMChange> GetChanges()
        {
            List<MMChange> listOfChanges = new List<MMChange>(changes);

            foreach (Element elem in elements)
            {
                foreach (MMChange chng in elem.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(1));
            }

            foreach (Package subPack in subPackages)
                foreach (MMChange chng in subPack.GetChanges())
                    listOfChanges.Add(chng.AppendTabs(1));

            return listOfChanges;
        }

        // adds all elements in the package and sub-packages to a list
        public List<Element> GetAllElements(List<Element> list)
        {
            foreach (Element elem in Elements)
            {
                list.Add(elem);
                elem.GetAllElements(list);
            }

            foreach (Package subPack in subPackages)
                subPack.GetAllElements(list);

            return list;
        }

        // adds all modified elements in the package and sub-packages  to a list
        public List<Element> GetAllModifiedElements(List<Element> list)
        {
            foreach (Element elem in modifiedElements) {
                list.Add(elem);
                elem.GetAllModifiedElements(list);
            }

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllModifiedElements(list);

            return list;
        }

        // adds all added elements in the package and sub-packages  to a list
        public List<Element> GetAllAddedElements(List<Element> list)
        {
            foreach (Element elem in addedElements)
            {
                list.Add(elem);
                elem.GetAllElements(list);
            }

            foreach (Element elem in modifiedElements)
                elem.GetAllAddedElements(list);

            foreach (Package subPack in addedSubPackages)
                subPack.GetAllElements(list);

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllAddedElements(list);

            return list;
        }

        // adds all removed elements in the package and sub-packages  to a list
        public List<Element> GetAllRemovedElements(List<Element> list)
        {
            foreach (Element elem in removedElements)
            {
                list.Add(elem);
                elem.GetAllElements(list);
            }

            foreach (Element elem in modifiedElements)
                elem.GetAllRemovedElements(list);

            foreach (Package subPack in removedSubPackages)
                subPack.GetAllElements(list);

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllRemovedElements(list);

            return list;
        }

        public void GetAllConnectors(List<Connector> list)
        {
            foreach (Element elem in Elements)
                elem.GetAllConnectors(list);

            foreach (Package subPack in subPackages)
                subPack.GetAllConnectors(list);
        }

        public void GetAllModifiedConnectors(List<Connector> list)
        {
            foreach (Element elem in modifiedElements)
                elem.GetAllModifiedConnectors(list);

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllModifiedConnectors(list);
        }

        public void GetAllAddedConnectors(List<Connector> list)
        {
            foreach (Element elem in addedElements)
                elem.GetAllConnectors(list);

            foreach (Element elem in modifiedElements)
                elem.GetAllAddedConnectors(list);

            foreach (Package subPack in addedSubPackages)
                subPack.GetAllConnectors(list);

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllAddedConnectors(list);
        }

        public void GetAllRemovedConnectors(List<Connector> list)
        {
            foreach (Element elem in removedElements)
                elem.GetAllConnectors(list);

            foreach (Element elem in modifiedElements)
                elem.GetAllRemovedConnectors(list);

            foreach (Package subPack in removedSubPackages)
                subPack.GetAllConnectors(list);

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllRemovedConnectors(list);
        }

        // adds all attributes in the package and sub-packages  to a list
        public List<Attribute> GetAllAttributes(List<Attribute> list)
        {
            foreach (Element elem in Elements)
                elem.GetAllAttributes(list);

            foreach (Package subPack in subPackages)
                subPack.GetAllAttributes(list);

            return list;
        }

        // adds all modified attributes in the package and sub-packages to a list
        public List<Attribute> GetAllModifiedAttributes(List<Attribute> list)
        {
            foreach (Element elem in modifiedElements)
                elem.GetAllModifiedAttributes(list);

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllModifiedAttributes(list);

            return list;
        }

        // adds all added attributes in the package and sub-packages to a list
        public List<Attribute> GetAllAddedAttributes(List<Attribute> list)
        {
            foreach (Element elem in addedElements)
                elem.GetAllAttributes(list);

            foreach (Element elem in modifiedElements)
                elem.GetAllAddedAttributes(list);

            foreach (Package subPack in addedSubPackages)
                subPack.GetAllAttributes(list);

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllAddedAttributes(list);

            return list;
        }

        // adds all removed attributes in the package and sub-packages to a list
        public List<Attribute> GetAllRemovedAttributes(List<Attribute> list)
        {
            foreach (Element elem in removedElements)
                elem.GetAllAttributes(list);

            foreach (Element elem in modifiedElements)
                elem.GetAllRemovedAttributes(list);

            foreach (Package subPack in removedSubPackages)
                subPack.GetAllAttributes(list);

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllRemovedAttributes(list);

            return list;
        }

        // adds all sub-packages in the package and sub-packages to a list
        public List<Package> GetAllSubPackages(List<Package> list)
        {
            foreach (Package subPack in subPackages)
            {
                list.Add(subPack);
                subPack.GetAllSubPackages(list);
            }

            return list;
        }

        // adds all modified sub-packages in the package and sub-packages to a list
        public List<Package> GetAllModifiedSubPackages(List<Package> list)
        {
            foreach (Package subPack in modifiedSubPackages)
            {
                list.Add(subPack);
                subPack.GetAllModifiedSubPackages(list);
            }

            return list;
        }

        // adds all added sub-packages in the package and sub-packages to a list
        public List<Package> GetAllAddedSubPackages(List<Package> list)
        {
            foreach (Package subPack in addedSubPackages)
            {
                list.Add(subPack);
                subPack.GetAllSubPackages(list);
            }

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllAddedSubPackages(list);

            return list;
        }

        // adds all removed sub-packages in the package and sub-packages to a list
        public List<Package> GetAllRemovedSubPackages(List<Package> list)
        {
            foreach (Package subPack in removedSubPackages)
            {
                list.Add(subPack);
                subPack.GetAllSubPackages(list);
            }

            foreach (Package subPack in modifiedSubPackages)
                subPack.GetAllRemovedSubPackages(list);

            return list;
        }

        // retrieves the sub-package by its name - used for comparing packages from the main menu
        public Package GetSubPackageByName(string name)
        {
            foreach (Package subPackage in subPackages)
                if (subPackage.Name.Equals(name))
                    return subPackage;

            return null;
        }

        // retrieves the element by its name - used for comparing elements from the main menu
        public Element GetElementByName(string name)
        {
            foreach (Element element in elements)
                if (element.Name.Equals(name))
                    return element;

            return null;
        }

        // retrieves the path to the package
        public string GetPath()
        {
            string path = name;

            Package pack = this;

            while (pack.parentPackage != null)
            {
                path = pack.parentPackage.name + "." + path;
                pack = pack.parentPackage;
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
            modifiedSubPackages.Clear();
            removedSubPackages.Clear();
            addedSubPackages.Clear();
            modifiedElements.Clear();
            removedElements.Clear();
            addedElements.Clear();

            foreach (Package subPack in subPackages)
                subPack.ResetCalculation();

            foreach (Element elem in elements)
                elem.ResetCalculation();
        }

        // checks whether the package shall be ignored if relevance policy is enabled
        public bool IgnorePackage()
        {
            foreach (string str in ConfigReader.ExcludedPackageNames)
                if (name.Equals(str))
                    return true;

            return false;
        }

        // compares two packages in two releases
        public int ComparePackages(Package oldPackage, bool RelevantOnly)
        {
            if (RelevantOnly && IgnorePackage())
                return 0;

            if (!name.Equals(oldPackage.Name))  // number of changes not increased as the change in the name of a package is not considered a change
                changes.Add(new MMChange("~ Name: " + oldPackage.Name + " -> " + name, true));

            if (((RelevantOnly && !ConfigReader.ExcludedAttributeNote) || !RelevantOnly) && !Equals(note, oldPackage.Note))
            {
                numOfChanges++;
                changes.Add(new MMChange("~ Note", false));
            }

            // checking if the element is changed or added in the new model
            foreach (Element element in elements)
            {
                if (RelevantOnly && element.IgnoreElement())
                    continue;

                Element oldElement = oldPackage.GetElementByName(element.Name);

                int num = 0;

                // chacking if the element is added to the new model
                if (oldElement == null)
                {
                    numOfChanges += element.NumOfAllModifiableElements(RelevantOnly);
                    addedElements.Add(element);
                    //changes.Add(new MMChange("+ Element " + element.GetPath() + " " + element.Name, false));
                }

                // chacking if the element is changed in the new model
                else if ((num = element.CompareElements(oldElement, RelevantOnly)) != 0)
                {
                    numOfChanges += num;
                    modifiedElements.Add(element);
                }
            }

            // checking if the element is removed in the new model
            foreach (Element oldElement in oldPackage.Elements)
            {
                if (RelevantOnly && oldElement.IgnoreElement())
                    continue;

                Element element = GetElementByName(oldElement.Name);

                if (element == null)
                {
                    numOfChanges += oldElement.NumOfAllModifiableElements(RelevantOnly);
                    removedElements.Add(oldElement);
                }
            }

            return numOfChanges;
        }

        #endregion

        #region Getters and setters

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public String Note
        {
            get { return note; }
            set { note = value; }
        }

        public List<Element> Elements
        {
            get { return elements; }
        }

        public List<Package> SubPackages
        {
            get { return subPackages; }
        }

        public Package ParentPackage
        {
            get { return parentPackage; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
        }

        #endregion
    }
}
