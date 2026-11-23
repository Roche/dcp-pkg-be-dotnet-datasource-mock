using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Configuration
{
    public class AnalyticalDataConfiguration
    {
        public List<string> AttributeNames { get; set; }
        public List<string> UoM { get; set; }
        public int MaxAttributes { get; set; } = 10;

    }
}
