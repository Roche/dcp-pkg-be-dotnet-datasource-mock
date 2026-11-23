using MathNet.Numerics.Statistics;
using OpenDCP.Framework.Configuration.Model.Errors;
using OpenDCP.Framework.Datasource.Mock.Generator.Constants;
using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using OpenDCP.Framework.Datasource.Mock.Generator.Generic.Generators;
using OpenDCP.Framework.Datasource.Mock.Generator.Helpers;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
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
        public Task<SensorDataChunksResponse> GetRecordedDataChunksAsync(RecordedDataChunksRequest model)
        {

            var result = new SensorDataChunksResponse
            {
                Errors = new List<DatasourceError>(),
                Chunks = new List<SensorDataChunkDTO>()
            };

            var factory = new DataGeneratorFactoy();

            foreach (var chunk in model.Chunks)
            {
                var attributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, chunk.WebId);

                result.Errors = chunk.WebId.Except(attributes.Select(a => a.WebId)).Select(x => new DatasourceError
                {
                    Error = Errors.SensorNotFound,
                    Identifier = x
                }).ToList();

                var generator = factory.GetDataGenerator(attributes.FirstOrDefault());

                var unixtimes = Range(new DateTimeOffset(chunk.StartTime).ToUnixTimeMilliseconds(), new DateTimeOffset(chunk.EndTime).ToUnixTimeMilliseconds(), 60000).ToList();
                var data = generator.GetValues(AssetHierarchy, attributes, unixtimes);

                var chunkResponse = new SensorDataChunkDTO
                {
                    ChunkId = chunk.ChunkId,
                    ChunkData = data.Select(a =>
                    new SensorDataDTO
                    {
                        WebId = a.Attribute.WebId,
                        Name = a.Attribute.Name,
                        UoM = a.Attribute.UoM,
                        DataPoints = a.Values.Select(dp => new ValueDTO
                        {
                            ObsId = HelperUtils.UnixTimeStampToDateTime(dp.Unixtime),
                            Good = dp.Good,
                            Value = dp.Value,
                        }).ToList()
                    }).ToList()
                };

                result.Chunks.Add(chunkResponse);
            }

            return Task.FromResult(result);
        }

        public Task<SensorDataChunksResponse> GetInterpolatedDataChunksAsync(InterpolatedDataChunksRequest model)
        {
            var result = new SensorDataChunksResponse
            {
                Errors = new List<DatasourceError>(),
                Chunks = new List<SensorDataChunkDTO>()
            };

            var factory = new DataGeneratorFactoy();

            foreach (var chunk in model.Chunks)
            {
                var attributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, chunk.WebId);

                result.Errors = chunk.WebId.Except(attributes.Select(a => a.WebId)).Select(x => new DatasourceError
                {
                    Error = Errors.SensorNotFound,
                    Identifier = x
                }).ToList();

                var generator = factory.GetDataGenerator(attributes.FirstOrDefault());

                var unixtimes = Range(new DateTimeOffset(chunk.StartTime).ToUnixTimeMilliseconds(), new DateTimeOffset(chunk.EndTime).ToUnixTimeMilliseconds(), (int)DateTimeHelper.ParseIntervalToTimeSpan(chunk.Interval).TotalMilliseconds).ToList();
                var data = generator.GetValues(AssetHierarchy, attributes, unixtimes);

                var chunkResponse = new SensorDataChunkDTO
                {
                    ChunkId = chunk.ChunkId,
                    ChunkData = data.Select(a =>
                    new SensorDataDTO
                    {
                        WebId = a.Attribute.WebId,
                        Name = a.Attribute.Name,
                        UoM = a.Attribute.UoM,
                        DataPoints = a.Values.Select(dp => new ValueDTO
                        {
                            ObsId = HelperUtils.UnixTimeStampToDateTime(dp.Unixtime),
                            Good = dp.Good,
                            Value = dp.Value,
                        }).ToList()
                    }).ToList()
                };

                result.Chunks.Add(chunkResponse);
            }

            return Task.FromResult(result);
        }

        public Task<SensorDataSummaryStatisticsChunksResponse> GetSummaryDataChunksAsync(SummaryDataChunksRequest model)
        {

            var result = new SensorDataSummaryStatisticsChunksResponse
            {
                Errors = new List<DatasourceError>(),
                Chunks = new List<SensorSummaryDataChunkDTO>()
            };

            var factory = new DataGeneratorFactoy();

            foreach (var chunk in model.Chunks)
            {
                var attributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, chunk.WebId);

                result.Errors = chunk.WebId.Except(attributes.Select(a => a.WebId)).Select(x => new DatasourceError
                {
                    Error = Errors.SensorNotFound,
                    Identifier = x
                }).ToList();

                var generator = factory.GetDataGenerator(attributes.FirstOrDefault());

                var unixtimes = Range(new DateTimeOffset(chunk.StartTime).ToUnixTimeMilliseconds(), new DateTimeOffset(chunk.EndTime).ToUnixTimeMilliseconds(), 60000).ToList();
                var data = generator.GetValues(AssetHierarchy, attributes, unixtimes);

                var chunkResponse = new SensorSummaryDataChunkDTO
                {
                    ChunkId = chunk.ChunkId,
                    ChunkData = data.Select(a =>
                    new SensorSummaryDataDTO
                    {
                        WebId = a.Attribute.WebId,
                        Name = a.Attribute.Name,
                        UoM = a.Attribute.UoM,
                        DataPoints = CalculateStatistics(a.Values, chunk.SummaryTypes)
                    }).ToList()
                };

                result.Chunks.Add(chunkResponse);
            }

            return Task.FromResult(result);
        }

        private List<SensorStatisticValueDTO> CalculateStatistics(List<AFAttributeValue> points, List<string> summaryTypes)
        {
            var result = new List<SensorStatisticValueDTO>();

            foreach (var summaryType in summaryTypes)
            {
                if (summaryType == "Total")
                {
                    var value = points.Zip(
                       points.Skip(1),
                       (prev, current) => (current.Unixtime - prev.Unixtime) / 3600000.0 * prev.Value).Sum();

                    result.Add(new SensorStatisticValueDTO
                    {
                        Value = value,
                        ObsId = HelperUtils.UnixTimeStampToDateTime(points.LastOrDefault().Unixtime),
                        SummaryType = summaryType,
                        Good = true,
                    });
                }
                else if (summaryType == "Average")
                {
                    result.Add(new SensorStatisticValueDTO
                    {
                        Value = points.Select(x => x.Value).Average(),
                        ObsId = HelperUtils.UnixTimeStampToDateTime(points.LastOrDefault().Unixtime),
                        SummaryType = summaryType,
                        Good = true,
                    });
                }
                else if (summaryType == "Minimum")
                {
                    var point = points.OrderBy(x => x.Value).FirstOrDefault();
                    result.Add(new SensorStatisticValueDTO
                    {
                        Value = point.Value,
                        ObsId = HelperUtils.UnixTimeStampToDateTime(point.Unixtime),
                        SummaryType = summaryType,
                        Good = point.Good,
                    });
                }
                else if (summaryType == "Maximum")
                {
                    var point = points.OrderByDescending(x => x.Value).FirstOrDefault();
                    result.Add(new SensorStatisticValueDTO
                    {
                        Value = point.Value,
                        ObsId = HelperUtils.UnixTimeStampToDateTime(point.Unixtime),
                        SummaryType = summaryType,
                        Good = point.Good,
                    });
                }
                else if (summaryType == "Range")
                {
                    var min = points
                        .Where(x => x.Good)
                        .OrderBy(x => x.Value)
                        .Select(x => x.Value)
                        .FirstOrDefault(double.NaN)
                        ;
                    var max = points.Where(x => x.Good)
                        .OrderByDescending(x => x.Value)
                        .Select(x => x.Value)
                        .FirstOrDefault(double.NaN);

                    result.Add(new SensorStatisticValueDTO
                    {
                        Value = max - min,
                        ObsId = HelperUtils.UnixTimeStampToDateTime(points.LastOrDefault().Unixtime),
                        SummaryType = summaryType,
                        Good = true,
                    });
                }
                else if (summaryType == "StdDev")
                {

                    result.Add(new SensorStatisticValueDTO
                    {
                        Value = points.Select(x => x.Value).MeanStandardDeviation().StandardDeviation,
                        ObsId = HelperUtils.UnixTimeStampToDateTime(points.LastOrDefault().Unixtime),
                        SummaryType = summaryType,
                        Good = true,
                    });
                }
                else if (summaryType == "PopulationStdDev")
                {
                    result.Add(new SensorStatisticValueDTO
                    {
                        Value = points.Select(x => x.Value).MeanStandardDeviation().StandardDeviation,
                        ObsId = HelperUtils.UnixTimeStampToDateTime(points.LastOrDefault().Unixtime),
                        SummaryType = summaryType,
                        Good = true,
                    });
                }
                else if (summaryType == "Count")
                {

                    result.Add(new SensorStatisticValueDTO
                    {
                        Value = points.Count,
                        ObsId = HelperUtils.UnixTimeStampToDateTime(points.LastOrDefault().Unixtime),
                        SummaryType = summaryType,
                        Good = true,
                    });
                }
                else if (summaryType == "PercentGood")
                {

                    result.Add(new SensorStatisticValueDTO
                    {
                        Value = points.Where(x => x.Good).Count() / points.Count,
                        ObsId = HelperUtils.UnixTimeStampToDateTime(points.LastOrDefault().Unixtime),
                        SummaryType = summaryType,
                        Good = true,
                    });
                }
                else
                {
                    throw new Exception("Not implemented summary Type");
                }
            }

            return result;
        }
    }
}
