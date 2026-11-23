using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Generic.Generators
{
    public class DataGeneratorFactoy
    {
        public DataGenerator GetDataGenerator(AFAttribute attribute)
        {
            int version = attribute?.Configuration?.Version ?? 0;

            return new DynamicDataGenerator(version);
        }
    }
}
