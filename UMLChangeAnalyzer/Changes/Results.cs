using System;
using System.Collections.Generic;
using System.Text;
using ModelicaChangeAnalyzer.Datamodel;

namespace ModelicaChangeAnalyzer.Changes
{
    // contains the calculation results (changes, metrics and features)
    public class Results
    {
        /* ***** SIZE METRICS ***** */
        private int numOfElementsMod1 = 0;
        private int numOfElementsMod2 = 0;
        private int numOfAttributesMod1 = 0;
        private int numOfAttributesMod2 = 0;
        private int numOfConnectorsMod1 = 0;
        private int numOfConnectorsMod2 = 0;
        private int numOfPackagesMod1 = 0;
        private int numOfPackagedMod2 = 0;

        /* ***** CHANGE METRICS ***** */
        private int numOfChanges = 0;
        private int numOfChangedPackages = 0;
        private int numOfModifiedPackages = 0;
        private int numOfAddedPackages = 0;
        private int numOfRemovedPackages = 0;
        private int numOfChangedElements = 0;
        private int numOfAddedElements = 0;
        private int numOfModifiedElements = 0;
        private int numOfRemovedElements = 0;
        private int numOfChangedConnectors = 0;
        private int numOfModifiedConnectors = 0;
        private int numOfAddedConnectors = 0;
        private int numOfRemovedConnectors = 0;
        private int numOfChangedAttributes = 0;
        private int numOfModifiedAttributes = 0;
        private int numOfAddedAttributes = 0;
        private int numOfRemovedAttributes = 0;

        /* ***** CHANGES ***** */
        private List<Change> changes = new List<Change>();
        private List<Package> modifiedPackages = new List<Package>();
        private List<Package> addedPackages = new List<Package>();
        private List<Package> removedPackages = new List<Package>();
        private List<Element> modifiedElements = new List<Element>();
        private List<Element> removedElements = new List<Element>();
        private List<Element> addedElements = new List<Element>();
        private List<Connector> modifiedConnectors = new List<Connector>();
        private List<Connector> removedConnectors = new List<Connector>();
        private List<Connector> addedConnectors = new List<Connector>(); 
        private List<ModelicaChangeAnalyzer.Datamodel.Attribute> modifiedAttributes = new List<ModelicaChangeAnalyzer.Datamodel.Attribute>();
        private List<ModelicaChangeAnalyzer.Datamodel.Attribute> addedAttributes = new List<ModelicaChangeAnalyzer.Datamodel.Attribute>();
        private List<ModelicaChangeAnalyzer.Datamodel.Attribute> removedAttributes = new List<ModelicaChangeAnalyzer.Datamodel.Attribute>();

        #region Compare

        // calculating results from comparing two models
        public void CalculateModels(MetaModel model1, MetaModel model2, bool relevancy)
        {
            model2.CompareModels(model1, relevancy);

            NumOfElementsMod1 = model1.NumberOfElements(relevancy);
            NumOfElementsMod2 = model2.NumberOfElements(relevancy);
            NumOfAttributesMod1 = model1.NumberOfAttributes(relevancy);
            NumOfAttributesMod2 = model2.NumberOfAttributes(relevancy);
            numOfConnectorsMod1 = model1.NumberOfConnectors(relevancy);
            numOfConnectorsMod2 = model2.NumberOfConnectors(relevancy);
            NumOfPackagesMod1 = model1.NumberOfPackages(relevancy);
            NumOfPackagedMod2 = model2.NumberOfPackages(relevancy);

            NumOfChanges = model2.NumOfChanges;
            NumOfModifiedPackages = model2.NumberOfModifiedPackages();
            NumOfAddedPackages = model2.NumberOfAddedPackages();
            NumOfRemovedPackages = model2.NumberOfRemovedPackages();
            NumOfModifiedElements = model2.NumberOfModifiedElements();
            NumOfAddedElements = model2.NumberOfAddedElements();
            NumOfRemovedElements = model2.NumberOfRemovedElements();
            NumOfModifiedConnectors = model2.NumberOfModifiedConnectors();
            NumOfAddedConnectors = model2.NumberOfAddedConnectors();
            NumOfRemovedConnectors = model2.NumberOfRemovedConnectors();
            NumOfModifiedAttributes = model2.NumberOfModifiedAttributes();
            NumOfAddedAttributes = model2.NumberOfAddedAttributes();
            NumOfRemovedAttributes = model2.NumberOfRemovedAttributes();

            Changes = model2.GetChanges();
            ModifiedPackages = model2.GetAllModifiedPackages();
            AddedPackages = model2.GetAllAddedPackages();
            RemovedPackages = model2.GetAllRemovedPackages();
            ModifiedElements = model2.GetAllModifiedElements();
            AddedElements = model2.GetAllAddedElements();
            RemovedElements = model2.GetAllRemovedElements();
            ModifiedConnectors = model2.GetAllModifiedConnectors();
            AddedConnectors = model2.GetAllAddedConnectors();
            RemovedConnectors = model2.GetAllRemovedConnectors();
            ModifiedAttributes = model2.GetAllModifiedAttributes();
            AddedAttributes = model2.GetAllAddedAttributes();
            RemovedAttributes = model2.GetAllRemovedAttributes();
        }

