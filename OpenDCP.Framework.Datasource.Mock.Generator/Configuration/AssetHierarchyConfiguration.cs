using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Configuration
{
    public class AssetHierarchyConfiguration
    {
        public virtual List<string> ElementNames { get; set; }
        public virtual List<string> AttributeNames { get; set; }
        public virtual List<string> LocationNames { get; set; }
        public virtual List<string> UoM { get; set; }
        public virtual List<string> Categories { get; set; }
        public virtual int MaxDepth { get; set; }
        public virtual int MaxElements { get; set; }
        public virtual int NumberOfElementTempaltes { get; set; }
        public virtual int MaxAttributesPerElement { get; set; }
        public virtual int StepAttributesPercentage { get; set; } // keep it whole number for now
        public virtual List<string> ValueTypes { get; set; }
        public virtual List<double> ValueTypesWeights { get; set; }
        public virtual string ConfigurationItemAttributeName { get; set; }
    }
}
