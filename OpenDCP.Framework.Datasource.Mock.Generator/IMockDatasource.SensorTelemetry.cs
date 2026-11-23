using OpenDCP.Framework.DataTypes.Equipment.Requests.SensorData;
using OpenDCP.Framework.DataTypes.Equipment.Responses.SensorData;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial interface IMockDatasource
    {
        Task<SensorDataSummaryStatisticsChunksResponse> GetSummaryDataChunksAsync(SummaryDataChunksRequest model);
        Task<SensorDataChunksResponse> GetRecordedDataChunksAsync(RecordedDataChunksRequest model);
        Task<SensorDataChunksResponse> GetInterpolatedDataChunksAsync(InterpolatedDataChunksRequest model);
        Task<SensorDataResponse> InterpolatedAtTimeDataAsync(InterpolatedAtTimeDataQueryDTO model);
        Task<SensorDataResponse> InterpolatedDataAsync(InterpolatedDataQueryDTO model);
        Task<SensorDataResponse> PlotDataAsync(PlotDataQueryDTO model);
        Task<SensorDataResponse> RecordedAtTimesDataAsync(RecordedAtTimeDataQueryDTO model);
        Task<SensorDataResponse> RecordedDataAsync(RecordedDataQueryDTO model);
    }
}
