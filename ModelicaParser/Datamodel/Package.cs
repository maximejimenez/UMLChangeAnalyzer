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
                    numOfElements++;

            foreach (Package subPackage in subPackages)
                numOfElements += subPackage.NumberOfElements(relevantOnly);

            return numOfElements;
        }

        // calculates the number of attributes in the package
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

            foreach (Package subPackage in modifiedSubPackages)
                numberOfChangedElements += subPackage.NumberOfModifiedElements();

            return numberOfChangedElements;
        }

        // calculates the number of added elements of this package and all of its sub-packages
        public int NumberOfAddedElements()
        {
            int numberOfAddedElements = addedElements.Count;

            foreach (Package subPack in addedSubPackages)
                numberOfAddedElements += subPack.NumberOfElements(false); // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package subPackage in modifiedSubPackages)
                numberOfAddedElements += subPackage.NumberOfAddedElements();

            return numberOfAddedElements;
        }

        // calculates the number of removed elements of this package and all of its sub-packages
        public int NumberOfRemovedElements()
        {
            int numberOfRemovedElements = removedElements.Count;

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

            int numOfPackages = 0;

            foreach (Package pack in subPackages)
                if (!(relevantOnly && pack.IgnorePackage()))
                    numOfPackages++;

            foreach (Package subPack in subPackages)
                numOfPackages += subPack.NumberOfPackages(relevantOnly);

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
                numberOfAddedSubPackages++;
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
                numberOfRemovedSubPackages++;
                numberOfRemovedSubPackages += subPackage.NumberOfPackages(false); // false because we already excluded non relevant packages/elements, etc. during compare
            }

            foreach (Package subPackage in modifiedSubPackages)
                numberOfRemovedSubPackages += subPackage.NumberOfRemovedSubPackages();

            return numberOfRemovedSubPackages;
        }

        // calculates the number of modified attributes of this package
        public int NumberOfModifiedAttributes()
        {
            int numberOfModifiedAttributes = 0;

            foreach (Element elem in modifiedElements)
                numberOfModifiedAttributes += elem.ModifiedAttributes.Count;

            foreach (Package subPackage in modifiedSubPackages)
                numberOfModifiedAttributes += subPackage.NumberOfModifiedAttributes();

            return numberOfModifiedAttributes;
        }

        // calculates the number of added attributes of this package
        public int NumberOfAddedAttributes()
        {
            int numberOfAddedAttributes = 0;

            foreach (Element elem in modifiedElements)
                numberOfAddedAttributes += elem.AddedAttributes.Count;

            foreach (Element elem in addedElements)
                numberOfAddedAttributes += elem.Attributes.Count;

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
                numberOfRemovedAttributes += elem.RemovedAttributes.Count;

            foreach (Element elem in removedElements)
                numberOfRemovedAttributes += elem.Attributes.Count;

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

        // retrieves all changes in the package and all of its sub-packages
        public List<MMChange> GetChanges()
        {
            List<MMChange> listOfChanges = new List<MMChange>(changes);

            foreach (Element elem in elements)
            {
                if (elem.NumOfChanges != 0)
                    listOfChanges.Add(new MMChange("~ Element " + elem.GetPath(), true));

                foreach (MMChange chng in elem.GetChanges())
                    listOfChanges.Add(chng);
            }

            foreach (Package subPack in subPackages)
                foreach (MMChange chng in subPack.GetChanges())
                    listOfChanges.Add(chng);

            return listOfChanges;
        }

        // adds all elements in the package and sub-packages to a list
        public List<Element> GetAllElements(List<Element> list)
        {
            foreach (Element elem in Elements)
                list.Add(elem);

            foreach (Package subPack in subPackages)
                subPack.GetAllElements(list);

            return list;
        }

        // adds all modified elements in the package and sub-packages  to a list
        public List<Element> GetAllModifiedElements(List<Element> list)
        {
            foreach (Element elem in modifiedElements) {
                list.Add(elem);
                foreach (Element subElem in elem.ModifiedElements)
                {
                    list.Add(subElem);
                }
            }

            foreach (Package subPack in subPackages)
                subPack.GetAllModifiedElements(list);

            return list;
        }

        // adds all added elements in the package and sub-packages  to a list
        public List<Element> GetAllAddedElements(List<Element> list)
        {
            foreach (Element elem in addedElements)
            {
                list.Add(elem);
                foreach (Element subElem in elem.AddedElements)
                {
                    list.Add(subElem);
                }
            }

            foreach (Package subPack in addedSubPackages)
                subPack.GetAllElements(list);

            foreach (Package subPack in subPackages)
                subPack.GetAllAddedElements(list);

            return list;
        }

        // adds all removed elements in the package and sub-packages  to a list
        public List<Element> GetAllRemovedElements(List<Element> list)
        {
            foreach (Element elem in removedElements)
            {
                list.Add(elem);
                foreach (Element subElem in elem.RemovedElements)
                {
                    list.Add(subElem);
                }
            }

            foreach (Package subPack in removedSubPackages)
                subPack.GetAllElements(list);

            foreach (Package subPack in subPackages)
                subPack.GetAllRemovedElements(list);

            return list;
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

            foreach (Package subPack in subPackages)
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

            foreach (Package subPack in subPackages)
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

            foreach (Package subPack in subPackages)
                subPack.GetAllRemovedAttributes(list);

            return list;
        }

        // adds all sub-packages in the package and sub-packages to a list
        public List<Package> GetAllSubPackages(List<Package> list)
        {
            foreach (Package subPack in subPackages)
                list.Add(subPack);

            foreach (Package subPack in subPackages)
                subPack.GetAllSubPackages(list);

            return list;
        }

        // adds all modified sub-packages in the package and sub-packages to a list
        public List<Package> GetAllModifiedSubPackages(List<Package> list)
        {
            foreach (Package subPack in modifiedSubPackages)
                list.Add(subPack);

            foreach (Package subPack in subPackages)
                subPack.GetAllModifiedSubPackages(list);

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

            foreach (Package subPack in subPackages)
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

            foreach (Package subPack in subPackages)
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
                path = pack.parentPackage.name + "::" + path;
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

        // adding changes for the elements and sub-packages of the package (used for new packages)
        private void AddChangesForAllNewSubPackagesAndElements(int ident, Package package, bool RelevantOnly)
        {
            changes.Add(new MMChange("+ Package " + package.GetPath(), true).AppendTabs(ident));

            foreach (Element elem in package.Elements)
            {
                if (RelevantOnly && elem.IgnoreElement())
                    continue;

                changes.Add(new MMChange("+ Element " + elem.GetPath() + " " + elem.Name, false).AppendTabs(ident + 1));

                foreach (Attribute attr in elem.Attributes)
                {
                    if (RelevantOnly && attr.IgnoreAttribute())
                        continue;

                    changes.Add(new MMChange("+ Attribute " + attr.GetPath(), false).AppendTabs(ident + 2));
                }

                foreach (Connector conn in elem.SourceConnectors)
                {
                    if (RelevantOnly && conn.IgnoreConector())
                        continue;

                    changes.Add(new MMChange("+ Connector (source) " + conn.GetPath(), false).AppendTabs(ident + 2));
                }

                foreach (Connector conn in elem.TargetConnectors)
                {
                    if (RelevantOnly && conn.IgnoreConector())
                        continue;

                    changes.Add(new MMChange("+ Connector (target) " + conn.GetPath(), false).AppendTabs(ident + 2));
                }
            }

            foreach (Package pack in package.SubPackages)
            {
                if (RelevantOnly && IgnorePackage())
                    continue;

                AddChangesForAllNewSubPackagesAndElements(ident + 1, pack, RelevantOnly);
            }
        }

        // adding changes for  the elements and sub-packages of the package (used for old packages)
        private void AddChangesForAllRemovedSubPackagesAndElements(int ident, Package package, bool RelevantOnly)
        {
            changes.Add(new MMChange("- Package " + package.GetPath(), true).AppendTabs(ident));

            foreach (Element elem in package.Elements)
            {
                if (RelevantOnly && elem.IgnoreElement())
                    continue;

                changes.Add(new MMChange("- Element " + elem.GetPath() + " " + elem.Name, false).AppendTabs(ident + 1));

                foreach (Attribute attr in elem.Attributes)
                {
                    if (RelevantOnly && attr.IgnoreAttribute())
                        continue;

                    changes.Add(new MMChange("- Attribute " + attr.GetPath(), false).AppendTabs(ident + 2));
                }

                foreach (Connector conn in elem.SourceConnectors)
                {
                    if (RelevantOnly && conn.IgnoreConector())
                        continue;

                    changes.Add(new MMChange("- Connector (source) " + conn.GetPath(), false).AppendTabs(ident + 2));
                }

                foreach (Connector conn in elem.TargetConnectors)
                {
                    if (RelevantOnly && conn.IgnoreConector())
                        continue;

                    changes.Add(new MMChange("- Connector (target) " + conn.GetPath(), false).AppendTabs(ident + 2));
                }
            }

            foreach (Package pack in package.SubPackages)
            {
                if (RelevantOnly && IgnorePackage())
                    continue;

                AddChangesForAllRemovedSubPackagesAndElements(ident + 1, pack, RelevantOnly);
            }
        }

        // compares two packages in two releases
        public int ComparePackages(Package oldPackage, bool RelevantOnly)
        {
            if (RelevantOnly && IgnorePackage())
                return 0;

            if (!name.Equals(oldPackage.Name))  // number of changes not increased as the change in the name of a package is not considered a change
                changes.Add(new MMChange("~ Name: " + oldPackage.Name + " -> " + name, true).AppendTabs(1));

            // checking if the sub-package is changed or added in the new model
            foreach (Package subPackage in subPackages)
            {
                if (RelevantOnly && subPackage.IgnorePackage())
                    continue;

                Package oldSubPackage = oldPackage.GetSubPackageByName(subPackage.Name);

                int num = 0;

                // chacking if the package is added to the new model
                if (oldSubPackage == null)
                {
                    numOfChanges += subPackage.NumOfAllModifiableElements(RelevantOnly); ;
                    addedSubPackages.Add(subPackage);   // package with its all sub-packages and elements is added to the addedPackages list

                    AddChangesForAllNewSubPackagesAndElements(0, subPackage, RelevantOnly); // add changes for all elements and sub-packages of the old sub-package
                }

                // chacking if the package is changed in the new model
                else if ((num = subPackage.ComparePackages(oldSubPackage, RelevantOnly)) != 0)
                {
                    numOfChanges += num;
                    modifiedSubPackages.Add(subPackage);
                }
            }

            // checking if the sub-package is removed in the new model
            foreach (Package oldSubPackage in oldPackage.SubPackages)
            {
                if (RelevantOnly && oldSubPackage.IgnorePackage())
                    continue;

                Package subPackage = GetSubPackageByName(oldSubPackage.Name);

                if (subPackage == null)
                {
                    numOfChanges += oldSubPackage.NumOfAllModifiableElements(RelevantOnly);
                    removedSubPackages.Add(oldSubPackage);  // package with its all sub-packages and elements is added to the removedPackages list

                    AddChangesForAllRemovedSubPackagesAndElements(1, oldSubPackage, RelevantOnly);  // add changes for all elements and sub-packages of the old sub-package
                }
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
                    changes.Add(new MMChange("+ Element " + element.GetPath() + " " + element.Name, false));

                    foreach (Attribute attr in element.Attributes)
                    {
                        if (RelevantOnly && attr.IgnoreAttribute())
                            continue;

                        changes.Add(new MMChange("+ Attribute " + attr.GetPath(), false).AppendTabs(1));
                    }

                    foreach (Connector conn in element.SourceConnectors)
                    {
                        if (RelevantOnly && conn.IgnoreConector())
                            continue;

                        changes.Add(new MMChange("+ Connector (source) " + conn.GetPath(), false).AppendTabs(1));
                    }

                    foreach (Connector conn in element.TargetConnectors)
                    {
                        if (RelevantOnly && conn.IgnoreConector())
                            continue;

                        changes.Add(new MMChange("+ Connector (target) " + conn.GetPath(), false).AppendTabs(1));
                    }
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
                    changes.Add(new MMChange("- Element " + oldElement.GetPath() + " " + oldElement.Name, false));

                    foreach (Attribute attr in oldElement.Attributes)
                    {
                        if (RelevantOnly && attr.IgnoreAttribute())
                            continue;

                        changes.Add(new MMChange("- Attribute " + attr.GetPath(), false).AppendTabs(1));
                    }

                    foreach (Connector conn in oldElement.SourceConnectors)
                    {
                        if (RelevantOnly && conn.IgnoreConector())
                            continue;

                        changes.Add(new MMChange("- Connector (source) " + conn.GetPath(), false).AppendTabs(1));
                    }

                    foreach (Connector conn in oldElement.TargetConnectors)
                    {
                        if (RelevantOnly && conn.IgnoreConector())
                            continue;

                        changes.Add(new MMChange("- Connector (target) " + conn.GetPath(), false).AppendTabs(1));
                    }
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

        /*
        public Element FindElement(string name)
        {
            // For imports
            if (name.Contains("."))
            {
                int firstPoint = name.IndexOf(".");
                string packageName = name.Substring(0, firstPoint);
                string qualifiedName = name.Substring(firstPoint + 1, name.Length - firstPoint - 1);

                //Console.WriteLine("packageName = " + packageName);
                //Console.WriteLine("qualifiedName = " + qualifiedName);
                foreach (Package package in subPackages)
                {
                    if (packageName == package.name)
                    {
                        package.FindElement(qualifiedName);
                    }
                }
            }
            else
            {
                foreach (Element element in elements)
                {
                    if (element.Name == name)
                    {
                        return element;
                    }
                }
            }


            return null;
        }
        */
    }
}
