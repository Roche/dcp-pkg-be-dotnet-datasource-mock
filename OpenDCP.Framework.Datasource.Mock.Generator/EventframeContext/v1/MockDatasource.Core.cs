using OpenDCP.Framework.Configuration.Model.Errors;
using OpenDCP.Framework.Datasource.Mock.Generator.Constants;
using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using OpenDCP.Framework.Datasource.Mock.Generator.EventframeContext.Helpers;
using OpenDCP.Framework.Datasource.Mock.Generator.Extensions;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using OpenDCP.Framework.DataTypes.Equipment.Constants;
using OpenDCP.Framework.DataTypes.Equipment.Entities.EventframeContext;
using OpenDCP.Framework.DataTypes.Equipment.Requests.EventFrameContext;
using OpenDCP.Framework.DataTypes.Equipment.Responses.EventFrameContext;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial class MockDatasource_v_1_0
    {
        public Task<EventFrameResponse> GetEventframeAsync(string id)
        {
            var result = new EventFrameResponse
            {
                Eventframe = new EventFrameDTO()
            };

            var ef = AssetHierarchy.FindEventFrameById(Guid.Parse(id));

            if (ef == null)
            {
                result.Errors.Add(new DatasourceError
                {
                    Error = Errors.EventframeNotFound,
                    Identifier = id,
                    ErrorCode = (byte)ErrorCode.DeletedEventFrameWebId
                });
                return Task.FromResult(result);
            }

            result.Eventframe = new EventFrameDTO
            {
                Id = ef.Id.ToString(),
                StartTime = ef.StartTime.ToIso8601String(),
                EndTime = ef.EndTime.ToIso8601String(),
                RefWebId = ef.RefElementWebId,
                Values = ef.Values
            };

            return Task.FromResult(result);
        }

        public string ExtractDisplayEventId(List<EventFrameValueDTO> values)
        {
            var val = values.FirstOrDefault(v => v.Name == "BatchID");

            if (val != null)
            {
                if (val.Good)
                {
                    return val.Value;
                }
                else
                {
                    return "BadValue";
                }
            }

            return DateTime.UtcNow.ToString();
        }

        public Task<EventframeListResponse> SearchEventframeContextByIdAsync(EventframeContextIdQueryDTO model)
        {
            var result = new EventframeListResponse
            {
                Eventframes = new List<EventFrameDTO>(),
                Attributes = new List<EventFrameAttributeDTO>(),
            };

            var efList = new List<AFEventFrame>();

            foreach (var id in model.Id)
            {
                var ef = AssetHierarchy.FindEventFrameById(Guid.Parse(id));

                if (ef == null)
                {
                    result.Errors.Add(new DatasourceError
                    {
                        Error = Errors.ElementNotFound,
                        Identifier = id,
                        ErrorCode = (byte)ErrorCode.DeleteDevice
                    });
                    continue;
                }

                efList.Add(ef);

                result.Eventframes.Add(new EventFrameDTO
                {
                    Id = ef.Id.ToString(),
                    StartTime = ef.StartTime.ToIso8601String(),
                    EndTime = ef.EndTime.ToIso8601String(),
                    RefWebId = ef.RefElementWebId,
                    Values = ef.Values
                });
            }

            foreach (var webId in model.WebId)
            {
                var ef = AssetHierarchy.FindEventFrameById(WebIdEncoding.WebId2Id(webId));

                if (ef == null)
                {
                    result.Errors.Add(new DatasourceError
                    {
                        Error = Errors.ElementNotFound,
                        Identifier = webId,
                        ErrorCode = (byte)ErrorCode.DeleteDevice
                    });
                    continue;
                }

                efList.Add(ef);

                result.Eventframes.Add(new EventFrameDTO
                {
                    Id = ef.Id.ToString(),
                    StartTime = ef.StartTime.ToIso8601String(),
                    EndTime = ef.EndTime.ToIso8601String(),
                    RefWebId = ef.RefElementWebId,
                    Values = ef.Values
                });
            }

            var templates = efList.Select(x => x.Template).Distinct();
            var templateAttributes = new List<AFEventframeTemplateAttribute>();

            foreach (var tempalte in templates)
            {
                templateAttributes.AddRange(tempalte.GetAttributeTempaltes());
            }

            result.Attributes = templateAttributes.Select(at => new EventFrameAttributeDTO
            {
                Description = at.Description,
                Name = at.Name,
                ValueType = at.Type,
                UoM = at.UoM,
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<EventframeAttributesResponse> GetDeviceEventFrameAttributeAsync(string deviceWebId)
        {
            var result = new EventframeAttributesResponse
            {
                Errors = new List<DatasourceError>()
            };

            var deviceId = WebIdEncoding.WebId2NodeId(deviceWebId);
            var element = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, deviceId);

            if (element == null)
            {
                result.Errors.Add(new DatasourceError
                {
                    Error = Errors.ElementNotFound,
                    Identifier = deviceWebId,
                    ErrorCode = (byte)ErrorCode.DeleteDevice
                });
            }
            else
            {
                var eventframes = AssetHierarchy.GetElementEventFrames(element,
                    DateTimeOffset.UtcNow.AddDays(-360).ToUnixTimeMilliseconds(),
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    AssetHierarchy.EventFrameTemplates);

                var templates = eventframes.Select(x => x.Template).Distinct();
                var templateAttributes = new List<AFEventframeTemplateAttribute>();

                foreach (var tempalte in templates)
                {
                    templateAttributes.AddRange(tempalte.GetAttributeTempaltes());
                }

                result.Attributes = templateAttributes.Select(at => new EventFrameAttributeDTO
                {
                    Description = at.Description,
                    Name = at.Name,
                    ValueType = at.Type,
                }).ToList();
            }

            return Task.FromResult(result);
        }

        public Task<EventframeTemplateListResponse> GetEventframeTemplatesAsync(List<string> webIds)
        {
            var result = new EventframeTemplateListResponse
            {
                Errors = new List<DatasourceError>(),
                Templates = new List<EventframeTemplateDetailsDTO>()
            };

            var eventframeTemplates = AssetHierarchy.EventFrameTemplates;

            foreach (var webId in webIds)
            {
                var templates = eventframeTemplates.Where(t => t.WebId == webId);

                if (templates.Count() == 0)
                {
                    result.Errors.Add(new DatasourceError
                    {
                        Error = Errors.EventframeNotFound,
                        Identifier = webId,
                        ErrorCode = (byte)ErrorCode.DeletedTemplate
                    });
                }
                else
                {

                    foreach (var template in templates)
                    {
                        result.Templates.Add(new EventframeTemplateDetailsDTO
                        {
                            Id = template.Id,
                            WebId = template.WebId,
                            Name = template.Name,
                            Attributes = template.GetAttributeTempaltes().Select(at => new EventFrameAttributeDTO
                            {
                                Description = at.Description,
                                Name = at.Name,
                                ValueType = at.Type,
                                UoM = at.UoM,
                            }).ToList()
                        });
                    }
                }
            }

            return Task.FromResult(result);
        }

        public Task<EventframeListResponse> GetDeviceEventFramesAsync(EventframeContextTimeQueryDTO model)
        {
            var response = new EventframeListResponse
            {
                Errors = new List<DatasourceError>(),
                Eventframes = new List<EventFrameDTO>(),
                Attributes = new List<EventFrameAttributeDTO>()
            };

            var templateAttributes = new List<AFEventframeTemplateAttribute>();

            foreach (var deviceWebId in model.DeviceWebId)
            {

                var deviceId = WebIdEncoding.WebId2NodeId(deviceWebId);
                var element = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, deviceId);

                if (element == null)
                {
                    response.Errors.Add(new DatasourceError
                    {
                        Error = Errors.ElementNotFound,
                        Identifier = deviceWebId
                    });
                }
                else
                {
                    var eventframes = AssetHierarchy.GetElementEventFrames(element,
                        HelperUtils.ParseDateToUnix(model.StartTime.ToIso8601String()),
                        HelperUtils.ParseDateToUnix(model.EndTime.ToIso8601String()),
                        AssetHierarchy.EventFrameTemplates);

                    var filteredEventframes = eventframes.FilterBySearchMode(model.StartTime, model.EndTime, model.SearchMode);

                    response.Eventframes.AddRange(filteredEventframes.Select(x => new EventFrameDTO
                    {
                        Id = x.Id.ToString(),
                        RefWebId = x.RefElementWebId,
                        StartTime = x.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                        EndTime = x.EndTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                        Values = x.Values.Select(v => new EventFrameValueDTO
                        {
                            Name = v.Name,
                            Good = v.Good,
                            Value = (object)v.Value
                        }).Distinct().ToList()
                    }).Distinct().ToList());

                    var templates = filteredEventframes.Select(x => x.Template).Distinct();

                    foreach (var tempalte in templates)
                    {
                        templateAttributes.AddRange(tempalte.GetAttributeTempaltes());
                    }
                }

                if (model.IncludeAttributeSettings)
                {
                    response.Attributes = templateAttributes
                    .GroupBy(x => x.Name)
                    .Select(x => new EventFrameAttributeDTO
                    {
                        Description = x.Select(e => e.UoM).FirstOrDefault(),
                        UoM = x.Select(e => e.UoM).FirstOrDefault(),
                        Name = x.Key,
                        //ToDo change to data Type promoting
                        ValueType = x.Select(e => e.Type).FirstOrDefault()
                    })
                    .ToList();
                }
            }

            response.Eventframes = response.Eventframes
                .OrderBy(x => x.StartTime)
                .ToList();

            return Task.FromResult(response);
        }

        public Task<BatchContextListResponse> SearchBatchContextByTimeAsync(BatchContextTimeQueryDTO model)
        {
            var response = new BatchContextListResponse
            {
                Errors = new List<DatasourceError>()
            };

            foreach (var webId in model.DeviceWebId)
            {
                var deviceId = WebIdEncoding.WebId2NodeId(webId);
                var element = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, deviceId);

                if (element == null)
                {
                    response.Errors.Add(new DatasourceError
                    {
                        Error = Errors.ElementNotFound,
                        Identifier = webId,
                        ErrorCode = (byte)ErrorCode.DeleteDevice
                    });
                }
                else
                {
                    var eventframes = AssetHierarchy.GetElementEventFrames(element,
                        HelperUtils.ParseDateToUnix(model.StartTime.ToIso8601String()),
                        HelperUtils.ParseDateToUnix(model.EndTime.ToIso8601String()),
                        AssetHierarchy.EventFrameTemplates);

                    var filteredEventframes = eventframes.FilterBySearchMode(model.StartTime, model.EndTime, model.SearchMode);

                    response.Eventframes.AddRange(filteredEventframes.Select(x => new BatchContextDTO
                    {
                        Id = x.Id.ToString(),
                        StartTime = x.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                        EndTime = x.EndTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                        DeviceWebId = x.RefElementWebId,
                    }).ToList());
                }
            }

            return Task.FromResult(response);
        }

        public Task<EventframeValuesResponse> GetEventFrameValuesAsync(string id)
        {
            var result = new EventframeValuesResponse
            {
                Errors = new List<DatasourceError>()
            };

            var eventframe = AssetHierarchy.FindEventFrameById(Guid.Parse(id));

            if (eventframe == null)
            {
                result.Errors.Add(new DatasourceError
                {
                    Error = Errors.EventframeNotFound,
                    Identifier = id,
                    ErrorCode = (byte)ErrorCode.DeletedEventFrameWebId
                });
            }
            else
            {
                result.Values = eventframe.Values.Select(v => new EventFrameValueDTO
                {
                    Good = v.Good,
                    Value = v.Value,
                    Name = v.Name,
                }).ToList();
            }

            return Task.FromResult(result);
        }
    }
}
