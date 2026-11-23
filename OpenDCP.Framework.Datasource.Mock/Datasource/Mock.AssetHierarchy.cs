using OpenDCP.Framework.DataTypes.Equipment.Interfaces;
using OpenDCP.Framework.DataTypes.Equipment.Requests.AssetHierarchy;
using OpenDCP.Framework.DataTypes.Equipment.Responses.AssetHierarchy;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Datasource
{
    public partial class MockDatasource : IAssetHierarchyDataSource
    {
        public Task<ElementMetaDataResponse> GetElementMetaDataAsync(string webId, CancellationToken cancellationToken = default)
        {
            return _datasource.ElementMetaDataAsync(webId);
        }

        public Task<GlobalFilterListResponse> GetGlobalFilterAsync(string link, CancellationToken cancellationToken = default)
        {
            return _datasource.GetGlobalFilterAsync(link);
        }

        public Task<GlobalFilterListResponse> GetGlobalFilterBackwardsAsync(string webId, CancellationToken cancellationToken = default)
        {
            return _datasource.GlobalFilterBackwardsAsync(webId);
        }

        public Task<GlobalFilterLevelResponse> GetFullHierarchyLevelsAsync(string webId, CancellationToken cancellationToken = default)
        {
            return _datasource.GetFullHierarchyLevelsAsync(webId);
        }

        public Task<GlobalFilterResponse> GetGlobalFilterElementAsync(string webId, CancellationToken cancellationToken = default)
        {
            return _datasource.GetGlobalFilterElementAsync(webId);
        }

        public Task<GlobalFilterResponse> GetPreviousGlobalFilterAsync(string link, CancellationToken cancellationToken = default)
        {
            return _datasource.GetPreviousGlobalFilterAsync(link);
        }

        public Task<DeviceListResponse> SearchDevicesAsync(DeviceQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.SearchDevicesAsync(model);
        }

        public Task<SensorListResponse> GetDeviceSensorsAsync(string webId, string groupName = null, CancellationToken cancellationToken = default)
        {
            return _datasource.GetDeviceSensorsByGroupAsync(webId, groupName);
        }

        public Task<DeviceResponse> GetSingleDeviceAsync(string webId, CancellationToken cancellationToken = default)
        {
            return _datasource.GetSingleDeviceDetailsAsync(webId);
        }

        public Task<DeviceListResponse> GetMultipleDevicesAsync(MultiWebIdQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.GetMultipleDevicesDetailsAsync(model);
        }

        public Task<SensorResponse> GetSingleSensorAsync(string webId, CancellationToken cancellationToken = default)
        {
            return _datasource.GetSingleSensorDetailsAsync(webId);
        }

        public Task<SensorListResponse> GetMultipleSensorsAsync(SensorQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.GetMultipleSensorsDetailsAsync(model);
        }

        public Task<DatasourceUnitResponse> GetUnitsOfMeasureAsync(CancellationToken cancellationToken = default)
        {
            return _datasource.GetUnitsOfMeasureAsync();
        }

        public Task<GlobalFilterResponse> GetGlobalFilterElementAsync(GlobalFilterElementByPathRequest model, CancellationToken cancellationToken = default)
        {
            return _datasource.GetGlobalFilterElementAsync(model);
        }

        public Task<GlobalFilterListResponse> SearchGlobalFilterElementsAsync(GlobalFilterElementQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.SearchGlobalFilterElementsAsync(model);
        }

        public Task<SensorDeviceInfoResponse> GetSensorsDeviceAsync(List<string> webId, CancellationToken cancellationToken = default)
        {
            return _datasource.GetSensorsDeviceAsync(webId);
        }

        public Task<GroupedSensorListResponse> GetGroupedDeviceSensorsAsync(string webId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public Task<string> GetPreviousGlobalFilterWebIdAsync(string webId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}