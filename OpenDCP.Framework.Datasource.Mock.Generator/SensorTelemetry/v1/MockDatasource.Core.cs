using OpenDCP.Framework.Configuration.Model.Errors;
using OpenDCP.Framework.Datasource.Mock.Generator.Constants;
using OpenDCP.Framework.Datasource.Mock.Generator.Extensions;
using OpenDCP.Framework.Datasource.Mock.Generator.Generic.Generators;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using OpenDCP.Framework.DataTypes.Equipment.Constants;
using OpenDCP.Framework.DataTypes.Equipment.Entities.SensorData;
using OpenDCP.Framework.DataTypes.Equipment.Requests.SensorData;
using OpenDCP.Framework.DataTypes.Equipment.Responses.SensorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial class MockDatasource_v_1_0
    {
        #region Data
        public Task<SensorDataResponse> InterpolatedDataAsync(InterpolatedDataQueryDTO model)
        {
            var result = new SensorDataResponse
            {
                Errors = new List<DatasourceError>()
            };

            TimeSpan intervalSpan = ParseIntervalToTimeSpan(model.Interval);

            var attributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, model.WebId);

            result.Errors = model.WebId.Except(attributes.Select(a => a.WebId)).Select(x => new DatasourceError
            {
                Error = Errors.SensorNotFound,
                Identifier = x,
                ErrorCode = (byte)ErrorCode.DeleteSensor
            }).ToList();

            var factory = new DataGeneratorFactoy();
            var unixtimes = Range(HelperUtils.ParseDateToUnix(model.StartTime.ToIso8601String()), HelperUtils.ParseDateToUnix(model.EndTime.ToIso8601String()), (int)intervalSpan.TotalMilliseconds).ToList();

            var hasRemainder = (model.EndTime - model.StartTime).TotalSeconds % intervalSpan.TotalSeconds != 0;

            if (model.BoundaryType == BoundaryType.Outside && !hasRemainder)
            {
                long outside = unixtimes.Last() + (long)intervalSpan.TotalMilliseconds;
                unixtimes.Add(outside);
            }

            if (model.BoundaryType == BoundaryType.Inside && hasRemainder)
            {
                unixtimes.RemoveAt(unixtimes.Count - 1);
            }

            var generator = factory.GetDataGenerator(attributes.FirstOrDefault());

            var data = generator.GetValues(AssetHierarchy, attributes, unixtimes);

            result.Data = data.Select(s => new SensorDataDTO
            {
                WebId = s.Attribute.WebId,
                Name = s.Attribute.Name,
                UoM = s.Attribute.UoM,
                DataPoints = s.Values.Select(v => new ValueDTO
                {
                    Good = v.Good,
                    Value = v.Value,
                    ObsId = HelperUtils.UnixTimeStampToDateTime(v.Unixtime)
                }).ToList()
            }).ToList();

            return Task.FromResult(result);
        }

        public static TimeSpan ParseIntervalToTimeSpan(string interval)
        {
            if (string.IsNullOrWhiteSpace(interval))
                throw new ArgumentException("Interval cannot be null or empty.", nameof(interval));

            interval = interval.Trim().ToLowerInvariant();

            if (interval.EndsWith("ms"))
            {
                if (double.TryParse(interval.Substring(0, interval.Length - 2), out double value))
                    return TimeSpan.FromMilliseconds(value);
            }
            else if (interval.EndsWith("mo"))
            {
                if (double.TryParse(interval.Substring(0, interval.Length - 2), out double value))
                    return TimeSpan.FromDays(value * 30);
            }
            else
            {
                char suffix = interval[interval.Length - 1];
                string numberPart = interval.Substring(0, interval.Length - 1);

                if (!double.TryParse(numberPart, out double value))
                    throw new ArgumentException($"Invalid numeric value in interval: {interval}");

                switch (suffix)
                {
                    case 's': return TimeSpan.FromSeconds(value);
                    case 'm': return TimeSpan.FromMinutes(value);
                    case 'h': return TimeSpan.FromHours(value);
                    case 'd': return TimeSpan.FromDays(value);
                    case 'w': return TimeSpan.FromDays(value * 7);
                }
            }

            throw new ArgumentException($"Invalid interval format: {interval}");
        }

        public Task<SensorDataResponse> InterpolatedAtTimeDataAsync(InterpolatedAtTimeDataQueryDTO model)
        {
            var result = new SensorDataResponse
            {
                Errors = new List<DatasourceError>()
            };

            var attributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, model.WebId);

            result.Errors = model.WebId.Except(attributes.Select(a => a.WebId)).Select(x => new DatasourceError
            {
                Error = Errors.SensorNotFound,
                Identifier = x
            }).ToList();

            var factory = new DataGeneratorFactoy();
            var unixtimes = model.Time.Select(t => HelperUtils.ParseDateToUnix(t.ToIso8601String())).ToList();

            var generator = factory.GetDataGenerator(attributes.FirstOrDefault());

            var data = generator.GetValues(AssetHierarchy, attributes, unixtimes);

            result.Data = data.Select(s => new SensorDataDTO
            {
                WebId = s.Attribute.WebId,
                Name = s.Attribute.Name,
                UoM = s.Attribute.UoM,
                DataPoints = s.Values.Select(v => new ValueDTO
                {
                    Good = v.Good,
                    Value = v.Value,
                    ObsId = HelperUtils.UnixTimeStampToDateTime(v.Unixtime)
                }).ToList()
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<SensorDataResponse> PlotDataAsync(PlotDataQueryDTO model)
        {
            var result = new SensorDataResponse
            {
                Errors = new List<DatasourceError>()
            };

            var attributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, model.WebId);

            result.Errors = model.WebId.Except(attributes.Select(a => a.WebId)).Select(x => new DatasourceError
            {
                Error = Errors.SensorNotFound,
                Identifier = x,
                ErrorCode = (byte)ErrorCode.DeleteSensor
            }).ToList();

            if (model.StartTime == null)
            {
                model.StartTime = DateTime.UtcNow;
            }

            if (model.EndTime == null)
            {
                model.EndTime = DateTime.UtcNow;
            }

            var factory = new DataGeneratorFactoy();
            var stepSize = (int)(HelperUtils.ParseDateToUnix(model.EndTime.ToIso8601String()) - HelperUtils.ParseDateToUnix(model.StartTime.ToIso8601String())) / 1000 / (double)model.Intervals * 1000;

            //Minimum step-size is 1s
            if ((int)stepSize == 0)
            {
                stepSize = 1000;
            }

            var unixtimes = Range(HelperUtils.ParseDateToUnix(model.StartTime.ToIso8601String()), HelperUtils.ParseDateToUnix(model.EndTime.ToIso8601String()), (int)stepSize).ToList();

            var generator = factory.GetDataGenerator(attributes.FirstOrDefault());

            var data = generator.GetValues(AssetHierarchy, attributes, unixtimes);

            result.Data = data.Select(s => new SensorDataDTO
            {
                WebId = s.Attribute.WebId,
                UoM = s.Attribute.UoM,
                Name = s.Attribute.Name,
                DataPoints = DataMethodFilters.PlotDataFilter(s.Values, model.Intervals).Select(v => new ValueDTO
                {
                    Good = v.Good,
                    Value = v.Value,
                    ObsId = HelperUtils.UnixTimeStampToDateTime(v.Unixtime)
                }).ToList()
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<SensorDataResponse> RecordedDataAsync(RecordedDataQueryDTO model)
        {
            var result = new SensorDataResponse
            {
                Errors = new List<DatasourceError>()
            };

            var attributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, model.WebId);

            result.Errors = model.WebId
                .Except(attributes.Select(a => a.WebId))
                .Select(x => new DatasourceError
                {
                    Error = Errors.SensorNotFound,
                    Identifier = x,
                    ErrorCode = (byte)ErrorCode.DeleteSensor
                })
                .ToList();

            var factory = new DataGeneratorFactoy();

            var unixtimes = Range(
                min: HelperUtils.ParseDateToUnix(model.StartTime.ToIso8601String()),
                max: HelperUtils.ParseDateToUnix(model.EndTime.ToIso8601String()),
                step: 60000
            ).ToList();

            var generator = factory.GetDataGenerator(attributes.FirstOrDefault());

            var data = generator.GetValues(AssetHierarchy, attributes, unixtimes.Take(model.MaxCount * 5).ToList());

            result.Data = data.Select(s => new SensorDataDTO
            {
                WebId = s.Attribute.WebId,
                DataPoints = DataMethodFilters.RecordedDataFilter(s.Values, 0.01, 3600)
                    .Take(model.MaxCount)
                    .Select(v => new ValueDTO
                    {
                        Good = v.Good,
                        Value = v.Value,
                        ObsId = HelperUtils.UnixTimeStampToDateTime(v.Unixtime)
                    })
                .ToList()
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<SensorDataResponse> RecordedAtTimesDataAsync(RecordedAtTimeDataQueryDTO model)
        {
            var result = new SensorDataResponse
            {
                Errors = new List<DatasourceError>()
            };

            var attributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, model.WebId);

            result.Errors = model.WebId.Except(attributes.Select(a => a.WebId)).Select(x => new DatasourceError
            {
                Error = Errors.SensorNotFound,
                Identifier = x,
                ErrorCode = (byte)ErrorCode.DeleteSensor
            }).ToList();

            var factory = new DataGeneratorFactoy();
            var unixtimes = model.Time.Select(x => HelperUtils.ParseDateToUnix(x.ToIso8601String())).ToList();

            var generator = factory.GetDataGenerator(attributes.FirstOrDefault());

            var data = generator.GetValues(AssetHierarchy, attributes, unixtimes);

            result.Data = data.Select(s => new SensorDataDTO
            {
                WebId = s.Attribute.WebId,
                Name = s.Attribute.Name,
                UoM = s.Attribute.UoM,
                DataPoints = s.Values.Select(v => new ValueDTO
                {
                    Good = v.Good,
                    Value = v.Value,
                    ObsId = HelperUtils.UnixTimeStampToDateTime(v.Unixtime)
                }).ToList()
            }).ToList();

            return Task.FromResult(result);
        }
        #endregion
        private static IEnumerable<long> Range(long min, long max, int step)
        {
            if (min == max)
            {
                yield return min;

                yield break;
            }

            if (step == 0)
            {
                throw new Exception("Step point cannot be calculated");
            }

            for (long i = min; i <= max; i = checked(i + step))
            {
                yield return i;
            }
            ;

            if ((max - min) % step != 0)
            {
                yield return max;
            }
        }

    }
}
