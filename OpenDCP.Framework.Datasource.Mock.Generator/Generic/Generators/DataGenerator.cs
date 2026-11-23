using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using System.Collections.Generic;


namespace OpenDCP.Framework.Datasource.Mock.Generator.Generic.Generators
{
    public abstract class DataGenerator
    {
        protected DataGenerator(int version)
        {
            Version = version;
        }
        public int Version { get; set; }

        public abstract List<AFAttributeValueSet> GetValues(AFHirachey hirachey, List<AFAttribute> attributes, List<long> unixtimes);

    }
}
