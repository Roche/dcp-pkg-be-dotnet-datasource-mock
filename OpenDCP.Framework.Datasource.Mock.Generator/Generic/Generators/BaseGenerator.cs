using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using System;
using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Generic.Generators
{
    public class BaseGenerator
    {

        public BaseGenerator(AdminConfiguration configuration)
        {

        }
        public string GenerateId(int? seed = null)
        {

            string uuid = string.Empty;

            if (seed == null)
            {
                uuid = Guid.NewGuid().ToString();
            }
            else
            {
                var source = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                var r = new Random(seed.Value);


                for (int i = 0; i <= 30; i++)
                {
                    uuid += source[(int)(r.NextDouble() * 15)];
                }
                uuid = $"{uuid.Substring(0, 8)}-{uuid.Substring(8, 4)}-4{uuid.Substring(12, 3)}-{uuid.Substring(12, 3)}-{uuid.Substring(12, 3)}";
            }

            return uuid;
        }

        public Func<int, double> GetFunctionClass(int seed)
        {
            var r = new Random(seed);
            int functionClass = r.Next() % 3;

            if (functionClass == 0)
            {
                return x => System.Math.Sin(x) + r.NextDouble();
            }
            else if (functionClass == 1)
            {
                return x => x * x + r.NextDouble();
            }
            else
            {
                return x => System.Math.Exp(x) + r.NextDouble();
            }
        }

    }
}
