using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using System;
using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Generic.Generators
{
    public class ConstantDataGenerator : DataGenerator
    {
        public ConstantDataGenerator(int version) : base(version)
        {
        }

        public override List<AFAttributeValueSet> GetValues(AFHirachey hirachey, List<AFAttribute> attributes, List<long> unixtimes)
        {
            throw new NotImplementedException();
        }
    }
}
