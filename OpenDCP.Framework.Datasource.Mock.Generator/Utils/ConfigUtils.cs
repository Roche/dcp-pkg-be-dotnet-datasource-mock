using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Utils
{
    public static class ConfigUtils
    {
        public static int SampleWithWeights(List<int> values, List<double> weights, Random r)
        {
            if (values.Count != weights.Count)
            {
                throw new Exception("Inputs need to have equal length");
            }
            var val = r.NextDouble();
            var total = weights.Sum();
            var threshold = 0.0;

            for (var i = 0; i <= values.Count; i++)
            {
                threshold += weights[i] / total;

                if (val < threshold)
                {
                    return values[i];
                }
            }

            throw new Exception("Some processing error");

        }

        public static string SampleWithWeights(List<string> values, List<double> weights, Random r)
        {
            if (values.Count != weights.Count)
            {
                throw new Exception("Inputs need to have equal length");
            }
            var val = r.NextDouble();
            var total = weights.Sum();
            var threshold = 0.0;

            for (var i = 0; i <= values.Count; i++)
            {
                threshold += weights[i] / total;

                if (val < threshold)
                {
                    return values[i];
                }
            }

            throw new Exception("Some processing error");

        }
    }
}
