using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;


namespace OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities
{
    public class AFEventFrameTemplate
    {
        public AdminConfiguration Configuration { get; set; }

        public AFEventFrameTemplate(Guid id, AdminConfiguration config)
        {
            Configuration = config;
            Id = id;
        }
        public virtual List<string> Categories
        {
            get
            {
                return new List<string> { "ChromUnit", "Test" };
            }
        }
        public Guid Id { get; private set; }
        public virtual string Name
        {
            get
            {
                return $"Eventframe template - {Id}";
            }
        }
        public virtual string WebId
        {
            get
            {
                return "I1ET" + WebIdEncoding.EncodeId(new Guid(Configuration.SystemId)) + WebIdEncoding.EncodeId(Id);
            }
        }

        public IEnumerable<AFEventframeTemplateAttribute> GetAttributeTempaltes()
        {
            var seed = IdUtils.GetSeed(new List<string> { Id.ToString() });
            var r = new Random(seed);

            var result = new List<AFEventframeTemplateAttribute>
            {
                new AFEventframeTemplateAttribute
                {
                    Template = this,
                    Name = Configuration.Settings.AssetHierarchy.ConfigurationItemAttributeName,
                    Description = "DisplayID",
                    Type = "String",
                    IsConfiguration = false,
                    Categories = new List<string>(),
                    UoM = "dim",
                    TimeMethod = "StartTime"
                }
            };

            for (var i = 0; i <= r.Next(2, 10); i++)
            {
                var attributeInfo = Configuration.Settings.EventframeContext.AttributeNames[r.Next(0, Configuration.Settings.EventframeContext.AttributeNames.Count - 1)];
                var attributeName = attributeInfo.Split('|').First();
                attributeName = result.Select(x => x.Name).Contains(attributeName) ? $"{attributeName} {i}" : attributeName;

                result.Add(
                    new AFEventframeTemplateAttribute
                    {
                        Template = this,
                        Categories = new List<string> { "Test" },
                        Description = "Test",
                        Name = attributeName,
                        UoM = Configuration.Settings.AssetHierarchy.UoM[r.Next(0, Configuration.Settings.AssetHierarchy.UoM.Count - 1)],
                        IsConfiguration = false,
                        Type = attributeInfo.Split('|').Last()
                    }
                );
            }

            //This adds an attribute to be able to differentiate between the templates
            if (Configuration.Version > 2)
            {
                result.Add(
                    new AFEventframeTemplateAttribute
                    {
                        Template = this,
                        Categories = new List<string> { "constant" },
                        Description = "Test",
                        Name = "Constant",
                        UoM = "dim",
                        IsConfiguration = false,
                        Type = "String"
                    }
                );
            }

            return result;
        }
    }
}
