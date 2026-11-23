using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities
{
    // Sensor
    public class AFAttribute
    {
        public Guid Id { get; set; }
        public AdminConfiguration Configuration { get; set; }

        public string EntityVersion = "1.0";
        public AFAttribute(Guid id, Guid elementId, AdminConfiguration config)
        {
            Id = id;
            ElementId = elementId;
            Configuration = config;
        }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public Guid ElementId { get; set; }
        public virtual string ElementWebId
        {
            get
            {
                return "I1Em" + WebIdEncoding.EncodeId(new Guid(Configuration.SystemId)) + WebIdEncoding.EncodeId(ElementId);
            }
        }
        public List<string> Categories { get; set; } = new List<string>();
        public string UoM { get; set; }
        public virtual string Path
        {
            get
            {
                return $"{ElementId}|{Name}";
            }
        }
        public virtual string WebId
        {
            get
            {
                return "I1AbE" + WebIdEncoding.EncodeId(new Guid(Configuration.SystemId)) + WebIdEncoding.EncodeId(ElementId) + WebIdEncoding.EncodeId(Id);
            }
        }
        public bool IsConfiguration { get; set; }
        public bool IsStep { get; set; }
        public string Type { get; set; }
        public string ActiveDataGeneratorFunctionString(int period)
        {
            //Attribute specific seed
            var r = new Random(IdUtils.GetSeed(new List<string> { Id.ToString() }));

            //Set the range of function based on the version
            var mIndex = EntityVersion == "1.0" ? 7 : 10;

            switch (r.Next(1, mIndex))
            {
                case 1:
                    return "sin(x)";
                case 2:
                    return "exp(x)";
                case 3:
                    return "x";
                case 4:
                    return "1 / (1 - exp(x))";
                case 5:
                    return "rnorm";
                case 6:
                    return "x^2";
                case 7:
                    return "constant";
                case 8:
                    return "NaN";
                case 9:
                    return "x || NaN";
                default:
                    return "NA";
            }

        }
        public Func<double, long, double> ActiveDataGeneratorFunction(int period)
        {
            //Attribute specific seed
            var r = new Random(IdUtils.GetSeed(new List<string> { Id.ToString() }));

            //Period specifc seed
            var r2 = new Random(IdUtils.GetSeed(new List<string> { Id.ToString(), period.ToString() }));
            var periodVariation = r.Next(0, 10);

            if (Categories.Contains("realtime"))
            {
                periodVariation = 0;
            }

            var offset = ScaleToInterval(r.NextDouble(), -1.0, 5.0) * ScaleToInterval(r2.NextDouble(), (100 - periodVariation) / 100, (100 + periodVariation) / 100);      //Vertical Shift
            var shift = ScaleToInterval(r.NextDouble(), -0.2, 0.2) * ScaleToInterval(r2.NextDouble(), (100 - periodVariation) / 100, (100 + periodVariation) / 100);   //Horizontal Shift

            var xStretch = ScaleToInterval(r.NextDouble(), 0.2, 2) * ScaleToInterval(r2.NextDouble(), (100 - periodVariation) / 100, (100 + periodVariation) / 100); //Vertical Stretch/Compression
            var yStretch = ScaleToInterval(r.NextDouble(), 0.8, 2) * ScaleToInterval(r2.NextDouble(), (100 - periodVariation) / 100, (100 + periodVariation) / 100); //Horizontal Stretch/Compression

            Func<double, long, double> baseFunction = (x, xAbs) => x;

            // Check for some special functions based on the category
            if (Categories.Contains("Constant"))
            {
                return (x, xAbs) => 5;
            }
            else if (Categories.Contains("Maturity"))
            {
                return (x, xAbs) => x * 100;
            }

            //Set the range of function based on the version
            var mIndex = EntityVersion == "1.0" ? 7 : 10;

            switch (r.Next(1, mIndex))
            {
                case 1:
                    //Sin
                    baseFunction = (x, xAbs) => (System.Math.Sin(2 * x) + 1) * 400;
                    break;
                case 2:
                    //Exponential
                    baseFunction = (x, xAbs) => 5 * System.Math.Exp(2 * x);
                    break;
                case 3:
                    //Linear
                    baseFunction = (x, xAbs) => x * 113.2;
                    break;
                case 4:
                    //Sigmoid
                    baseFunction = (x, xAbs) => 30.0 / (1.0 + System.Math.Exp(-x * 5));
                    break;
                case 5:
                    //Noise
                    baseFunction = (x, xAbs) =>
                    {
                        var dr = new Random((int)(x * 100000));
                        return dr.NextDouble() + offset;
                    };
                    break;
                case 6:
                    //Qudratic
                    baseFunction = (x, xAbs) => x * x * 1000;
                    break;
                case 7:
                    //Constant
                    baseFunction = (x, xAbs) => r.Next(100);
                    break;
                case 8:
                    //Bad-always
                    baseFunction = (x, xAbs) => double.NaN;
                    break;
                case 9:
                    //Sometimes bad
                    if (Configuration.Version <= 2)
                    {
                        var val = r.NextDouble();
                        return (x, xAbs) =>
                        {
                            return x < val ? double.NaN : x * 113.2;
                        };
                    }
                    else
                    {
                        baseFunction = (x, xAbs) =>
                        {
                            return x % 137835 < 100 ? double.NaN : x * 113.2;
                        };
                        break;
                    }
            }

            return TransformFunction(baseFunction, yStretch, xStretch, shift, offset);

            throw new Exception("Something went wrong");
        }
        private double ScaleToInterval(double value, double min, double max)
        {
            return min + (max - min) * value;
        }
        private Func<double, long, double> TransformFunction(Func<double, long, double> f, double a, double b, double c, double d)
        {
            return (x, xAbs) => a * f(b * (x - c), xAbs) + d;
        }
        public Func<double, long, double> NonActiveDataGeneratorFunction(int period)
        {
            var r = new Random(IdUtils.GetSeed(new List<string> { Id.ToString() }));

            var offset = r.NextDouble() * 10;
            return (x, xAbs) => System.Math.Sin(x) + offset;

        }
    }
}