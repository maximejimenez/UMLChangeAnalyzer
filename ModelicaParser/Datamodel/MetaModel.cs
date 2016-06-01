using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModelicaParser.Datamodel
{
    class MetaModel
    {
        // Attributes
        private String version = "";
        private List<Package> packages = new List<Package>();

        // Changes
        private int numOfChanges;
        private List<MMChange> changes = new List<MMChange>();
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
            int numOfPackages = packages.Count;

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

        public int NumberOfAttributes(bool relevantOnly)
        {
            int numberOfAttributes = 0;

            foreach (Package package in packages)
                numberOfAttributes += package.NumberOfAttributes(relevantOnly);

            return numberOfAttributes;
        }

        /*public int DeptOfInheritance(bool relevantOnly)
        {
            int DeptOfInheritance = 0;

            foreach (EA_Package package in packages)
                DeptOfInheritance += package.DeptOfInheritance(this, relevantOnly);

            return DeptOfInheritance;
        }*/

        public int NumberOfModifiedElements()
        {
            int numberOfChangedElements = 0;

            foreach (Package package in modifiedPackages)
                numberOfChangedElements += package.NumberOfModifiedElements();

            return numberOfChangedElements;
        }

        public int NumberOfAddedElements()
        {
            int numberOfAddedElements = 0;

            foreach (Package pack in addedPackages)
                numberOfAddedElements += pack.NumberOfElements(false); // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package package in modifiedPackages)
                numberOfAddedElements += package.NumberOfAddedElements();

            return numberOfAddedElements;
        }

        public int NumberOfRemovedElements()
        {
            int numberOfRemovedElements = 0;

            foreach (Package pack in removedPackages)
                numberOfRemovedElements += pack.NumberOfElements(false); // false because we already excluded non relevant packages/elements, etc. during compare

            foreach (Package package in modifiedPackages)
                numberOfRemovedElements += package.NumberOfRemovedElements();

            return numberOfRemovedElements;
        }

        public int NumberOfModifiedPackages()
        {
            int numberOfChangedPackages = 0;

            foreach (Package package in modifiedPackages)
            {
                numberOfChangedPackages++;
                numberOfChangedPackages += package.NumberOfModifiedSubPackages();
            }

            return numberOfChangedPackages;
        }

        public int NumberOfAddedPackages()
        {
            int numberOfAddedPackages = 0;

            foreach (Package package in addedPackages)
            {
                numberOfAddedPackages++;
                numberOfAddedPackages += package.NumberOfPackages(false); // false because we already excluded non relevant packages/elements, etc. during compare
            }

            foreach (Package package in modifiedPackages)
                numberOfAddedPackages += package.NumberOfAddedSubPackages();

            return numberOfAddedPackages;
        }

        public int NumberOfRemovedPackages()
        {
            int numberOfRemovedPackages = 0;

            foreach (Package package in removedPackages)
            {
                numberOfRemovedPackages++;
                numberOfRemovedPackages += package.NumberOfPackages(false); // false because we already excluded non relevant packages/elements, etc. during compare
            }

            foreach (Package package in modifiedPackages)
                numberOfRemovedPackages += package.NumberOfRemovedSubPackages();

            return numberOfRemovedPackages;
        }

        public int NumberOfModifiedAttributes()
        {
            int numberOfModifiedAtributes = 0;

            foreach (Package package in modifiedPackages)
                numberOfModifiedAtributes += package.NumberOfModifiedAttributes();

            return numberOfModifiedAtributes;
        }

        public int NumberOfAddedAttributes()
        {
            int numberOfAddedAttributes = 0;

            foreach (Package package in modifiedPackages)
                numberOfAddedAttributes += package.NumberOfAddedAttributes();

            foreach (Package package in addedPackages)
                numberOfAddedAttributes += package.NumberOfAttributes(false);    // false because we already excluded non relevant packages/elements, etc. during compare

            return numberOfAddedAttributes;
        }

        public int NumberOfRemovedAttributes()
        {
            int numberOfRemovedAttributes = 0;

            foreach (Package package in modifiedPackages)
                numberOfRemovedAttributes += package.NumberOfRemovedAttributes();

            foreach (Package package in removedPackages)
                numberOfRemovedAttributes += package.NumberOfAttributes(false);    // false because we already excluded non relevant packages/elements, etc. during compare

            return numberOfRemovedAttributes;
        }

        public int CalculateNumberOfConnectedElements(bool relevantOnly)
        {
            int numberOfConnectedElements = 0;

            foreach (Package pack in packages)
            {
                if (relevantOnly && pack.IgnorePackage())
                    continue;

                numberOfConnectedElements += pack.CalculateNumberOfConnectedElements(relevantOnly);
            }

            return numberOfConnectedElements;
        }


        #endregion

        #region Retrieve objects

        // retrieves all changes in the model
        public List<MMChange> GetChanges()
        {
            List<MMChange> listOfChanges = new List<MMChange>(changes);

            foreach (Package pack in packages)
                foreach (MMChange chng in pack.GetChanges())
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

        /*
        public EA_Package FindPackageByName(string name)
        {
            foreach (EA_Package package in packages)
                if (package.Name.Equals(name))
                    return package;

            return null;
        }

        public EA_Package FindPackageByPath(string packagePath)
        {
            string[] pathParts = Regex.Split(packagePath, "::");

            EA_Package package = this.FindPackageByName(pathParts[0]);

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

        public EA_Element FindElementByPath(string elementPath)
        {
            string[] pathParts = Regex.Split(elementPath, "::");

            EA_Package package = this.FindPackageByName(pathParts[0]);

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
        */

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
        /*
        // writing the elements and sub-packages of the package (used for new packages)
        private void AddChangesForAllNewSubPackagesAndElements(int ident, EA_Package package)
        {
            changes.Add(new ARChange("+ Package " + package.GetPath(), true).AppendTabs(ident++));  // not counted as a change

            foreach (EA_Element elem in package.Elements)
                changes.Add(new ARChange("+ Element " + elem.GetPath(), false).AppendTabs(ident));

            foreach (EA_Package pack in package.SubPackages)
                AddChangesForAllNewSubPackagesAndElements(ident, pack);
        }

        // writing the elements and sub-packages of the package (used for old packages)
        private void AddChangesForAllRemovedSubPackagesAndElements(int ident, EA_Package package)
        {
            changes.Add(new ARChange("- Package " + package.GetPath(), true).AppendTabs(ident++));  // not counted as a change

            foreach (EA_Element elem in package.Elements)
                changes.Add(new ARChange("- Element " + elem.GetPath(), false).AppendTabs(ident));

            foreach (EA_Package pack in package.SubPackages)
                AddChangesForAllRemovedSubPackagesAndElements(ident, pack);
        }

        // comparing two different models
        public int CompareModels(EA_Model oldModel, bool RelevantOnly)
        {
            // checking if the package is changed or added in the new model
            foreach (EA_Package package in packages)
            {
                if (RelevantOnly && package.IgnorePackage())
                    continue;

                EA_Package oldPackage = oldModel.FindPackageById(package.Id);

                int num = 0;

                // chacking if the package is added to the new model
                if (oldPackage == null)
                {
                    numOfChanges += package.NumOfAllModifiableElements(RelevantOnly);
                    addedPackages.Add(package); // package with its all sub-packages and elements is added to the addedPackages list

                    AddChangesForAllNewSubPackagesAndElements(1, package);    // add changes for all elements and sub-packages of the old sub-package
                }

                // chacking if the package is changed in the new model
                else if ((num = package.ComparePackages(oldPackage, RelevantOnly)) != 0)
                {
                    numOfChanges += num;
                    modifiedPackages.Add(package);
                }
            }

            // checking if the package is removed in the new model
            foreach (EA_Package oldPackage in oldModel.Packages)
            {
                if (RelevantOnly && oldPackage.IgnorePackage())
                    continue;

                EA_Package package = FindPackageById(oldPackage.Id);

                if (package == null)
                {
                    numOfChanges += oldPackage.NumOfAllModifiableElements(RelevantOnly);
                    removedPackages.Add(oldPackage);    // old package with its all sub-packages and elements is added to the removedPackages list

                    AddChangesForAllRemovedSubPackagesAndElements(0, oldPackage);     // add changes for all elements and sub-packages of the old sub-package
                }
            }

            return numOfChanges;
        }
        */
 
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

        public List<MMChange> Changes
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

        /*public Element FindElement(string name)
        {
            Element result = null;
            // For imports
            if(name.Contains(".")){
                int firstPoint = name.IndexOf(".");
                string packageName = name.Substring(0, firstPoint);
                string qualifiedName = name.Substring(firstPoint + 1, name.Length - firstPoint - 1);

                //Console.WriteLine("packageName = " + packageName);
                //Console.WriteLine("qualifiedName = " + qualifiedName);
                foreach (Package package in packages)
                {
                    if(packageName == package.Name){
                        package.FindElement(qualifiedName);
                    }
                }

            }
            else
            {
                foreach (Package package in packages)
                {
                    result = package.FindElement(name);
                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public void CompareWithMetamodel(MetaModel newMetamodel)
        {
            //TODO several packages

            Package oldPackage = packages.ElementAt(0);
            Package newPackage = newMetamodel.packages.ElementAt(0);

            // New uniontypes
            List<Element> oldUniontypes = new List<Element>(oldPackage.Elements);
            Dictionary<string, Element> newUniontypes = new Dictionary<string, Element>();
            foreach(Element element in newPackage.Elements)
            {
                newUniontypes.Add(element.Name, element);
            }

            foreach (Element uniontype in oldUniontypes)
            {
                Element newUniontype;
                if(newUniontypes.TryGetValue(uniontype.Name, out newUniontype)){
                    uniontype.Compare(newUniontype);
                    // Add to modifiedElement if not equals
                    newUniontypes.Remove(uniontype.Name);
                    oldUniontypes.Remove(uniontype);
                }
                else
                {
                    // Deleted elements
                    removedElements.Add(uniontype);
                    oldUniontypes.Remove(uniontype);
                }
            }

            foreach(Element addedUniontype in newUniontypes.Values)
            {
                // Added elements
                addedElements.Add(addedUniontype);
            }
        }*/
    }
}