        // calculating results from comparing two packages for the non UTM role
        public void CalculatePackages(MetaModel model1, MetaModel model2, Package package1, Package package2, bool relevancy)
        {
            package2.ComparePackages(package1, relevancy);

            NumOfElementsMod1 += package1.NumberOfElements(relevancy);
            NumOfElementsMod2 += package2.NumberOfElements(relevancy);
            NumOfAttributesMod1 += package1.NumberOfAttributes(relevancy);
            NumOfAttributesMod2 += package2.NumberOfAttributes(relevancy);
            numOfConnectorsMod1 = package1.NumberOfConnectors(relevancy);
            numOfConnectorsMod2 = package2.NumberOfConnectors(relevancy);
            NumOfPackagesMod1 += package1.NumberOfPackages(relevancy);
            NumOfPackagedMod2 += package2.NumberOfPackages(relevancy);

            NumOfChanges += package2.NumOfChanges;
            NumOfModifiedPackages += package2.NumberOfModifiedSubPackages();
            NumOfAddedPackages += package2.NumberOfAddedSubPackages();
            NumOfRemovedPackages += package2.NumberOfRemovedSubPackages();
            NumOfModifiedElements += package2.NumberOfModifiedElements();
            NumOfAddedElements += package2.NumberOfAddedElements();
            NumOfRemovedElements += package2.NumberOfRemovedElements();
            NumOfModifiedAttributes += package2.NumberOfModifiedAttributes();
            NumOfAddedAttributes += package2.NumberOfAddedAttributes();
            NumOfRemovedAttributes += package2.NumberOfRemovedAttributes();

            foreach (Change chg in package2.GetChanges())
                Changes.Add(chg);

            package2.GetAllModifiedSubPackages(ModifiedPackages);
            package2.GetAllAddedSubPackages(AddedPackages);
            package2.GetAllRemovedSubPackages(RemovedPackages);
            package2.GetAllModifiedElements(ModifiedElements);
            package2.GetAllAddedElements(AddedElements);
            package2.GetAllRemovedElements(RemovedElements);
            package2.GetAllModifiedAttributes(ModifiedAttributes);
            package2.GetAllAddedAttributes(AddedAttributes);
            package2.GetAllRemovedAttributes(RemovedAttributes);
        }

        #endregion

        #region Getters and Setters

        public int NumOfElementsMod1
        {
            get { return numOfElementsMod1; }
            set { numOfElementsMod1 = value; }
        }

        public int NumOfElementsMod2
        {
            get { return numOfElementsMod2; }
            set { numOfElementsMod2 = value; }
        }

        public int NumOfAttributesMod1
        {
            get { return numOfAttributesMod1; }
            set { numOfAttributesMod1 = value; }
        }

        public int NumOfAttributesMod2
        {
            get { return numOfAttributesMod2; }
            set { numOfAttributesMod2 = value; }
        }

        public int NumOfConnectorsMod1
        {
            get { return numOfConnectorsMod1; }
            set { numOfConnectorsMod1 = value; }
        }

        public int NumOfConnectorsMod2
        {
            get { return numOfConnectorsMod2; }
            set { numOfConnectorsMod2 = value; }
        }

        public int NumOfPackagesMod1
        {
            get { return numOfPackagesMod1; }
            set { numOfPackagesMod1 = value; }
        }

