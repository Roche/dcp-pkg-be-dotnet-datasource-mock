using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Configuration
{
    public class EventframeContextConfiguration
    {
        // List of base peridos in seconds
        public List<int> BasePeriods { get; set; }
        // Weight factor for each base period
        public List<double> BasePeriodsWeights { get; set; }
        public List<string> AttributeNames { get; set; }
        public virtual int NumberOfEventFrameTemplate { get; set; }


    }
}
