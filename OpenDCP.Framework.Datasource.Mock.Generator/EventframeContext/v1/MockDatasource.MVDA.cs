using OpenDCP.Framework.Configuration.Model.Errors;
using OpenDCP.Framework.Configuration.Model.Warnings;
using OpenDCP.Framework.Datasource.Mock.Generator.Constants;
using OpenDCP.Framework.DataTypes.Equipment.Constants;
using OpenDCP.Framework.DataTypes.Equipment.Entities.EventframeContext;
using OpenDCP.Framework.DataTypes.Equipment.Responses.EventFrameContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial class MockDatasource_v_1_0
    {
        public Task<EventframeAttributesResponse> GetTemplateDetailsAsync(List<string> webIds)
        {
            var result = new EventframeAttributesResponse
            {
                Errors = new List<DatasourceError>(),
                Warnings = new List<DatasourceWarning>(),
            };

            var templates = AssetHierarchy.EventFrameTemplates
                .Where(x => webIds.Contains(x.WebId)).ToList();

            result.Errors = webIds.Except(templates.Select(a => a.WebId))
                .Select(x => new DatasourceError
                {
                    Error = Errors.EventframeNotFound,
                    Identifier = x,
                    ErrorCode = (byte)ErrorCode.DeletedTemplate
                }).ToList();

            var attributes = templates
                .Select(x => x.GetAttributeTempaltes().Select(a => new EventFrameAttributeDTO
                {
                    Description = a.Description,
                    Name = a.Name,
                    ValueType = a.Type,
                    UoM = a.UoM,
                }).ToList()).SelectMany(x => x).ToList();

            result.Attributes = attributes;

            return Task.FromResult(result);
        }
    }
}
