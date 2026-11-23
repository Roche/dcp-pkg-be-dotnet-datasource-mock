using OpenDCP.Framework.DataTypes.Equipment.Entities.EventframeContext;
using OpenDCP.Framework.DataTypes.Equipment.Requests.EventFrameContext;
using OpenDCP.Framework.DataTypes.Equipment.Responses.EventFrameContext;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial interface IMockDatasource
    {
        string GetDisplayId();
        string EventFrameId2WebId(string eventFrameId);
        string WebId2EventFrameId(string webId);
        List<string> EventFrameId2WebId(List<string> id);
        List<string> WebId2EventFrameId(List<string> webId);
        string ExtractDisplayEventId(List<EventFrameValueDTO> values);
        Task<EventFrameResponse> GetEventframeAsync(string id);
        Task<EventframeValuesResponse> GetEventFrameValuesAsync(string id);
        Task<EventframeTemplateListResponse> GetEventframeTemplatesAsync(List<string> webIds);
        Task<EventframeListResponse> SearchEventframeContextByIdAsync(EventframeContextIdQueryDTO model);
        Task<EventframeAttributesResponse> GetDeviceEventFrameAttributeAsync(string deviceWebId);
        Task<EventframeListResponse> GetDeviceEventFramesAsync(EventframeContextTimeQueryDTO model);
        Task<BatchContextListResponse> SearchBatchContextByTimeAsync(BatchContextTimeQueryDTO model);
        Task<EventframeAttributesResponse> GetTemplateDetailsAsync(List<string> webIds);
    }
}
