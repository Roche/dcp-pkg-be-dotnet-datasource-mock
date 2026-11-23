using OpenDCP.Framework.Configuration.Model.Errors;
using OpenDCP.Framework.Configuration.Model.Warnings;
using OpenDCP.Framework.Datasource.Mock.Generator.AssetHierarchy.Generators;
using OpenDCP.Framework.Datasource.Mock.Generator.Constants;
using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using OpenDCP.Framework.DataTypes.Equipment.Constants;
using OpenDCP.Framework.DataTypes.Equipment.Entities.AssetHierarchy;
using OpenDCP.Framework.DataTypes.Equipment.Requests.AssetHierarchy;
using OpenDCP.Framework.DataTypes.Equipment.Responses.AssetHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial class MockDatasource_v_1_0
    {
        public AFHirachey AssetHierarchy { get; set; }

        partial void InitializeEquipment()
        {
            if (_configuration.Settings != null
                && _configuration.Settings.AssetHierarchy != null
                && _configuration.Settings.EventframeContext != null)
            {
                AssetHierarchy = new AFHirachey(_configuration);
            }
        }

        public Task<DatasourceUnitResponse> GetUnitsOfMeasureAsync()
        {
            var res = new DatasourceUnitResponse
            {
                Units = AssetHierarchy.Configuration.Settings.AssetHierarchy.UoM.Select(uom =>
                    new DatasourceUnit
                    {
                        Name = $"{uom}-name",
                        Abbreviation = uom,
                        Class = $"{uom}-class"
                    }).ToList()
            };

            return Task.FromResult(res);
        }

        public Task<GlobalFilterListResponse> GetGlobalFilterAsync(string link)
        {
            var result = new GlobalFilterListResponse
            {
                Elements = new List<GlobalFilterElementDTO>(),
                Errors = new List<DatasourceError>()
            };

            string nodeIdToFind;

            if (string.IsNullOrEmpty(link))
            {
                nodeIdToFind = AssetHierarchy.Elements.NodeId;
            }
            else
            {
                nodeIdToFind = WebIdEncoding.WebId2NodeId(link.Split('/')[0]);
            }

            var nodeToFind = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, nodeIdToFind.ToUpper());

            if (nodeIdToFind == null)
            {
                result.Errors.Add(
                    new DatasourceError
                    {
                        Error = Errors.ElementNotFound,
                    });

                return Task.FromResult(result);
            }

            if (string.IsNullOrEmpty(link) && AssetHierarchy.Configuration.Version > 1)
            {
                result.Elements.Add(
                    new GlobalFilterElementDTO
                    {
                        Name = nodeToFind.Name,
                        CategoryNames = new List<string> { },
                        IsChromUnit = false,
                        WebId = nodeToFind.WebId,
                        IsDevice = nodeToFind.IsDevice,
                        Description = nodeToFind.Description,
                        Link = $"{nodeToFind.WebId}{(nodeToFind.IsDevice ? "/sensor" : "/level")}",
                        UoM = null,
                        Step = false,
                        ValueType = null
                    }
                );
            }
            //This is added for legacy reasons
            else if ((string.IsNullOrEmpty(link) && AssetHierarchy.Configuration.Version == 1)
                 || (!string.IsNullOrEmpty(link) && link.EndsWith("/level")))
            {
                //Get further subelements
                foreach (var child in nodeToFind.Children)
                {
                    result.Elements.Add(
                        new GlobalFilterElementDTO
                        {
                            Name = child.Name,
                            CategoryNames = child.Template.Categories,
                            IsChromUnit = child.Template.Categories.Contains("ChromUnit"),
                            WebId = child.WebId,
                            IsDevice = child.IsDevice,
                            Description = child.Description,
                            Link = $"{child.WebId}{(child.IsDevice ? "/sensor" : "/level")}",
                            UoM = null,
                            Step = false,
                            ValueType = null
                        }
                    );
                }
            }
            else if (link.EndsWith("/sensor"))
            {
                //Generate Attributes
                foreach (var attribute in nodeToFind.Attribtues)
                {
                    result.Elements.Add(
                        new GlobalFilterElementDTO
                        {
                            Name = attribute.Name,
                            CategoryNames = attribute.Categories,
                            IsChromUnit = false,
                            WebId = attribute.WebId,
                            IsDevice = false,
                            Description = attribute.Description,
                            Link = $"{attribute.WebId}/data",
                            UoM = attribute.UoM,
                            Step = attribute.IsStep,
                            ValueType = attribute.Type
                        }
                    );
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return Task.FromResult(result);
        }

        public Task<GlobalFilterListResponse> GlobalFilterBackwardsAsync(string webId)
        {
            var result = new GlobalFilterListResponse
            {
                Elements = new List<GlobalFilterElementDTO>(),
                Errors = new List<DatasourceError>()
            };

            string nodeIdToFind;

            if (string.IsNullOrEmpty(webId))
            {
                nodeIdToFind = AssetHierarchy.Elements.NodeId;
            }
            else
            {
                nodeIdToFind = WebIdEncoding.WebId2NodeId(webId);
            }

            var path = AssetHierarchy.FindPathToRoot(AssetHierarchy.Elements, nodeIdToFind);

            if (path.Count() == 0)
            {
                result.Errors.Add(
                    new DatasourceError
                    {
                        Error = Errors.ElementNotFound,
                        Identifier = webId,
                        ErrorCode = (byte)ErrorCode.DeleteDevice
                    });

                return Task.FromResult(result);
            }

            if (AssetHierarchy.Configuration.Version == 1)
            {
                path.RemoveAt(0);
            }

            foreach (var el in path)
            {
                result.Elements.Add(
                    new GlobalFilterElementDTO
                    {
                        CategoryNames = el.Categories,
                        Description = el.Description,
                        WebId = el.WebId,
                        IsDevice = el.IsDevice,
                        IsChromUnit = el.Categories.Contains("ChromUnit"),
                        Name = el.Name,
                        Step = false,
                        UoM = null,
                        ValueType = null,
                        Link = el.WebId + (el.IsDevice ? "/sensor" : "/level")
                    });
            }

            if (webId.StartsWith("I1Ab"))
            {
                var element = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, nodeIdToFind);

                var attribute = element.Attribtues.Where(x => x.WebId == webId).FirstOrDefault();

                if (attribute != null)
                {
                    result.Elements.Add(
                        new GlobalFilterElementDTO
                        {
                            CategoryNames = attribute.Categories,
                            Description = attribute.Description,
                            WebId = attribute.WebId,
                            IsChromUnit = false,
                            IsDevice = false,
                            Name = attribute.Name,
                            Step = false,
                            UoM = attribute.UoM,
                            ValueType = attribute.Type,
                            Link = attribute.WebId + "/data"
                        });
                }
            }

            return Task.FromResult(result);
        }

        public Task<GlobalFilterLevelResponse> GetFullHierarchyLevelsAsync(string webId)
        {
            var result = new GlobalFilterLevelResponse
            {
                Levels = new List<GlobalFilterLevelDTO>(),
                Errors = new List<DatasourceError>()
            };

            string nodeIdToFind;
            bool isSensorWebId = string.Equals(webId.Substring(2, 2), "Ab", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(webId))
            {
                nodeIdToFind = AssetHierarchy.Elements.NodeId;
            }
            else
            {
                nodeIdToFind = WebIdEncoding.WebId2NodeId(webId);
            }

            var path = AssetHierarchy.FindPathToRoot(AssetHierarchy.Elements, nodeIdToFind).ToList();

            if (path.Count == 0)
            {
                result.Errors.Add(new DatasourceError
                {
                    Error = Errors.ElementNotFound,
                    Identifier = webId,
                    ErrorCode = (byte)ErrorCode.DeleteDevice
                });

                return Task.FromResult(result);
            }

            var rootOptions = new List<GlobalFilterElementDTO>
            {
                MapNode(AssetHierarchy.Elements)
            };
            string rootSelectedId = rootOptions.First().WebId;

            result.Levels.Add(new GlobalFilterLevelDTO
            {
                Id = rootSelectedId,
                Options = rootOptions
            });

            for (int i = 1; i < path.Count; i++)
            {
                var currentElementNode = path[i];

                var level = new GlobalFilterLevelDTO
                {
                    Id = currentElementNode.WebId,
                    Options = AssetHierarchy.FindSiblings(AssetHierarchy.Elements, currentElementNode.NodeId).Select(MapNode).ToList()
                };

                result.Levels.Add(level);
            }

            if (isSensorWebId)
            {
                var finalElementNode = path.Last();

                var sensorLevel = new GlobalFilterLevelDTO
                {
                    Id = webId,
                    Options = finalElementNode.Attribtues.Select(MapAttribute).ToList()
                };

                result.Levels.Add(sensorLevel);
            }

            return Task.FromResult(result);
        }

        private GlobalFilterElementDTO MapNode(dynamic el)
        {
            return new GlobalFilterElementDTO
            {
                CategoryNames = el.Categories,
                Description = el.Description,
                WebId = el.WebId,
                IsDevice = el.IsDevice,
                IsChromUnit = el.Categories.Contains("ChromUnit"),
                Name = el.Name,
                Step = false,
                UoM = null,
                ValueType = null,
                Link = el.WebId + (el.IsDevice ? "/sensor" : "/level")
            };
        }

        private GlobalFilterElementDTO MapAttribute(dynamic attribute)
        {
            return new GlobalFilterElementDTO
            {
                CategoryNames = attribute.Categories,
                Description = attribute.Description,
                WebId = attribute.WebId,
                IsChromUnit = false,
                IsDevice = false,
                Name = attribute.Name,
                Step = false,
                UoM = attribute.UoM,
                ValueType = attribute.Type,
                Link = attribute.WebId + "/data"
            };
        }

        public async Task<GlobalFilterResponse> GetGlobalFilterElementAsync(string webId)
        {
            var result = new GlobalFilterResponse
            {
                Element = new GlobalFilterElementDTO(),
                Errors = new List<DatasourceError>()
            };

            var webIdCharForCheck = webId.Substring(2, 2);

            if (string.Equals(webIdCharForCheck, "AB", StringComparison.OrdinalIgnoreCase))
            {
                var sensorDetails = await GetSingleSensorDetailsAsync(webId);

                if (sensorDetails.Errors.Count > 0)
                {
                    result.Errors.AddRange(sensorDetails.Errors);

                    return result;
                }

                result.Element = new GlobalFilterElementDTO
                {
                    Name = sensorDetails.Sensor.Name,
                    CategoryNames = sensorDetails.Sensor.CategoryNames,
                    Description = sensorDetails.Sensor.Description,
                    WebId = sensorDetails.Sensor.WebId,
                    Link = null,
                    IsDevice = false,
                    IsChromUnit = false,
                    Step = sensorDetails.Sensor.IsStep,
                    UoM = sensorDetails.Sensor.UoM,
                    ValueType = sensorDetails.Sensor.ValueType,
                };
            }
            else if (string.Equals(webIdCharForCheck, "EM", StringComparison.OrdinalIgnoreCase))
            {
                var nodeId = WebIdEncoding.WebId2NodeId(webId);
                var element = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, nodeId);

                if (element == null)
                {
                    result.Errors.Add(
                        new DatasourceError
                        {
                            Error = Errors.ElementNotFound,
                            Identifier = webId,
                            ErrorCode = (byte)ErrorCode.DeleteDevice
                        });
                    return result;
                }

                result.Element = new GlobalFilterElementDTO
                {
                    CategoryNames = element.Categories,
                    Description = element.Description,
                    WebId = element.WebId,
                    IsDevice = element.IsDevice,
                    IsChromUnit = element.Categories.Contains("ChromUnit"),
                    Name = element.Name,
                    Step = false,
                    UoM = null,
                    ValueType = null,
                    Link = element.WebId + (element.IsDevice ? "/sensor" : "/level")
                };
            }

            return result;
        }

        public Task<GlobalFilterResponse> GetPreviousGlobalFilterAsync(string link)
        {
            var result = new GlobalFilterResponse
            {
                Element = new GlobalFilterElementDTO(),
                Errors = new List<DatasourceError>()
            };

            string nodeIdToFind = WebIdEncoding.WebId2NodeId(link.Split('/')[0]);

            var nodeToFind = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, nodeIdToFind.ToUpper());

            result.Element = new GlobalFilterElementDTO
            {
                CategoryNames = nodeToFind.Categories,
                Description = nodeToFind.Description,
                WebId = nodeToFind.WebId,
                IsDevice = nodeToFind.IsDevice,
                IsChromUnit = nodeToFind.Categories.Contains("ChromUnit"),
                Name = nodeToFind.Name,
                Step = false,
                UoM = null,
                ValueType = null,
                Link = nodeToFind.WebId + (nodeToFind.IsDevice ? "/sensor" : "/level")
            };

            return Task.FromResult(result);
        }

        public Task<GlobalFilterResponse> GetGlobalFilterElementAsync(GlobalFilterElementByPathRequest model)
        {
            var result = new GlobalFilterResponse
            {
                Element = new GlobalFilterElementDTO()
            };

            if (model?.Path == null || model.Path.Count == 0)
                throw new ArgumentException("Invalid input model");

            var names = model.Path
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .Select(p => p.Name)
                .ToList();

            var nodeToFind = AssetHierarchy.FindNodeByPath(AssetHierarchy.Elements, names);

            if (nodeToFind == null)
            {
                result.Errors.Add(
                    new DatasourceError
                    {
                        Error = Errors.ElementNotFound,
                        ErrorCode = (byte)ErrorCode.NotFound
                    });
                return Task.FromResult(result);
            }

            if (nodeToFind is AFElement element)
            {
                result.Element = new GlobalFilterElementDTO
                {
                    CategoryNames = element.Categories,
                    Description = element.Description,
                    WebId = element.WebId,
                    IsDevice = element.IsDevice,
                    IsChromUnit = element.Categories.Contains("ChromUnit"),
                    Name = element.Name,
                    Step = false,
                    UoM = null,
                    ValueType = null,
                    Link = element.WebId + (element.IsDevice ? "/sensor" : "/level")
                };
            }
            else if (nodeToFind is AFAttribute attribute)
            {
                result.Element = new GlobalFilterElementDTO
                {
                    Name = attribute.Name,
                    CategoryNames = attribute.Categories,
                    IsChromUnit = false,
                    WebId = attribute.WebId,
                    IsDevice = false,
                    Description = attribute.Description,
                    Link = $"{attribute.WebId}/data",
                    UoM = attribute.UoM,
                    Step = attribute.IsStep,
                    ValueType = attribute.Type
                };
            }

            return Task.FromResult(result);
        }

        public async Task<GlobalFilterListResponse> SearchGlobalFilterElementsAsync(GlobalFilterElementQueryDTO model)
        {
            if (model?.MetaData == null || model.MetaData.Count == 0)
                throw new ArgumentException("MetaData must contain at least one element");

            var pairs = model.MetaData
                .Where(m => !string.IsNullOrWhiteSpace(m.Name) && !string.IsNullOrWhiteSpace(m.Value))
                .Select(m => new AFMetaData { Name = m.Name, Value = m.Value })
                .ToList();

            if (pairs.Count == 0)
                throw new ArgumentException("No valid name/value pairs provided");

            var resultList = new List<GlobalFilterElementDTO>();

            SearchElementsRecursive(AssetHierarchy.Elements, pairs, resultList);

            var result = new GlobalFilterListResponse
            {
                Elements = resultList,
                Errors = new List<DatasourceError>(),
                Warnings = new List<DatasourceWarning>()
            };

            return await Task.FromResult(result);
        }

        private async void SearchElementsRecursive(AFElement element, List<AFMetaData> pairs, List<GlobalFilterElementDTO> result)
        {
            if (element == null) return;

            if (await ElementMatchesPairs(element, pairs))
            {
                result.Add(new GlobalFilterElementDTO
                {
                    CategoryNames = element.Categories,
                    Description = element.Description,
                    WebId = element.WebId,
                    IsDevice = element.IsDevice,
                    IsChromUnit = element.Categories.Contains("ChromUnit"),
                    Name = element.Name,
                    Step = false,
                    UoM = null,
                    ValueType = null,
                    Link = element.WebId + (element.IsDevice ? "/sensor" : "/level")
                });
            }

            foreach (var child in element.Children)
            {
                SearchElementsRecursive(child, pairs, result);
            }
        }

        private async Task<bool> ElementMatchesPairs(AFElement element, List<AFMetaData> pairs)
        {
            if (element == null)
                return false;

            var metaDataResponse = await ElementMetaDataAsync(element.WebId);
            var metaData = metaDataResponse.MetaData ?? new List<ElementMetaDataDTO>();

            return pairs.All(p =>
            metaData.Any(m =>
                string.Equals(m.Name, p.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(m.Value, p.Value, StringComparison.OrdinalIgnoreCase)
            ));
        }

        public Task<DeviceListResponse> SearchDevicesAsync(DeviceQueryDTO model)
        {
            var result = new DeviceListResponse
            {
                Errors = new List<DatasourceError>(),
                Devices = new List<DeviceDTO>()
            };

            var generator = new AssetHierarchyGenerator(_configuration);
            var element = generator.GenerateAssetHierarchy(AssetHierarchy.ElementTemplates, int.MaxValue);

            FindNodeByName(element);

            void FindNodeByName(AFElement root)
            {
                if (model.NameQuery == null)
                {
                    result.Devices.Add(new DeviceDTO
                    {
                        Name = root.Name,
                        WebId = root.WebId,
                        Path = AssetHierarchy.FindPathToRoot(AssetHierarchy.Elements, root.NodeId)
                            .Select(x => new ElementPathDTO
                            {
                                Name = x.Name,
                                WebId = x.WebId
                            })
                            .ToList(),
                        Description = root.Description,
                        CategoryNames = root.Categories,
                        Link = $"{root.WebId}/sensor"
                    });
                }

                if (!string.IsNullOrEmpty(model.NameQuery) &&
                    root.Name?.ToLowerInvariant().Contains(model.NameQuery.ToLowerInvariant()) == true)
                {
                    result.Devices.Add(new DeviceDTO
                    {
                        Name = root.Name,
                        WebId = root.WebId,
                        Path = AssetHierarchy.FindPathToRoot(AssetHierarchy.Elements, root.NodeId)
                            .Select(x => new ElementPathDTO
                            {
                                Name = x.Name,
                                WebId = x.WebId
                            })
                            .ToList(),
                        Description = root.Description,
                        CategoryNames = root.Categories,
                        Link = $"{root.WebId}/sensor"
                    });
                }

                foreach (var child in root.Children)
                {
                    FindNodeByName(child);
                }
            }

            return Task.FromResult(result);
        }

        public Task<ElementMetaDataResponse> ElementMetaDataAsync(string webId)
        {
            var result = new ElementMetaDataResponse
            {
                Errors = new List<DatasourceError>(),
                MetaData = new List<ElementMetaDataDTO>()
            };

            var deviceId = WebIdEncoding.WebId2NodeId(webId);
            var element = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, deviceId);

            if (element == null)
            {
                result.Errors.Add(new DatasourceError
                {
                    Error = Errors.ElementNotFound,
                    Identifier = webId,
                    ErrorCode = (byte)ErrorCode.DeleteDevice
                });
                return Task.FromResult(result);
            }

            result.MetaData = element.Attribtues.Where(x => x.IsConfiguration).Select(x => new ElementMetaDataDTO
            {
                Name = x.Name,
                Description = x.Description,
                Value = "ElmentMetaData - ToDo",
            }).ToList();

            return Task.FromResult(result);

        }

        public Task<SensorListResponse> GetDeviceSensorsByGroupAsync(string webId, string groupName)
        {
            var result = new SensorListResponse
            {
                Errors = new List<DatasourceError>()
            };

            var deviceId = WebIdEncoding.WebId2NodeId(webId);
            var element = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, deviceId);

            if (element == null)
            {
                result.Errors.Add(
                   new DatasourceError
                   {
                       Error = Errors.ElementNotFound,
                       Identifier = webId
                   });
                return Task.FromResult(result);
            }

            result.Sensors = element.Attribtues.Select(x => new SensorDTO
            {
                Name = x.Name,
                WebId = x.WebId,
                Description = x.Description,
                DeviceWebId = x.ElementWebId,
                CategoryNames = x.Categories,
                UoM = x.UoM,
                IsStep = x.IsStep,
                ValueType = x.Type,
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<SensorListResponse> GetMultipleSensorsDetailsAsync(SensorQueryDTO model)
        {
            var result = new SensorListResponse
            {
                Errors = new List<DatasourceError>(),
                Sensors = new List<SensorDTO>()
            };

            var resultAttributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, model.WebId);

            result.Errors = model.WebId.Except(resultAttributes.Select(x => x.WebId)).Select(x => new DatasourceError
            {
                Error = Errors.SensorNotFound,
                Identifier = x,
                ErrorCode = (byte)ErrorCode.DeleteSensor
            }).ToList();


            result.Sensors = resultAttributes.Select(a =>
            new SensorDTO
            {
                Name = a.Name,
                Description = a.Description,
                UoM = a.UoM,
                CategoryNames = a.Categories,
                IsStep = a.IsStep,
                ValueType = a.Type,
                WebId = a.WebId,
                DeviceWebId = a.ElementWebId
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<DeviceResponse> GetSingleDeviceDetailsAsync(string webId)
        {
            var result = new DeviceResponse
            {
                Errors = new List<DatasourceError>()
            };

            var deviceId = WebIdEncoding.WebId2NodeId(webId);
            var element = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, deviceId);

            if (element == null)
            {
                result.Errors.Add(new DatasourceError
                {
                    Error = Errors.ElementNotFound,
                    Identifier = webId,
                    ErrorCode = (byte)ErrorCode.DeleteDevice
                });

                return Task.FromResult(result);
            }

            result.Device = new DeviceDTO
            {
                Description = element.Description,
                WebId = element.WebId,
                Name = element.Name,
                Path = AssetHierarchy.FindPathToRoot(AssetHierarchy.Elements, element.NodeId).Select(x => new ElementPathDTO
                {
                    Name = x.Name,
                    WebId = x.WebId
                }).ToList(),
                CategoryNames = element.Categories,
                Link = $"{element.WebId}/sensor"
            };

            return Task.FromResult(result);
        }

        public Task<DeviceListResponse> GetMultipleDevicesDetailsAsync(MultiWebIdQueryDTO model)
        {
            var result = new DeviceListResponse
            {
                Errors = new List<DatasourceError>(),
                Devices = new List<DeviceDTO>()
            };

            var deviceIds = model.WebId.Select(webId => WebIdEncoding.WebId2NodeId(webId));
            var elements = deviceIds.Select(deviceId => AssetHierarchy.FindNodeById(AssetHierarchy.Elements, deviceId)).Where(x => x != null);

            result.Errors = model.WebId.Except(elements.Select(e => e.WebId).ToList()).Select(x => new DatasourceError
            {
                Error = "Not exisiting Element",
                Identifier = x,
                ErrorCode = (byte)ErrorCode.DeleteDevice
            }).ToList();

            result.Devices = elements.Select(el => new DeviceDTO
            {
                CategoryNames = el.Categories,
                WebId = el.WebId,
                Path = AssetHierarchy.FindPathToRoot(AssetHierarchy.Elements, el.NodeId).Select(x => new ElementPathDTO
                {
                    Name = x.Name,
                    WebId = x.WebId
                }).ToList(),
                Description = el.Description,
                Name = el.Name,
                Link = $"{el.WebId}/sensor"
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<SensorResponse> GetSingleSensorDetailsAsync(string webId)
        {
            var result = new SensorResponse
            {
                Errors = new List<DatasourceError>()
            };

            var deviceId = WebIdEncoding.Id2NodeId(WebIdEncoding.WebId2ElementId(webId));
            var element = AssetHierarchy.FindNodeById(AssetHierarchy.Elements, deviceId);

            if (element == null)
            {
                result.Errors.Add(new DatasourceError
                {
                    Error = Errors.SensorNotFound,
                    Identifier = webId,
                    ErrorCode = (byte)ErrorCode.DeleteSensor
                });

                return Task.FromResult(result);
            }

            var attribtue = element.Attribtues.FirstOrDefault(a => a.WebId == webId);

            if (attribtue == null)
            {
                result.Errors.Add(new DatasourceError
                {
                    Error = Errors.SensorNotFound,
                    Identifier = webId
                });
            }

            result.Sensor = new SensorDTO
            {
                Description = attribtue.Description,
                WebId = attribtue.WebId,
                Name = attribtue.Name,
                CategoryNames = attribtue.Categories,
                DeviceWebId = attribtue.ElementWebId,
                IsStep = attribtue.IsStep,
                UoM = attribtue.UoM,
                ValueType = attribtue.Type
            };

            return Task.FromResult(result);
        }


        public Task<SensorDeviceInfoResponse> GetSensorsDeviceAsync(List<string> webId)
        {
            var result = new SensorDeviceInfoResponse
            {
                Errors = new List<DatasourceError>(),
                SensorDeviceInfo = new List<SensorDeviceInfoDTO>()
            };

            var resultAttributes = AssetHierarchy.FindAttributesByWebId(AssetHierarchy.Elements, webId);

            result.Errors = webId.Except(resultAttributes.Select(x => x.WebId)).Select(x => new DatasourceError
            {
                Error = Errors.SensorNotFound,
                Identifier = x,
                ErrorCode = (byte)ErrorCode.DeleteSensor
            }).ToList();

            var elements = resultAttributes
                .Select(s => AssetHierarchy.FindNodeById(AssetHierarchy.Elements, WebIdEncoding.Id2NodeId(s.ElementId)))
                .Where(x => x != null);

            result.SensorDeviceInfo = resultAttributes.Select(a => new SensorDeviceInfoDTO
            {
                WebId = a.WebId,
                Device = elements
                    .Where(e => e.WebId == a.ElementWebId)
                    .Select(e => new DeviceDTO
                    {
                        CategoryNames = e.Categories,
                        Description = e.Description,
                        Name = e.Name,
                        WebId = a.WebId,
                        Path = AssetHierarchy.FindPathToRoot(AssetHierarchy.Elements, e.NodeId)
                            .Select(x => new ElementPathDTO
                            {
                                Name = x.Name,
                                WebId = x.WebId
                            })
                            .ToList(),
                        Link = $"{a.WebId}/sensor"
                    })
                    .First()
            }).ToList();

            return Task.FromResult(result);
        }
    }
}
