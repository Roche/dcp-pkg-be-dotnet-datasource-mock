using OpenDCP.Framework.FormBuilder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial interface IMockDatasource
    {
        List<byte> GetImplementedDataTypes();
        void UpdateConnectionString(List<FormValue> connectionString);
        string DisplayConnectionString();
        Task ValidateConnectionString();
    }
}
