using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using ModelicaChangeAnalyzer.Changes;

namespace ModelicaChangeAnalyzer.Datamodel
{
    public class MetaModel
    {
        // Attributes
        private String version = "";
        private List<Package> packages = new List<Package>();

        // Changes
        private int numOfChanges;
        private List<Change> changes = new List<Change>();
        private List<Package> modifiedPackages = new List<Package>();
        private List<Package> removedPackages = new List<Package>();
        private List<Package> addedPackages = new List<Package>();  

        #region Loading

        public MetaModel(string version)
        {
            this.version = version;
        }

        public void AddPackage(Package package)
        {
            packages.Add(package);
            package.Metamodel = this;
        }

        public override string ToString()
        {
            string ret = "Model " + version + "\n";

            foreach (Package pack in packages)
                ret += pack.ToString();

            return ret;
        }

        #endregion

        #region Calculate number of

        public int NumberOfPackages(bool relevantOnly)
        {
            int numOfPackages = 0;

            foreach (Package pack in packages)
                numOfPackages += pack.NumberOfPackages(relevantOnly);

            return numOfPackages;
        }

        public int NumberOfElements(bool relevantOnly)
        {
            int numberOfElements = 0;

            foreach (Package package in packages)
                numberOfElements += package.NumberOfElements(relevantOnly);

            return numberOfElements;
        }

        public int NumberOfConnectors(bool relevantOnly)
        {
            int numberOfConnectors = 0;

            foreach (Package package in packages)
                numberOfConnectors += package.NumberOfConnectors(relevantOnly);

            return numberOfConnectors;
        }

        public int NumberOfAttributes(bool relevantOnly)
        {
            int numberOfAttributes = 0;

            foreach (Package package in packages)
                numberOfAttributes += package.NumberOfAttributes(relevantOnly);

            return numberOfAttributes;
        }

        public int NumberOfModifiedElements()
        {
            int numberOfChangedElements = 0;

            foreach (Package package in packages)
                numberOfChangedElements += package.NumberOfModifiedElements();

            return numberOfChangedElements;
        }

        public int NumberOfAddedElements()
        {
            int numberOfAddedElements = 0;

            foreach (Package pack in addedPackages)
                numberOfAddedElements += pack.NumberOfElements(false); // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package package in packages)
                numberOfAddedElements += package.NumberOfAddedElements();

            return numberOfAddedElements;
        }

        public int NumberOfRemovedElements()
        {
            int numberOfRemovedElements = 0;

            foreach (Package pack in removedPackages)
                numberOfRemovedElements += pack.NumberOfElements(false); // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package package in packages)
                numberOfRemovedElements += package.NumberOfRemovedElements();

            return numberOfRemovedElements;
        }

        public int NumberOfModifiedPackages()
        {
            int numberOfChangedPackages = 0;

            foreach (Package package in modifiedPackages)
                numberOfChangedPackages++;

            foreach (Package package in packages)
                numberOfChangedPackages += package.NumberOfModifiedSubPackages();

            return numberOfChangedPackages;
        }

        public int NumberOfAddedPackages()
        {
            int numberOfAddedPackages = 0;

            foreach (Package package in addedPackages)
                numberOfAddedPackages += package.NumberOfPackages(false); // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package package in packages)
                numberOfAddedPackages += package.NumberOfAddedSubPackages();

            return numberOfAddedPackages;
        }

        public int NumberOfRemovedPackages()
        {
            int numberOfRemovedPackages = 0;

            foreach (Package package in removedPackages)
                numberOfRemovedPackages += package.NumberOfPackages(false); // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package package in packages)
                numberOfRemovedPackages += package.NumberOfRemovedSubPackages();

            return numberOfRemovedPackages;
        }

        public int NumberOfModifiedConnectors()
        {
            int numberOfModifiedConnectors = 0;

            foreach (Package package in packages)
                numberOfModifiedConnectors += package.NumberOfModifiedConnectors();

            return numberOfModifiedConnectors;
        }

        public int NumberOfAddedConnectors()
        {
            int numberOfAddedConnectors = 0;

            foreach (Package package in addedPackages)
                numberOfAddedConnectors += package.NumberOfConnectors(false);

            foreach (Package package in packages)
                numberOfAddedConnectors += package.NumberOfAddedConnectors();

            return numberOfAddedConnectors;
        }

        public int NumberOfRemovedConnectors()
        {
            int numberOfRemovedConnectors = 0;

            foreach (Package package in removedPackages)
                numberOfRemovedConnectors += package.NumberOfConnectors(false);

            foreach (Package package in packages)
                numberOfRemovedConnectors += package.NumberOfRemovedConnectors();

            return numberOfRemovedConnectors;
        }

        public int NumberOfModifiedAttributes()
        {
            int numberOfModifiedAtributes = 0;

            foreach (Package package in packages)
                numberOfModifiedAtributes += package.NumberOfModifiedAttributes();

            return numberOfModifiedAtributes;
        }

