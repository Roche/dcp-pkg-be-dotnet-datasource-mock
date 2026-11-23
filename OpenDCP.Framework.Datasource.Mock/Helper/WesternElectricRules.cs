using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Helper
{
    public static class WesternElectricRules
    {
        // Rule 1: One point outside control limits
        public static (bool, string?)[] R1(double[] x, double? ucl, double? lcl)
        {
            var result = new (bool, string?)[x.Length];

            if (!ucl.HasValue && !lcl.HasValue)
                return result;

            for (int i = 0; i < x.Length; i++)
            {
                if (ucl.HasValue && x[i] > ucl.Value)
                {
                    result[i] = (true, "Upper");
                    continue;
                }

                if (lcl.HasValue && x[i] < lcl.Value)
                {
                    result[i] = (true, "Lower");
                }
            }

            return result;
        }

        // Rule 2b: 8 consecutive points on the same side of center line
        public static (bool, string?)[] R2b(double[] x, double cl, double? lcl, double? ucl)
        {
            var result = new (bool, string?)[x.Length];
            int count = 0;

            int[] signs = x.Select(v => System.Math.Sign(v - cl)).ToArray();
            int runLength = 1;

            for (int i = 1; i <= signs.Length; i++)
            {
                if (i < signs.Length && signs[i] == signs[i - 1])
                {
                    runLength++;
                }
                else
                {
                    if (runLength >= 8 && signs[i - 1] != 0)
                    {
                        if ((signs[i - 1] > 0 && ucl.HasValue) || (signs[i - 1] < 0 && lcl.HasValue))
                        {
                            for (int j = count + 7; j < count + runLength; j++)
                                result[j] = (true, signs[i - 1] > 0 ? "Upper" : "Lower");
                        }
                    }
                    count += runLength;
                    runLength = 1;
                }
            }

            return result;
        }

        // Rule 2a: 4 out of 5 consecutive points beyond 1 SD on the same side
        public static (bool, string?)[] R2a(double[] x, double? lbl, double? ubl)
        {
            var result = new (bool, string?)[x.Length];
            if (x.Length < 5) return result;

            for (int i = 0; i <= x.Length - 5; i++)
            {
                if (ubl.HasValue)
                {
                    int count = 0;
                    for (int j = i; j < i + 5; j++)
                        if (x[j] > ubl.Value) count++;

                    if (count >= 4)
                    {
                        for (int j = i; j < i + 5; j++)
                            if (x[j] > ubl.Value) result[j] = (true, "Upper");
                    }
                }

                if (lbl.HasValue)
                {
                    int count = 0;
                    for (int j = i; j < i + 5; j++)
                        if (x[j] < lbl.Value) count++;

                    if (count >= 4)
                    {
                        for (int j = i; j < i + 5; j++)
                            if (x[j] < lbl.Value) result[j] = (true, "Lower");
                    }
                }
            }

            return result;
        }

        // Rule 3: 6 consecutive increasing or decreasing points
        public static (bool, string?)[] R3(double[] x)
        {
            var result = new (bool, string?)[x.Length];
            if (x.Length < 2) return result;

            int count = 0;
            int runLength = 1;
            int prevSign = System.Math.Sign(x[1] - x[0]);

            for (int i = 1; i < x.Length - 1; i++)
            {
                int currSign = System.Math.Sign(x[i + 1] - x[i]);

                if (currSign != 0 && currSign == prevSign)
                {
                    runLength++;
                }
                else
                {
                    if (runLength >= 5)
                    {
                        for (int j = count + 5; j <= count + runLength; j++)
                            result[j] = (true, prevSign > 0 ? "Increasing" : "Decreasing");
                    }
                    count += runLength;
                    runLength = 1;
                }

                prevSign = currSign;
            }

            // Check last run
            if (runLength >= 5)
            {
                for (int j = count + 5; j <= count + runLength; j++)
                    result[j] = (true, prevSign > 0 ? "Increasing" : "Decreasing");
            }

            return result;
        }
    }
}
