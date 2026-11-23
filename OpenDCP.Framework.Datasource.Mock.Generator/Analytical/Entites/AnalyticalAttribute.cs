using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Analytical.Entities
{
    public class AnalyticalAttribute
    {
        private readonly AnalyticalDataConfiguration _configuration;
        private Random _random;

        public AnalyticalAttribute(string id, AnalyticalDataConfiguration config)
        {
            _configuration = config;
            Id = id;
            _random = new Random(IdUtils.GetSeed(new List<string> { id.ToString(), "analytical-attribute" }));
        }
        public string Id { get; private set; }
        public virtual string UoM
        {
            get
            {
                return _configuration.UoM[_random.Next(0, _configuration.UoM.Count - 1)];
            }
        }
        public virtual string Name
        {
            get
            {
                return $"{_configuration.AttributeNames[_random.Next(0, _configuration.AttributeNames.Count - 1)]} {Regex.Replace(Id, @"[^\d]", "").Substring(0, 3)}";
            }
        }

        public virtual string Description
        {
            get
            {
                return $"Description - {Name}";
            }
        }
        public Func<int, double> DataGeneratorFunction()
        {
            var r = new Random(IdUtils.GetSeed(new List<string> { Id.ToString() }));

            switch (r.Next(1, 7))
            {
                case 1:
                    return (x) => System.Math.Sin(x) * 0.001;
                case 2:
                    return (x) => System.Math.Exp(0.001 * x);
                case 3:
                    //Linear
                    return (x) => x + 0.01;
                case 4:
                    //Sigmoid
                    return (x) => 1.0f / (1.0f + (float)System.Math.Exp(-x));
                case 5:
                    //Noise
                    return (x) =>
                    {
                        var dr = new Random(x);
                        return dr.NextDouble();
                    };
                case 6:
                    //qudratic
                    return (x) => x * x - 12;
                case 7:
                    //Constant
                    return (x) => r.Next(100);
            }

            throw new Exception("Something went wrong");
        }
    }
}