        public int NumberOfAddedAttributes()
        {
            int numberOfAddedAttributes = 0;

            foreach (Package package in addedPackages)
                numberOfAddedAttributes += package.NumberOfAttributes(false);    // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package package in packages)
                numberOfAddedAttributes += package.NumberOfAddedAttributes();

            return numberOfAddedAttributes;
        }

        public int NumberOfRemovedAttributes()
        {
            int numberOfRemovedAttributes = 0;

            foreach (Package package in removedPackages)
                numberOfRemovedAttributes += package.NumberOfAttributes(false);    // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package package in packages)
                numberOfRemovedAttributes += package.NumberOfRemovedAttributes();

            return numberOfRemovedAttributes;
        }

        #endregion

        #region Retrieve objects

        // retrieves all changes in the model
        public List<Change> GetChanges()
        {
            List<Change> listOfChanges = new List<Change>(changes);

            /*foreach (Package pack in addedPackages)
            {
                pack.printAsAddedPackage(listOfChanges, 0);
            }

            foreach (Package pack in modifiedPackages)
            {
                pack.printAsModifiedPackage(listOfChanges, 0);
            }

            foreach (Package pack in removedPackages)
            {
                pack.printAsAddedPackage(listOfChanges, 0);
            }*/

            foreach (Package pack in packages)
                foreach (Change chng in pack.GetChanges())
                    listOfChanges.Add(chng);

            return listOfChanges;
        }

        // retrieves all modified elements in the model
        public List<Element> GetAllModifiedElements()
        {
            List<Element> list = new List<Element>();

            foreach (Package pack in packages)
                pack.GetAllModifiedElements(list);

            return list;
        }

        // retrieves all added elements in the model
        public List<Element> GetAllAddedElements()
        {
            List<Element> list = new List<Element>();

            foreach (Package pack in addedPackages)
                pack.GetAllElements(list);

            foreach (Package pack in packages)
                pack.GetAllAddedElements(list);


            return list;
        }

        // retrieves all removed elements in the model
        public List<Element> GetAllRemovedElements()
        {
            List<Element> list = new List<Element>();

            foreach (Package pack in removedPackages)
                pack.GetAllElements(list);

            foreach (Package pack in packages)
                pack.GetAllRemovedElements(list);

            return list;
        }

        public List<Connector> GetAllModifiedConnectors()
        {
            List<Connector> list = new List<Connector>();

            foreach (Package pack in packages)
                pack.GetAllModifiedConnectors(list);

            return list;
        }

        public List<Connector> GetAllAddedConnectors()
        {
            List<Connector> list = new List<Connector>();

            foreach (Package pack in addedPackages)
                pack.GetAllConnectors(list);

            foreach (Package pack in packages)
                pack.GetAllAddedConnectors(list);

            return list;
        }

        public List<Connector> GetAllRemovedConnectors()
        {
            List<Connector> list = new List<Connector>();

            foreach (Package pack in removedPackages)
                pack.GetAllConnectors(list);

            foreach (Package pack in packages)
                pack.GetAllRemovedConnectors(list);

            return list;
        }

        // retrieves all modified attributes in the model
        public List<Attribute> GetAllModifiedAttributes()
        {
            List<Attribute> list = new List<Attribute>();

            foreach (Package pack in packages)
                pack.GetAllModifiedAttributes(list);

            return list;
        }

        // retrieves all added attributes in the model
        public List<Attribute> GetAllAddedAttributes()
        {
            List<Attribute> list = new List<Attribute>();

            foreach (Package pack in addedPackages)
                pack.GetAllAttributes(list);

            foreach (Package pack in packages)
                pack.GetAllAddedAttributes(list);

            return list;
        }

        // retrieves all removed attributes in the model
        public List<Attribute> GetAllRemovedAttributes()
        {
            List<Attribute> list = new List<Attribute>();

            foreach (Package pack in removedPackages)
                pack.GetAllAttributes(list);

            foreach (Package pack in packages)
                pack.GetAllRemovedAttributes(list);

            return list;
        }

        // retrieves all modified packages in the model
        public List<Package> GetAllModifiedPackages()
        {
            List<Package> list = new List<Package>();

            foreach (Package pack in modifiedPackages)
                list.Add(pack);

            foreach (Package pack in packages)
                pack.GetAllModifiedSubPackages(list);

            return list;
        }

        // retrieves all added packages in the model
        public List<Package> GetAllAddedPackages()
        {
            List<Package> list = new List<Package>();

            foreach (Package pack in addedPackages)
            {
                list.Add(pack);
                pack.GetAllSubPackages(list);
            }

            foreach (Package pack in packages)
                pack.GetAllAddedSubPackages(list);

            return list;
        }

        // retrieves all removed packages in the model
        public List<Package> GetAllRemovedPackages()
        {
            List<Package> list = new List<Package>();

            foreach (Package pack in removedPackages)
            {
                list.Add(pack);
                pack.GetAllSubPackages(list);
            }

            foreach (Package pack in packages)
                pack.GetAllRemovedSubPackages(list);

            return list;
        }

        public Package FindPackageByName(string name)
        {
            foreach (Package package in packages)
                if (package.Name.Equals(name))
                    return package;

            return null;
        }

