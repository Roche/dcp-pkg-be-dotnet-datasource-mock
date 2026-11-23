using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.AssetHierarchy.Generators
{
    public class AssetHierarchyGenerator
    {
        private readonly AdminConfiguration _dataSourceConfiguration;
        private Random random;


        public AssetHierarchyGenerator(AdminConfiguration dataSourceConfiguration)
        {
            _dataSourceConfiguration = dataSourceConfiguration ?? throw new ArgumentNullException(nameof(dataSourceConfiguration));
            random = new Random(IdUtils.GetSeed(new List<string> { dataSourceConfiguration.SystemId }));
        }

        public string GenerateHexId(int length = 30)
        {
            const string chars = "0123456789ABCDEF";
            char[] hexArray = new char[length];
            for (int i = 0; i < length; i++)
            {
                hexArray[i] = chars[random.Next(chars.Length)];
            }
            return new string(hexArray);
        }

        public AFElement GenerateAssetHierarchy(List<AFElementTemplate> templates, int maxChildren = 10)
        {
            var root = new AFElement(GenerateHexId(), _dataSourceConfiguration, null);

            var nodesByDepth = new Dictionary<int, List<AFElement>>();
            nodesByDepth[0] = new List<AFElement> { root };


            for (int depth = 1; depth <= _dataSourceConfiguration.Settings.AssetHierarchy.MaxDepth; depth++)
            {
                var nodes = new List<AFElement>();
                foreach (var parent in nodesByDepth[depth - 1])
                {
                    int maxChildsLevel = (int)Math.Min(maxChildren, Math.Floor(_dataSourceConfiguration.Settings.AssetHierarchy.MaxDepth * Math.Exp(-0.2 * (depth - 1))));
                    int numChildren = random.Next(0, maxChildsLevel);

                    for (int i = 0; i < numChildren; i++)
                    {
                        int templateIndex = random.Next(0, templates.Count - 1);
                        var childId = GenerateHexId();
                        var childNode = new AFElement(childId, _dataSourceConfiguration, templates[templateIndex]);
                        parent.Children.Add(childNode);
                        nodes.Add(childNode);
                    }
                }
                nodesByDepth[depth] = nodes;
            }

            return root;
        }

        public AFElement FindNodeById(AFElement root, string nodeId)
        {
            if (root.NodeId == nodeId)
                return root;

            foreach (var child in root.Children)
            {
                var foundNode = FindNodeById(child, nodeId);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }
    }
}
