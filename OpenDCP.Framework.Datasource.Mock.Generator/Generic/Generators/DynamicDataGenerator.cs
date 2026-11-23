using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using OpenDCP.Framework.Datasource.Mock.Generator.Extensions;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Generic.Generators
{

    public class DynamicDataGenerator : DataGenerator
    {
        public readonly int _version;

        public DynamicDataGenerator(int version) : base(version)
        {
            _version = version;
        }

        public override List<AFAttributeValueSet> GetValues(AFHirachey hirachey, List<AFAttribute> attributes, List<long> unixtimes)
        {
            var currentDate = DateTime.UtcNow.ToUnixSeconds() * 1000;

            if (unixtimes == null || unixtimes.Count == 0)
                return new List<AFAttributeValueSet>();

            // Sort once
            unixtimes.Sort();

            var minTime = unixtimes[0];
            var maxTime = unixtimes[unixtimes.Count - 1];

            var attributeGroups = attributes.GroupBy(x => x.ElementId);
            var result = new List<AFAttributeValueSet>(attributes.Count);

            var isVersionOne = _version == 1;

            foreach (var attributePerElement in attributeGroups)
            {
                var elementId = attributePerElement.Key;

                var element = hirachey.FindNodeById(
                    hirachey.Elements,
                    WebIdEncoding.Id2NodeId(elementId));

                if (element == null)
                    throw new Exception("Reference Element not found");

                var eventframes = hirachey
                    .GetElementEventFrames(element, minTime, maxTime, hirachey.EventFrameTemplates)
                    .OrderBy(x => x.Start)
                    .ToList();

                var basePeriod = hirachey.GenerateAnalysis(element)
                    .Select(x => x.BasePeriod)
                    .FirstOrDefault();

                // Precompute active state using sweep (O(T + F))
                var isActive = new bool[unixtimes.Count];

                int f = 0;
                for (int i = 0; i < unixtimes.Count; i++)
                {
                    var t = unixtimes[i];

                    // advance eventframe pointer until it might contain t
                    while (f < eventframes.Count && eventframes[f].End < t)
                        f++;

                    if (f < eventframes.Count)
                    {
                        long startBound = isVersionOne ? eventframes[f].Start - 1 : eventframes[f].Start;

                        if (startBound <= t && t <= eventframes[f].End)
                        {
                            isActive[i] = true;
                        }
                    }
                }

                // Precompute per-timestamp periodNumber + runtime (O(T))
                var periodNumbers = new int[unixtimes.Count];
                var runtimes = new double[unixtimes.Count];

                for (int i = 0; i < unixtimes.Count; i++)
                {
                    var t = unixtimes[i];
                    long offset = -1;

                    if (isActive[i])
                    {
                        periodNumbers[i] = (int)((t + offset) / basePeriod);
                    }
                    else
                    {
                        periodNumbers[i] = (int)(t / basePeriod); // Old logic
                    }
                    runtimes[i] = ((t + offset) % basePeriod) / (double)basePeriod;
                }

                // Now build values per attribute (O(Ae * T))
                foreach (var attribute in attributePerElement)
                {
                    var itemToAdd = new AFAttributeValueSet
                    {
                        Attribute = attribute,
                        Values = new List<AFAttributeValue>(unixtimes.Count)
                    };

                    // Optional micro-opt: cache generators by periodNumber
                    var activeGenCache = new Dictionary<int, Func<double, long, double>>();
                    var inactiveGenCache = new Dictionary<int, Func<double, long, double>>();

                    for (int i = 0; i < unixtimes.Count; i++)
                    {
                        var periodNumber = periodNumbers[i];
                        Func<double, long, double> generator;

                        if (isActive[i])
                        {
                            if (!activeGenCache.TryGetValue(periodNumber, out generator))
                            {
                                generator = attribute.ActiveDataGeneratorFunction(periodNumber);
                                activeGenCache[periodNumber] = generator;
                            }
                        }
                        else
                        {
                            if (!inactiveGenCache.TryGetValue(periodNumber, out generator))
                            {
                                generator = attribute.NonActiveDataGeneratorFunction(periodNumber);
                                inactiveGenCache[periodNumber] = generator;
                            }
                        }

                        var value = new AFAttributeValue
                        {
                            Unixtime = unixtimes[i],
                            Value = currentDate < unixtimes[i] ? double.NaN : generator(runtimes[i], unixtimes[i]),
                        };

                        value.Good = !double.IsNaN(value.Value);
                        itemToAdd.Values.Add(value);
                    }

                    result.Add(itemToAdd);
                }
            }

            return result;
        }
    }
}