        public int NumOfPackagedMod2
        {
            get { return numOfPackagedMod2; }
            set { numOfPackagedMod2 = value; }
        }

        public int NumOfChanges
        {
            get { return numOfChanges; }
            set { numOfChanges = value; }
        }

        public int NumOfChangedPackages
        {
            get { return numOfChangedPackages; }
            set { numOfChangedPackages = value; }
        }

        public int NumOfModifiedPackages
        {
            get { return numOfModifiedPackages; }
            set { numOfModifiedPackages = value; }
        }

        public int NumOfAddedPackages
        {
            get { return numOfAddedPackages; }
            set { numOfAddedPackages = value; }
        }

        public int NumOfRemovedPackages
        {
            get { return numOfRemovedPackages; }
            set { numOfRemovedPackages = value; }
        }

        public int NumOfChangedElements
        {
            get { return numOfChangedElements; }
            set { numOfChangedElements = value; }
        }

        public int NumOfModifiedElements
        {
            get { return numOfModifiedElements; }
            set { numOfModifiedElements = value; }
        }

        public int NumOfAddedElements
        {
            get { return numOfAddedElements; }
            set { numOfAddedElements = value; }
        }

        public int NumOfRemovedElements
        {
            get { return numOfRemovedElements; }
            set { numOfRemovedElements = value; }
        }

        public int NumOfChangedConnectors
        {
            get { return numOfChangedConnectors; }
            set { numOfChangedConnectors = value; }
        }

        public int NumOfModifiedConnectors
        {
            get { return numOfModifiedConnectors; }
            set { numOfModifiedConnectors = value; }
        }

        public int NumOfAddedConnectors
        {
            get { return numOfAddedConnectors; }
            set { numOfAddedConnectors = value; }
        }

        public int NumOfRemovedConnectors
        {
            get { return numOfRemovedConnectors; }
            set { numOfRemovedConnectors = value; }
        }

        public int NumOfChangedAttributes
        {
            get { return numOfChangedAttributes; }
            set { numOfChangedAttributes = value; }
        }

        public int NumOfModifiedAttributes
        {
            get { return numOfModifiedAttributes; }
            set { numOfModifiedAttributes = value; }
        }

        public int NumOfAddedAttributes
        {
            get { return numOfAddedAttributes; }
            set { numOfAddedAttributes = value; }
        }

        public int NumOfRemovedAttributes
        {
            get { return numOfRemovedAttributes; }
            set { numOfRemovedAttributes = value; }
        }

        public List<Change> Changes
        {
            get { return changes; }
            set { changes = value; }
        }

        public List<Package> ModifiedPackages
        {
            get { return modifiedPackages; }
            set { modifiedPackages = value; }
        }

        public List<Package> AddedPackages
        {
            get { return addedPackages; }
            set { addedPackages = value; }
        }

        public List<Package> RemovedPackages
        {
            get { return removedPackages; }
            set { removedPackages = value; }
        }

        public List<Element> ModifiedElements
        {
            get { return modifiedElements; }
            set { modifiedElements = value; }
        }

        public List<Element> AddedElements
        {
            get { return addedElements; }
            set { addedElements = value; }
        }

        public List<Element> RemovedElements
        {
            get { return removedElements; }
            set { removedElements = value; }
        }

        public List<Connector> ModifiedConnectors
        {
            get { return modifiedConnectors; }
            set { modifiedConnectors = value; }
        }

        public List<Connector> AddedConnectors
        {
            get { return addedConnectors; }
            set { addedConnectors = value; }
        }

        public List<Connector> RemovedConnectors
        {
            get { return removedConnectors; }
            set { removedConnectors = value; }
        }             

        public List<ModelicaChangeAnalyzer.Datamodel.Attribute> ModifiedAttributes
        {
            get { return modifiedAttributes; }
            set { modifiedAttributes = value; }
        }

        public List<ModelicaChangeAnalyzer.Datamodel.Attribute> AddedAttributes
        {
            get { return addedAttributes; }
            set { addedAttributes = value; }
        }

        public List<ModelicaChangeAnalyzer.Datamodel.Attribute> RemovedAttributes
        {
            get { return removedAttributes; }
            set { removedAttributes = value; }
        }

        #endregion
    }
}