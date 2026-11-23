using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities
{
    public class AFElementTemplate
    {
        public AdminConfiguration Configuration { get; set; }

        public AFElementTemplate(Guid id, AdminConfiguration config)
        {
            Configuration = config;
            Id = id;
        }
        public virtual List<string> Categories
        {
            get
            {
                return GetCategories();
            }
        }
        public Guid Id { get; private set; }
        public virtual string WebId
        {
            get
            {
                return "I1ET" + WebIdEncoding.EncodeId(new Guid(Configuration.SystemId)) + WebIdEncoding.EncodeId(Id);
            }
        }
        public List<AFTemplateAttribute> GetAttributeTempaltes()
        {
            var seed = IdUtils.GetSeed(new List<string> { Id.ToString() });
            var r = new Random(seed);

            var result = new List<AFTemplateAttribute>();

            for (var i = 0; i <= r.Next(2, 10); i++)
            {
                var locationName = Configuration.Settings.AssetHierarchy.LocationNames[r.Next(0, Configuration.Settings.AssetHierarchy.LocationNames.Count - 1)];
                var attributeDetails = Configuration.Settings.AssetHierarchy.AttributeNames[r.Next(0, Configuration.Settings.AssetHierarchy.AttributeNames.Count - 1)];
                var attributeInfo = attributeDetails.Split('|').Last();

                var itemToAdd = new AFTemplateAttribute
                {
                    Categories = new List<string> { "Test" },
                    Description = "Description ",
                    Name = $"{locationName} {attributeDetails.Split('|').First()} - MZ{Id.ToString().Substring(10, 2)}{i}",
                    UoM = attributeInfo.Split(':').Last(),
                    IsConfiguration = Configuration.Settings.AssetHierarchy.StepAttributesPercentage < r.Next(0, 100),
                    IsStep = Configuration.Settings.AssetHierarchy.StepAttributesPercentage < r.Next(0, 100),
                    Type = attributeInfo.Split(':').First()

                };

                if (Configuration.Version > 2)
                {
                    itemToAdd.Categories.Add("Template");
                }

                if (Categories.Contains("realtime"))
                {
                    itemToAdd.Categories.Add("realtime");
                }

                itemToAdd.Description = $"Description {itemToAdd.Name}";
                result.Add(itemToAdd);

                //Downwards compability - keeping the head of the random generator conistent
                r.Next();
                r.NextDouble();

            }

            //Add special attributes with data requirements for version 3 and above
            if (Configuration.Version > 2)
            {
                result.Add(new AFTemplateAttribute
                {
                    Categories = new List<string> { "Test", "Template", "Maturity" },
                    Description = "Description ",
                    Name = $"Maturity",
                    UoM = "L",
                    IsConfiguration = false,
                    IsStep = false,
                    Type = "Double"
                });

                result.Add(new AFTemplateAttribute
                {
                    Categories = new List<string> { "Test", "Template", "Constant" },
                    Description = "Description ",
                    Name = $"Constant",
                    UoM = "bar",
                    IsConfiguration = false,
                    IsStep = true,
                    Type = "Double"
                });
            }

            return result;
        }

        private List<string> GetCategories()
        {
            var categories = new List<string> { "Test" };

            if (Configuration.Version > 2)
            {
                var r = new Random(IdUtils.GetSeed(new List<string> { Id.ToString() }));
                var val = r.NextDouble();

                if (val > 0.65)
                {
                    categories.Add("realtime");

                    if (val > 0.75)
                    {
                        categories.Add("paralell");
                    }
                }
            }
            else
            {
                categories.Add("ChromUnit");
            }

            return categories;
        }
    }
}
