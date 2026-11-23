using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using OpenDCP.Framework.Datasource.Mock.Generator.JsonSerializerContexts;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public class MockDatasourceFactory
    {
        public IMockDatasource LoadMockdatasource(JsonObject connectionString)
        {
            var configuration = GetDatasourceConfiguration(connectionString);

            switch (configuration.Version)
            {
                case 1:
                    return new MockDatasource_v_1_0(configuration);
                case 2:
                    return new MockDatasource_v_1_0(configuration);
                case 3:
                    return new MockDatasource_v_1_0(configuration);
                default:
                    throw new Exception("Unkown Version");
            }
        }

        public AdminConfiguration GetDatasourceConfiguration(JsonObject connectionString)
        {
            AdminConfiguration configuration = null;

            if (connectionString.ContainsKey("MockConfig"))
            {
                var mockConfigJson = connectionString["MockConfig"];
                configuration = JsonSerializer.Deserialize<AdminConfiguration>(mockConfigJson.ToString(), MockJsonContext.Default.AdminConfiguration);
            }
            else
            {
                configuration = JsonSerializer.Deserialize<AdminConfiguration>(connectionString.ToString(), MockJsonContext.Default.AdminConfiguration);
            }

            if (configuration == null)
            {
                throw new Exception("Missing configuration");
            }

            return configuration;
        }
    }
}
