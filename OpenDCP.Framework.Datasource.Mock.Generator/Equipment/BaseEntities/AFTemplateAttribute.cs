using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities
{
    public class AFTemplateAttribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string UoM { get; set; }
        public List<string> Categories { get; set; }
        public bool IsConfiguration { get; set; }
        public bool IsStep { get; set; }
        public string Type { get; set; }
        public string Version { get; set; } = "1.0";
    }
}
