using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using System.Text.Json.Serialization;

namespace OpenDCP.Framework.Datasource.Mock.Generator.JsonSerializerContexts
{
    [JsonSerializable(typeof(AdminConfiguration))]
    public partial class MockJsonContext : JsonSerializerContext
    {

    }
}
