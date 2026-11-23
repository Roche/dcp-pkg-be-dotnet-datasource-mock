using OpenDCP.Framework.DataTypes.Equipment.Interfaces;
using OpenDCP.Framework.DataTypes.Equipment.Requests.SensorData;
using OpenDCP.Framework.DataTypes.Equipment.Responses.SensorData;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Datasource
{
    public partial class MockDatasource : ISensorTelemetryDataSource
    {
        public Task<SensorDataResponse> GetInterpolatedDataAsync(InterpolatedDataQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.InterpolatedDataAsync(model);
        }

        public Task<SensorDataResponse> GetInterpolatedDataAtTimesAsync(InterpolatedAtTimeDataQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.InterpolatedAtTimeDataAsync(model);
        }

        public Task<SensorDataChunksResponse> GetInterpolatedDataChunksAsync(InterpolatedDataChunksRequest model, CancellationToken cancellationToken = default)
        {
            return _datasource.GetInterpolatedDataChunksAsync(model);
        }

        public Task<SensorDataResponse> GetPlotDataAsync(PlotDataQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.PlotDataAsync(model);
        }

        public Task<SensorDataResponse> GetRecordedDataAsync(RecordedDataQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.RecordedDataAsync(model);
        }

        public Task<SensorDataResponse> GetRecordedDataAtTimesAsync(RecordedAtTimeDataQueryDTO model, CancellationToken cancellationToken = default)
        {
            return _datasource.RecordedAtTimesDataAsync(model);
        }

        public Task<SensorDataChunksResponse> GetRecordedDataChunksAsync(RecordedDataChunksRequest model, CancellationToken cancellationToken = default)
        {
            return _datasource.GetRecordedDataChunksAsync(model);
        }

        public Task<SensorDataSummaryStatisticsChunksResponse> GetSummaryDataChunksAsync(SummaryDataChunksRequest model, CancellationToken cancellationToken = default)
        {
            return _datasource.GetSummaryDataChunksAsync(model);
        }
    }
}