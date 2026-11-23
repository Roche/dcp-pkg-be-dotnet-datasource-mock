using OpenDCP.Framework.DataTypes.Analytical.Requests.AnalyticalData;
using OpenDCP.Framework.DataTypes.Analytical.Responses.AnalyticalData;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial interface IMockDatasource
    {
        Task<AttributeListResponse> GetAttributesAsync(string deviceWebId);
        Task<AttributeDataResponse> GetAnalyticalDataAsync(AttributeDataQueryDTO model);
    }
}
