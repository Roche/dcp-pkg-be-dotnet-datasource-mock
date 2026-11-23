using Microsoft.Extensions.Configuration;
using OpenDCP.Framework.Datasource.Base;
using OpenDCP.Framework.Datasource.Mock.Generator;
using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using System.Text.Json.Nodes;

namespace OpenDCP.Framework.Datasource.Mock.Datasource
{
    public partial class MockDatasource : BaseDatasource
    {
        private readonly IMockDatasource _datasource = default!;
        protected new AdminConfiguration _configuration { get; set; } = new AdminConfiguration();

        public MockDatasource(int? id, IConfiguration configuration, JsonObject connectionString) : base(configuration)
        {
            var factory = new MockDatasourceFactory();
            _datasource = factory.LoadMockdatasource(connectionString);
            _configuration = factory.GetDatasourceConfiguration(connectionString);
            _connectionString = connectionString;
        }

        public MockDatasource(IConfiguration configuration) : base(configuration)
        {
        }

    }
}
