using OpenDCP.Framework.Configuration.Model.Constants.Datasource;
using OpenDCP.Framework.Datasource.Base.Interfaces;
using OpenDCP.Framework.Datasource.Mock.Generator;
using OpenDCP.Framework.Exceptions;
using OpenDCP.Framework.FormBuilder.Constants;
using OpenDCP.Framework.FormBuilder.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Datasource
{
    public partial class MockDatasource : IDatasource
    {
        protected override List<InputField> ConnectionStringFields { get; } = new List<InputField>
        {
            new InputField
            {
                FieldType = (byte)InputFieldType.String,
                Name = "SystemId",
                Label = "SystemId",
                FieldValidations = new List<FieldValidationDTO>
                {
                    new FieldValidationDTO
                    {
                        Type = InputFieldValidationType.Required.ToString(),
                    }
                }
            },
            new InputField
            {
                FieldType = (byte)InputFieldType.Number,
                Name = "Version",
                Label = "Version",
                FieldSettings = new FieldSettings
                {
                    DefaultValue = 1,
                    StepSize = 1,
                },
                FieldValidations = new List<FieldValidationDTO>
                {
                    new FieldValidationDTO
                    {
                        Type = InputFieldValidationType.Required.ToString(),
                    }
                }
            },
            new InputField
            {
                FieldType = (byte)InputFieldType.TextArea,
                Name = "Settings",
                Label = "Settings",
                FieldValidations = new List<FieldValidationDTO>
                {
                    new FieldValidationDTO
                    {
                        Type = InputFieldValidationType.Required.ToString(),
                    }
                }
            }
        };

        [Obsolete]
        protected override ConnectionType Type { get; } = Configuration.Model.Constants.Datasource.ConnectionType.Mock;

        public new static string ConnectionType { get; } = "Mock";

        protected override List<string> SecretFields { get; } = new List<string>();

        protected override List<string> SecretDistributionFields { get; } = new List<string>();

        public override string DisplayConnectionString()
        {
            return _datasource.DisplayConnectionString();
        }

        public Task ValidateConnectionString(JsonObject connectionString)
        {
            if (connectionString == null)
            {
                throw new DCPBadRequestException("Mock configuration invalid - check sample configuration");
            }

            try
            {
                var factory = new MockDatasourceFactory();
                _configuration = factory.GetDatasourceConfiguration(connectionString);
            }
            catch (Exception)
            {
                throw new DCPBadRequestException("Mock configuration validation failed, invalid configuration");
            }

            if (!Guid.TryParse(_configuration.SystemId, out Guid systemId) || _configuration.SystemId == null)
            {
                throw new DCPBadRequestException("Mock configuration validation failed. Invalid SystemId");
            }

            if (_configuration.Version > 3 || _configuration.Version < 1)
            {
                throw new DCPBadRequestException("Mock configuration validation failed. Invalid Version");
            }

            if (_configuration.Settings == null)
            {
                throw new DCPBadRequestException("Mock configuration invalid - Missing configuration settings");
            }

            if (_configuration.Settings.AnalyticalData == null
                || _configuration.Settings.EventframeContext == null
                || _configuration.Settings.AssetHierarchy == null)
            {
                throw new DCPBadRequestException("Mock configuration invalid - check sample configuration");
            }

            return Task.CompletedTask;
        }

        public new void UpdateConnectionString(List<FormValue> connectionString)
        {
            _datasource.UpdateConnectionString(connectionString);
        }

        public override List<DataType> GetImplementedDataTypes()
        {
            return new List<DataType> {
                DataType.AnalyticalData,
                DataType.EquipmentData,
                DataType.AssetHierarchy,
                DataType.EventframeContext,
                DataType.SensorTelemetry
            };
        }

        public override JsonObject TransformConnectionString(List<FormValue> connectionString, JsonObject? previousConnectionString)
        {
            JsonObject connString = new JsonObject();

            foreach (var item in connectionString)
            {
                if (string.Equals(item.Key, "SystemId", StringComparison.OrdinalIgnoreCase))
                {
                    connString["SystemId"] = item.Value.ToString();
                }
                else if (string.Equals(item.Key, "Version", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(item.Value.ToString(), out int version))
                    {
                        connString["Version"] = version;
                    }
                }
                else if (string.Equals(item.Key, "Settings", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(item.Value.ToString()))
                        continue;

                    try
                    {
                        var node = JsonNode.Parse(item.Value.ToString());
                        var parsedSettings = JsonNode.Parse(item.Value.ToString()) as JsonObject
                        ?? throw new JsonException("Settings must be a JSON object.");

                        connString["Settings"] = parsedSettings;
                    }
                    catch (JsonException ex)
                    {
                        throw new ArgumentException("Settings is not valid JSON.", ex);
                    }
                }
            }

            return connString;
        }
    }
}
