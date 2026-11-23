using System;
using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Helpers
{
    public static class GenerateListHelper
    {
        public static List<string> GenerateSyntheticList(Random random, string prefix, int count)
        {
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                int randomSuffix = random.Next(10000, 99999);
                list.Add($"{prefix}_{randomSuffix}");
            }
            return list;
        }
    }
}
