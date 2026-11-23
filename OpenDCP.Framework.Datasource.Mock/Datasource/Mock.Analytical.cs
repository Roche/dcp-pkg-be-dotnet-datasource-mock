using OpenDCP.Framework.DataTypes.Analytical.Interfaces;
using OpenDCP.Framework.DataTypes.Analytical.Requests.AnalyticalData;
using OpenDCP.Framework.DataTypes.Analytical.Responses.AnalyticalData;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Datasource
{
    public partial class MockDatasource : IAnalyticalDataSource
    {
        public Task<AttributeDataResponse> GetAnalyticalDataAsync(AttributeDataQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.GetAnalyticalDataAsync(model);
        }

        public Task<AttributeListResponse> GetAttributesAsync(string webId, CancellationToken cancellationToken = default)
        {
            return _datasource.GetAttributesAsync(webId);
        }
    }
}
