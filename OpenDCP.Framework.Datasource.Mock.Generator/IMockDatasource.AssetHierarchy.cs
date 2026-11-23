using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using OpenDCP.Framework.DataTypes.Equipment.Requests.AssetHierarchy;
using OpenDCP.Framework.DataTypes.Equipment.Responses.AssetHierarchy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial interface IMockDatasource
    {
        AFHirachey AssetHierarchy { get; set; }
        Task<DatasourceUnitResponse> GetUnitsOfMeasureAsync();
        Task<ElementMetaDataResponse> ElementMetaDataAsync(string webId);
        Task<SensorListResponse> GetDeviceSensorsByGroupAsync(string webId, string groupName);
        Task<GlobalFilterListResponse> GetGlobalFilterAsync(string link);
        Task<SensorListResponse> GetMultipleSensorsDetailsAsync(SensorQueryDTO model);
        Task<DeviceResponse> GetSingleDeviceDetailsAsync(string webId);
        Task<DeviceListResponse> GetMultipleDevicesDetailsAsync(MultiWebIdQueryDTO model);
        Task<SensorResponse> GetSingleSensorDetailsAsync(string webId);
        Task<GlobalFilterListResponse> GlobalFilterBackwardsAsync(string webId);
        Task<GlobalFilterLevelResponse> GetFullHierarchyLevelsAsync(string webId);
        Task<GlobalFilterResponse> GetPreviousGlobalFilterAsync(string link);
        Task<GlobalFilterResponse> GetGlobalFilterElementAsync(string webId);
        Task<GlobalFilterResponse> GetGlobalFilterElementAsync(GlobalFilterElementByPathRequest model);
        Task<GlobalFilterListResponse> SearchGlobalFilterElementsAsync(GlobalFilterElementQueryDTO model);
        Task<DeviceListResponse> SearchDevicesAsync(DeviceQueryDTO model);
        Task<SensorDeviceInfoResponse> GetSensorsDeviceAsync(List<string> webIds);
    }
}