        public Package FindPackageByPath(string packagePath)
        {
            string[] pathParts = Regex.Split(packagePath, ".");

            Package package = this.FindPackageByName(pathParts[0]);

            if (package == null)
                return null;

            for (int i = 1; i < pathParts.Length; i++)
            {
                package = package.GetSubPackageByName(pathParts[i]);

                if (package == null)
                    return null;
            }

            return package;
        }

        public Element FindElementByPath(string elementPath)
        {
            string[] pathParts = Regex.Split(elementPath, ".");

            Package package = this.FindPackageByName(pathParts[0]);

            if (package == null)
                return null;

            for (int i = 1; i < pathParts.Length - 1; i++)
            {
                package = package.GetSubPackageByName(pathParts[i]);

                if (package == null)
                    return null;
            }

            return package.GetElementByName(pathParts[pathParts.Length - 1]);
        }

        #endregion

        #region Calculation

        // reseting the calulation fields in the model before the next calculation (done for all packages, elements, etc. in the model)
        public void ResetCalculation()
        {
            numOfChanges = 0;
            changes.Clear();
            modifiedPackages.Clear();
            removedPackages.Clear();
            addedPackages.Clear();

            foreach (Package pack in packages)
                pack.ResetCalculation();
        }

        // comparing two different models
        public int CompareModels(MetaModel oldModel, bool RelevantOnly)
        {
            foreach (Package package in packages)
            {
                if (RelevantOnly && package.IgnorePackage())
                    continue;

                Package oldPackage = oldModel.FindPackageByName(package.Name);

                int num = 0;

                // checking if the package is added to the new model
                if (oldPackage == null)
                {
                    numOfChanges += package.NumOfAllModifiableElements(RelevantOnly);
                    addedPackages.Add(package);
                    AddChangesForAllNewSubPackagesAndElements(0, package); 
                }
                // checking if the package is changed in the new model
                else if ((num = package.ComparePackages(oldPackage, RelevantOnly)) != 0)
                {
                    numOfChanges += num;
                    modifiedPackages.Add(package);
                }
            }

            // checking if the package is removed in the new model
            foreach (Package oldPackage in oldModel.Packages)
            {
                if (RelevantOnly && oldPackage.IgnorePackage())
                    continue;

                Package package = FindPackageByName(oldPackage.Name);

                if (package == null)
                {
                    numOfChanges += oldPackage.NumOfAllModifiableElements(RelevantOnly);
                    removedPackages.Add(oldPackage);
                    AddChangesForAllRemovedSubPackagesAndElements(0, oldPackage);
                }
            }

            return numOfChanges;
        }

        private void AddChangesForAllRemovedSubPackagesAndElements(int ident, Package package)
        {
            changes.Add(new Change("- Package " + package.GetPath(), true).AppendTabs(ident++));  // not counted as a change

            foreach (Element elem in package.Elements)
            {
                changes.Add(new Change("- Element " + elem.GetPath(), false).AppendTabs(ident));

                foreach (Attribute attr in elem.Attributes)
                    changes.Add(new Change("- Attribute " + attr.GetPath(), false).AppendTabs(ident + 1));

                foreach (Connector conn in elem.SourceConnectors)
                    changes.Add(new Change("- Connector (source) " + conn.GetPath(), false).AppendTabs(ident + 1));

                foreach (Connector conn in elem.TargetConnectors)
                    changes.Add(new Change("- Connector (target) " + conn.GetPath(), false).AppendTabs(ident + 1));
            }

            foreach (Package pack in package.SubPackages)
                AddChangesForAllRemovedSubPackagesAndElements(ident, pack);
        }

        private void AddChangesForAllNewSubPackagesAndElements(int ident, Package package)
        {
            changes.Add(new Change("+ Package " + package.GetPath(), true).AppendTabs(ident++));  // not counted as a change

            foreach (Element elem in package.Elements)
            {
                changes.Add(new Change("+ Element " + elem.GetPath(), false).AppendTabs(ident));

                foreach (Attribute attr in elem.Attributes)
                    changes.Add(new Change("+ Attribute " + attr.GetPath(), false).AppendTabs(ident + 1));

                foreach (Connector conn in elem.SourceConnectors)
                    changes.Add(new Change("+ Connector (source) " + conn.GetPath(), false).AppendTabs(ident + 1));

                foreach (Connector conn in elem.TargetConnectors)
                    changes.Add(new Change("+ Connector (target) " + conn.GetPath(), false).AppendTabs(ident + 1));
            }

            foreach (Package pack in package.SubPackages)
                AddChangesForAllNewSubPackagesAndElements(ident, pack);
        }
 
        #endregion

        #region Getters and setters

        public String Version
        {
            get { return version; }
            set { version = value; }
        }

        public List<Package> Packages
        {
            get { return packages; }
        }

        public List<Change> Changes
        {
            get { return changes; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
        }

        public List<Package> ModifiedPackages
        {
            get { return modifiedPackages; }
        }

        public List<Package> RemovedPackages
        {
            get { return removedPackages; }
        }

        public List<Package> AddedPackages
        {
            get { return addedPackages; }
        }

        #endregion
    }
}
