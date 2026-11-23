using OpenDCP.Framework.Datasource.Mock.Generator.Analytical.Entities;
using OpenDCP.Framework.Datasource.Mock.Generator.Extensions;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using OpenDCP.Framework.DataTypes.Analytical.Entities.AnalyticalData;
using OpenDCP.Framework.DataTypes.Analytical.Requests.AnalyticalData;
using OpenDCP.Framework.DataTypes.Analytical.Responses.AnalyticalData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial class MockDatasource_v_1_0
    {

        public Task<AttributeDataResponse> GetAnalyticalDataAsync(AttributeDataQueryDTO model)
        {
            var result = new AttributeDataResponse
            {
                Attributes = new List<AnalyticalDataDTO>()
            };

            result.Attributes = GenerateAttributeValues(model.EventId, model.AttributeId);
            return Task.FromResult(result);
        }

        public Task<AttributeListResponse> GetAttributesAsync(string deviceWebId)
        {
            var result = new AttributeListResponse
            {
                Attributes = GenerateAttributes(deviceWebId).Select(x => new AttributeDTO
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Description = x.Description,
                    UoM = x.UoM
                }).ToList()
            };

            return Task.FromResult(result);
        }

        private List<AnalyticalAttribute> GenerateAttributes(string deviceWebId)
        {

            var systemSeed = IdUtils.GetSeed(new List<string> { deviceWebId, _configuration.SystemId, "analytical-attributes" });
            var r = new Random(systemSeed);

            int nbrAttributes = r.Next(0,_configuration.Settings.AnalyticalData.MaxAttributes);

            var attributes = new List<AnalyticalAttribute>();

            for (int i = 0; i < nbrAttributes; i++)
            {
                string id = IdUtils.GenerateGuid(r).ToString();
                attributes.Add(new AnalyticalAttribute(id, _configuration.Settings.AnalyticalData));
            }
            return attributes;
        }

        private List<AnalyticalDataDTO> GenerateAttributeValues(List<string> eventFrameIds, List<string> attributeIds)
        {
           
            var result = new List<AnalyticalDataDTO>();

            foreach (string attributeId in attributeIds)
            {
                var attribute = new AnalyticalAttribute(attributeId, _configuration.Settings.AnalyticalData);


                var item = new AnalyticalDataDTO
                {
                    AttributeId = attribute.Id.ToString(),
                    AttributeName = attribute.Name,
                    Values = new List<AnalyticalDataValueDTO>()
                };

                var valueFun = attribute.DataGeneratorFunction();

                foreach (string eventframeId in eventFrameIds)
                {
                    var ef = AssetHierarchy.FindEventFrameById(Guid.Parse(eventframeId));

                    AnalyticalDataValueDTO value;

                    if (ef != null)
                    {
                        var timeSeed = IdUtils.GetSeed(new List<string> { _configuration.SystemId, eventframeId });
                        var r2 = new Random(timeSeed);
                        var unixtime = r2.Next(946684800, 1751353200) / 2629800;

                        value = new AnalyticalDataValueDTO
                        {
                            Id = eventframeId,
                            Value = valueFun(unixtime),
                            ObsId = IdUtils.GenerateGuid(r2).ToString(),
                            Timestamp = HelperUtils.UnixTimeStampToDateTime(unixtime).ToIso8601String(),
                        };
                    }
                    else
                    {
                        value = new AnalyticalDataValueDTO
                        {
                            Id = eventframeId,
                            Value = float.NaN,
                            ObsId = eventframeId,
                            Timestamp = string.Empty,
                        };
                    }

                    item.Values.Add(value);
                }

                result.Add(item);
            }

            return result;

        }

    }
}
