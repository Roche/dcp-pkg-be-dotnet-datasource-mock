using OpenDCP.Framework.DataTypes.Equipment.Entities.EventframeContext;
using OpenDCP.Framework.DataTypes.Equipment.Interfaces;
using OpenDCP.Framework.DataTypes.Equipment.Requests.EventFrameContext;
using OpenDCP.Framework.DataTypes.Equipment.Responses.EventFrameContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Datasource
{
    public partial class MockDatasource : IEventframeContextDataSource
    {
        public string EventFrameId2WebId(string eventFrameId)
        {
            return _datasource.EventFrameId2WebId(eventFrameId);
        }

        public List<string> EventFrameId2WebId(List<string> id)
        {
            return _datasource.EventFrameId2WebId(id);
        }

        public string ExtractDisplayEventId(List<EventFrameValueDTO> values, string id)
        {
            return _datasource.ExtractDisplayEventId(values);
        }

        public Task<List<string>> FindByAttributesAsync(Dictionary<string, object> attributes)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, List<string>>> FindByAttributesAsync(Dictionary<string, Dictionary<string, object>> attributes)
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, object>> GetAttribtuesAsync(string id)
        {
            var data = await _datasource.GetEventFrameValuesAsync(id);
            return data.Values.ToDictionary(x => x.Name, x => (object)x.Value);
        }

        public async Task<Dictionary<string, Dictionary<string, object>>> GetAttribtuesAsync(List<string> id)
        {
            var model = new EventframeContextIdQueryDTO
            {
                Id = id
            };
            var data = await _datasource.SearchEventframeContextByIdAsync(model);

            return data.Eventframes
                .ToDictionary(
                    x => x.Id,
                    x => x.Values.ToDictionary(v => v.Name, v => v.Value)
                 );
        }

        public Task<EventFrameResponse> GetEventframeAsync(string id, CancellationToken cancellationToken = default)
        {
            return _datasource.GetEventframeAsync(id);
        }

        public Task<EventframeAttributesResponse> GetEventframeAttributesAsync(string deviceWebId, CancellationToken cancellationToken = default)
        {
            return _datasource.GetDeviceEventFrameAttributeAsync(deviceWebId);
        }

        public Task<EventframeTemplateListResponse> GetEventframeTemplatesAsync(List<string> webIds, CancellationToken cancellationToken = default)
        {
            return _datasource.GetEventframeTemplatesAsync(webIds);
        }

        public Task<EventframeValuesResponse> GetEventframeValuesAsync(string id, CancellationToken cancellationToken = default)
        {
            return _datasource.GetEventFrameValuesAsync(id);
        }

        public Task<EventframeAttributesResponse> GetTemplateDetailsAsync(List<string> webIds, CancellationToken cancellationToken = default)
        {
            return _datasource.GetTemplateDetailsAsync(webIds);
        }

        public Task<BatchContextListResponse> SearchBatchContextByTimeAsync(BatchContextTimeQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.SearchBatchContextByTimeAsync(model);
        }

        public Task<EventframeListResponse> SearchEventframeContextByIdAsync(EventframeContextIdQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.SearchEventframeContextByIdAsync(model);
        }

        public Task<EventframeListResponse> SearchEventframeContextByTimeAsync(EventframeContextTimeQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.GetDeviceEventFramesAsync(model);
        }

        public List<string> WebId2EventFrameId(List<string> webId)
        {
            return _datasource.WebId2EventFrameId(webId);
        }

        public string WebId2EventFrameId(string webId)
        {
            return _datasource.WebId2EventFrameId(webId);
        }
    }
}