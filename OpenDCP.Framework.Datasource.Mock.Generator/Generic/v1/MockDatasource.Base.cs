using OpenDCP.Framework.Configuration.Model.Constants.Datasource;
using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using OpenDCP.Framework.Exceptions;
using OpenDCP.Framework.FormBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial class MockDatasource_v_1_0 : IMockDatasource
    {
        protected AdminConfiguration _configuration { get; set; }

        public MockDatasource_v_1_0(AdminConfiguration configuration)
        {
            _configuration = configuration;
            InitializeEquipment();
        }

        partial void InitializeEquipment();

        public List<byte> GetImplementedDataTypes()
        {
            return new List<byte> {
                (byte)DataType.AnalyticalData,
                (byte)DataType.EquipmentData
            };
        }

        public void UpdateConnectionString(List<FormValue> connectionString)
        {

            if (connectionString == null || !connectionString.Any())
                return;

            foreach (var item in connectionString)
            {
                if (string.Equals(item.Key, "SystemId", StringComparison.OrdinalIgnoreCase))
                {
                    _configuration.SystemId = item.Value.ToString();
                }
                else if (string.Equals(item.Key, "Version", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(item.Value.ToString(), out int version))
                    {
                        _configuration.Version = version;
                    }
                }
            }
        }

        public string DisplayConnectionString()
        {
            return $"Database mock: systemId: {_configuration.SystemId} with version {_configuration.Version}";
        }

        public Task ValidateConnectionString()
        {
            if (_configuration == null)
            {
                throw new DCPBadRequestException("Mock configuration invalid - check sample configuration");
            }

            if (_configuration.Settings.AnalyticalData == null || _configuration.Settings.EventframeContext == null
                || _configuration.Settings.AssetHierarchy == null )
            {
                throw new DCPBadRequestException("Mock configuration invalid - check sample configuration");
            }

            if (!Guid.TryParse(_configuration.SystemId, out Guid systemId))
            {
                throw new DCPBadRequestException("Mock configuration validation failed. Invalid SystemId");
            }

            if (_configuration.Version != 1)
            {
                throw new DCPBadRequestException("Mock configuration validation failed. Invalid Version");
            }

            if (_configuration.SystemId == null)
            {
                throw new DCPBadRequestException("Mock configuration invalid - check sample configuration");
            }
            return Task.CompletedTask;
        }
    }
}
