using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities
{
    public class AFEventframeTemplateAttribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string UoM { get; set; }
        public List<string> Categories { get; set; }
        public bool IsConfiguration { get; set; }
        public string Type { get; set; }
        public AFEventFrameTemplate Template { get; set; }
        public Func<long, double> NumericDataGeneratorFunction()
        {
            var r = new Random(IdUtils.GetSeed(new List<string> { Name }));

            Func<long, double> baseGenerator;

            switch (r.Next(1, 5))
            {
                case 1:
                    baseGenerator = x => System.Math.Sin(x) * 10;
                    break;
                case 2:
                    baseGenerator = x => System.Math.Cos(x) * 5;
                    break;
                case 3:
                    baseGenerator = x => System.Math.Tan(x);
                    break;
                default:
                    baseGenerator = x => x * x % 100;
                    break;
            }

            return x =>
            {
                if (x % r.Next(0, 100) * 100 == 0)
                {
                    return double.NaN;
                }
                else
                {
                    return baseGenerator(x);
                }
            };
        }
        public Func<long, string> StringDataGeneratorFunction()
        {
            return x =>
            {
                if (x % 18700 == 0)
                {
                    return double.NaN.ToString();
                }
                else
                {
                    return $"{Name} {x.ToString().Substring(x.ToString().Length - 6, 3)}";
                }
            };

        }

        public Func<long, string> StringCategoryDataGeneratorFunction()
        {
            if (Categories.Contains("constant"))
            {
                return x => Template.Name;
            }

            return x =>
            {
                var r = new Random(IdUtils.GetSeed(new List<string> { Name }));
                var nbrCategories = r.Next(1, 5);

                var r2 = new Random(IdUtils.GetSeed(new List<string> { Name, x.ToString() }));

                return $"{Name} {r2.Next(0, nbrCategories)}";

            };

        }
        public string TimeMethod { get; set; }
    }
}
