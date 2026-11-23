using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities
{
    public class AFElement
    {
        public string NodeId { get; private set; }
        public List<AFElement> Children { get; private set; } = new List<AFElement>();
        public AdminConfiguration Configuration { get; set; }
        public int Depth { get; private set; }
        public AFElementTemplate Template { get; private set; }
        public AFElement(string nodeId, AdminConfiguration config, AFElementTemplate template)
        {
            NodeId = nodeId;
            Children = new List<AFElement>();
            Configuration = config;
            Template = template;
        }
        public virtual Guid Id
        {
            get
            {
                var guidString = $"{NodeId.Substring(0, 8)}-{NodeId.Substring(8, 4)}-4{NodeId.Substring(12, 3)}-a{NodeId.Substring(15, 3)}-{NodeId.Substring(18, 12)}";
                Guid.TryParse(guidString, out Guid res);
                return res;
            }
        }
        public virtual List<string> Categories
        {
            get
            {
                if (Template == null)
                {
                    return new List<string>();
                }
                else if (!IsDevice)
                {
                    return Template.Categories.Except(new List<string> { "realtime", "paralell" }).ToList();
                }
                return Template.Categories;
            }
        }
        public virtual string WebId
        {
            get
            {
                return "I1Em" + WebIdEncoding.EncodeId(new Guid(Configuration.SystemId)) + WebIdEncoding.EncodeId(Id);
            }
        }
        public virtual bool IsDevice
        {
            get
            {
                return Children.Count == 0;
            }
        }
        public virtual string Name
        {
            get
            {
                return $"{(IsDevice ? "Device" : "Area")} - {Regex.Replace(Id.ToString(), @"[^0-9]+", "").Substring(0, 2)}";
            }
        }
        public virtual string Description
        {
            get
            {
                return $"Description - Element {Regex.Replace(Id.ToString(), @"[^0-9]+", "").Substring(0, 2)}";
            }
        }
        public virtual List<AFAttribute> Attribtues
        {
            get
            {
                return GenerateAttribtues();
            }
        }
        private List<AFAttribute> GenerateAttribtues()
        {
            var attribtues = new List<AFAttribute>();

            if (Template != null)
            {
                foreach (var attributeTemplate in Template.GetAttributeTempaltes())
                {

                    var attributeSeed = new List<string> { Id.ToString(), Template.Id.ToString(), attributeTemplate.Name };
                    var attribute = new AFAttribute(GenerateAttributeId(attributeSeed, Template.Id.ToString(), true), Id, Configuration);
                    attribute.EntityVersion = attributeTemplate.Version;
                    attribute.Name = attributeTemplate.Name;
                    attribute.Description = attributeTemplate.Description;
                    attribute.UoM = attributeTemplate.UoM;
                    attribute.IsStep = attributeTemplate.IsStep;
                    attribute.Categories = attributeTemplate.Categories;
                    attribute.IsConfiguration = attributeTemplate.IsConfiguration;
                    attribute.Type = attributeTemplate.Type;

                    attribtues.Add(attribute);
                }
            }

            attribtues = attribtues.Concat(GetNonTempalteAttributes()).Concat(GetNonTempalteAttributesV1()).ToList();

            return attribtues;
        }

        private List<AFAttribute> GetNonTempalteAttributesV1()
        {
            var seed = new List<string>
            {
                Id.ToString(),
                "v1.1"
            };

            var r = new Random(IdUtils.GetSeed(seed));

            var attributes = new List<AFAttribute>();

            for (var i = 0; i <= r.Next(0, 2); i++)
            {
                var attrSeed = new List<string> { Id.ToString(), i.ToString(), "v1.1" };
                var attribtueId = GenerateAttributeId(attrSeed, Id.ToString(), false);

                var attribute = new AFAttribute(attribtueId, Id, Configuration);
                var attributeDetails = Configuration.Settings.AssetHierarchy.AttributeNames[r.Next(0, Configuration.Settings.AssetHierarchy.AttributeNames.Count - 1)];
                var attributeInfo = attributeDetails.Split('|').Last();

                attribute.Name = $"Device - {Configuration.Settings.AssetHierarchy.LocationNames[r.Next(0, Configuration.Settings.AssetHierarchy.LocationNames.Count - 1)]} {attributeDetails.Split('|').First()} - MZ{Id.ToString().Substring(12, 2)}{i}";
                attribute.Description = $"Non- Template Attribute {attribute.Name}";
                attribute.UoM = attributeInfo.Split(':').Last();
                attribute.IsStep = Configuration.Settings.AssetHierarchy.StepAttributesPercentage < r.Next(0, 100);
                attribute.Categories = new List<string> { "NonTemplateAttribute" };
                attribute.IsConfiguration = Configuration.Settings.AssetHierarchy.StepAttributesPercentage < r.Next(0, 100);
                attribute.Type = attributeInfo.Split(':').First();
                attribute.EntityVersion = "1.1";
                attributes.Add(
                    attribute
                );
            }

            return attributes;
        }

        private List<AFAttribute> GetNonTempalteAttributes()
        {
            var seed = new List<string>
            {
                Id.ToString()
            };

            var r = new Random(IdUtils.GetSeed(seed));

            var attributes = new List<AFAttribute>();

            for (var i = 0; i <= r.Next(0, 5); i++)
            {
                var attrSeed = new List<string> { Id.ToString(), i.ToString() };
                var attribtueId = GenerateAttributeId(attrSeed, Id.ToString(), false);

                var attribute = new AFAttribute(attribtueId, Id, Configuration);
                var attributeDetails = Configuration.Settings.AssetHierarchy.AttributeNames[r.Next(0, Configuration.Settings.AssetHierarchy.AttributeNames.Count - 1)];
                var attributeInfo = attributeDetails.Split('|').Last();

                attribute.Name = $"Device - {Configuration.Settings.AssetHierarchy.LocationNames[r.Next(0, Configuration.Settings.AssetHierarchy.LocationNames.Count - 1)]} {attributeDetails.Split('|').First()} - MZ{Id.ToString().Substring(10, 2)}{i}";
                attribute.Description = $"Non- Template Attribute {attribute.Name}";
                attribute.UoM = attributeInfo.Split(':').Last();
                attribute.IsStep = Configuration.Settings.AssetHierarchy.StepAttributesPercentage < r.Next(0, 100);
                attribute.Categories = new List<string> { "NonTemplateAttribute" };
                attribute.IsConfiguration = Configuration.Settings.AssetHierarchy.StepAttributesPercentage < r.Next(0, 100);
                attribute.Type = attributeInfo.Split(':').First();

                //Downwards compability - keeping the head of the random generator conistent
                r.Next();
                r.NextDouble();

                attributes.Add(
                    attribute
                );
            }

            return attributes;
        }
        private Guid GenerateAttributeId(List<string> seed, string key, bool isTemplate)
        {
            var seedString = GenerateHexCode(IdUtils.GetSeed(seed));
            var guidString = $"{(isTemplate ? "1" : "a")}{key.Substring(0, 7)}-{seedString.Substring(0, 4)}-4{seedString.Substring(4, 3)}-4{seedString.Substring(7, 3)}-{seedString.Substring(10, 12)}";
            return new Guid(guidString);
        }
        private static string GenerateHexCode(int seed)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the seed to a byte array
                byte[] seedBytes = BitConverter.GetBytes(seed);

                // Compute hash from the seed
                byte[] hashBytes = sha256.ComputeHash(seedBytes);

                // Convert hash bytes to hexadecimal string
                StringBuilder hexBuilder = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    hexBuilder.AppendFormat("{0:x2}", b);
                }

                // Trim to 23 characters (92 hex digits)
                string hexCode = hexBuilder.ToString().Substring(0, 25);

                return hexCode;
            }
        }
    }
}
