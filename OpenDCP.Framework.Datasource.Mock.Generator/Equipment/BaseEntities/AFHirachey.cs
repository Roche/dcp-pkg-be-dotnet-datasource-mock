using OpenDCP.Framework.Datasource.Mock.Generator.AssetHierarchy.Generators;
using OpenDCP.Framework.Datasource.Mock.Generator.Configuration;
using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities
{
    public class AFHirachey
    {
        public AFHirachey(AdminConfiguration datasourceConfig)
        {
            Configuration = datasourceConfig;
            ElementTemplates = GenerateElementTemplates();
            EventFrameTemplates = GenerateEventFrameTemplates();

            var generator = new AssetHierarchyGenerator(Configuration);
            Elements = generator.GenerateAssetHierarchy(ElementTemplates, Configuration.Settings.AssetHierarchy.MaxElements);
        }
        public AdminConfiguration Configuration { get; set; }
        public AFElement Elements { get; set; }
        public List<AFElementTemplate> ElementTemplates { get; set; }
        public List<AFEventFrameTemplate> EventFrameTemplates { get; set; }
        public AFEventFrame FindEventFrameById(Guid id)
        {
            var guidString = id.ToString().Replace("-", "");
            var elSearchKey = guidString.Substring(0, 7);
            var start = new DateTime(int.Parse(guidString.Substring(7, 2)), int.Parse(guidString.Substring(9, 2)), 1).AddYears(1970);

            var element = FindNodeByKey(Elements, elSearchKey, new List<string>());

            if (element == null)
            {
                return null;
            }

            var eventframes = GetElementEventFrames(element, HelperUtils.DateTimeToUnix(start), HelperUtils.DateTimeToUnix(start.AddDays(31)), EventFrameTemplates);

            var eventframe = eventframes.Find(ef => ef.Id == id);

            if (eventframe == null)
            {
                throw new Exception("Eventframe not found");
            }

            return eventframe;
        }
        public List<AFAnalysis> GenerateAnalysis(AFElement element)
        {
            var result = new List<AFAnalysis>();

            var seed = IdUtils.GetSeed(new List<string> {
                    element.Template.Id.ToString(),
                    "eventframe-template"
                }
            );
            var r = new Random(seed);

            //Basis settings
            int nbrAnalysis = 0;
            int basePeriod = 0;
            var eventframeClass = EventFrameClass.Parent;
            AFEventFrameTemplate template = null;

            // Real-time behavior is set to a fixed period with a duration of 5 min
            if (element.Categories.Contains("realtime"))
            {
                nbrAnalysis = element.Categories.Contains("paralell") ? 2 : 1;
                basePeriod = 5 * 60 * 1000;
                template = EventFrameTemplates[r.Next(0, (Configuration.Version > 2) ? (EventFrameTemplates.Count - 1) : 0)];
            }
            else
            {
                nbrAnalysis = ConfigUtils.SampleWithWeights(new List<int> { 0, 1, 2, 3 }, new List<double> { 0.1, 0.75, 0.1, 0.05 }, r);
                //Backwards compatibility with old "incorrect logic"
                template = EventFrameTemplates[r.Next(0, Configuration.Version > 2 ? EventFrameTemplates.Count - 1 : 0)];
                //Base period is defined in seconds but needs to be used in milliseconds
                basePeriod = ConfigUtils.SampleWithWeights(Configuration.Settings.EventframeContext.BasePeriods, Configuration.Settings.EventframeContext.BasePeriodsWeights, r) * 1000;
            }

            for (var i = 0; i < nbrAnalysis; i++)
            {
                if (Configuration.Version > 2 && i > 0)
                {
                    template = EventFrameTemplates[r.Next(0, Configuration.Version > 2 ? EventFrameTemplates.Count - 1 : 0)];
                }

                var val = r.NextDouble();
                bool eventframeVariation = val > 0.9 || (val < 0.1 && Configuration.Version > 2 && !element.Categories.Contains("realtime"));

                //The 2nd condition is for downwards compatibility
                if (i > 0 && Configuration.Version > 2 || i > 1)
                {
                    eventframeClass = r.NextDouble() > 0.5 || element.Categories.Contains("paralell") ? EventFrameClass.Concurent : EventFrameClass.Child;
                }

                result.Add(
                    new AFAnalysis(
                        template: template,
                        basePeriod: basePeriod,
                        addNoise: eventframeVariation,
                        eventframeClass: eventframeClass
                    )
                );
            }

            return result;
        }
        public List<AFEventFrame> GetElementEventFrames(AFElement element, long unixstart, long unixend, List<AFEventFrameTemplate> templates)
        {
            var analysis = GenerateAnalysis(element);

            analysis = analysis
                .GroupBy(x => new { x.AddNoise, x.BasePeriod, x.EventFrameClass, x.Template.Id })
                .Select(g => g.First())
                .ToList();

            return analysis.SelectMany(a => a.GetAnalysisEventFrames(unixstart, unixend, element)).ToList();
        }
        public AFElement FindNodeById(AFElement root, string nodeId)
        {
            if (root.NodeId.ToUpper() == nodeId.ToUpper())
                return root;

            foreach (var child in root.Children)
            {
                var foundNode = FindNodeById(child, nodeId);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }
        private AFElement FindNodeByKey(AFElement root, string key, List<string> ignoreNodes)
        {
            if (root.NodeId.ToUpper().StartsWith(key.ToUpper()) && !ignoreNodes.Contains(root.NodeId))
                return root;

            foreach (var child in root.Children)
            {
                var foundNode = FindNodeByKey(child, key, ignoreNodes);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }
        public List<AFAttribute> FindAttributesByWebId(AFElement root, List<string> webIds)
        {
            var deviceIds = webIds.Select(webId => WebIdEncoding.Id2NodeId(WebIdEncoding.WebId2ElementId(webId))).Distinct().ToList();

            var attributes = new List<AFAttribute>();

            foreach (var deviceId in deviceIds)
            {
                var node = FindNodeById(root, deviceId);

                if (node != null)
                {
                    attributes.AddRange(node.Attribtues.Where(x => webIds.Contains(x.WebId)).ToList());
                }
            }

            return attributes;
        }
        private List<AFElementTemplate> GenerateElementTemplates()
        {
            var result = new List<AFElementTemplate>();

            var r = new Random(IdUtils.GetSeed(new List<string> { Configuration.SystemId }));

            for (var i = 0; i < r.Next(2, Configuration.Settings.AssetHierarchy.NumberOfElementTempaltes); i++)
            {
                var id = IdUtils.GenerateGuid(r);
                result.Add(
                    new AFElementTemplate(id, Configuration)
                 );
            }

            return result;
        }
        private List<AFEventFrameTemplate> GenerateEventFrameTemplates()
        {
            var result = new List<AFEventFrameTemplate>();

            var r = new Random(
                IdUtils.GetSeed(
                    new List<string> {
                        Configuration.SystemId,
                        "eventframe-templates"
                    })
                );

            for (var i = 0; i < r.Next(2, Configuration.Settings.AssetHierarchy.NumberOfElementTempaltes); i++)
            {
                var id = IdUtils.GenerateGuid(r);
                result.Add(
                    new AFEventFrameTemplate(id, Configuration)
                 );
            }

            return result;
        }
        public List<AFElement> FindPathToRoot(AFElement root, string nodeId)
        {
            var path = new List<AFElement>();
            var currentNode = FindNodeById(root, nodeId);

            if (currentNode == null)
            {
                return path;
            }

            path.Add(currentNode);

            while (currentNode.NodeId != root.NodeId)
            {
                currentNode = FindParentById(root, currentNode.NodeId);

                path.Add(currentNode);

                if (currentNode == root)
                    break;
            }

            path.Reverse();
            return path;
        }

        public List<AFElement> FindSiblings(AFElement root, string nodeId)
        {
            var siblings = new List<AFElement>();

            if (nodeId == root.NodeId)
            {
                return siblings;
            }

            var targetNode = FindNodeById(root, nodeId);
            if (targetNode == null)
            {
                return siblings;
            }

            var parentNode = FindParentById(root, nodeId);
            if (parentNode == null)
            {
                return siblings;
            }

            siblings.AddRange(parentNode.Children);

            return siblings;
        }

        public object FindNodeByPath(AFElement root, List<string> path, int level = 0)
        {
            if (root == null || path == null || path.Count == 0)
                return null;

            var currentName = path[level];

            if (!string.Equals(root.Name, currentName, StringComparison.OrdinalIgnoreCase))
                return null;

            if (level == path.Count - 1)
                return root;

            var nextName = path[level + 1];

            foreach (var child in root.Children)
            {
                var found = FindNodeByPath(child, path, level + 1);
                if (found != null)
                    return found;
            }

            if (root.Children.Count == 0 && root.Attribtues.Any() == true)
            {
                var attr = root.Attribtues
                    .FirstOrDefault(a => string.Equals(a.Name, nextName, StringComparison.OrdinalIgnoreCase));

                if (attr != null && level + 1 == path.Count - 1)
                    return attr;
            }

            return null;
        }

        public AFElement FindParentById(AFElement root, string nodeId)
        {
            if (root.Children.Any(x => x.NodeId.ToUpper() == nodeId.ToUpper()))
                return root;

            foreach (var child in root.Children)
            {
                var foundNode = FindParentById(child, nodeId);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }
        public void ExportAssetHierarchy()
        {
            string filePath = "elements.txt";
            string filePath2 = "element-attributes.txt";

            // Write tree structure to file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                using (StreamWriter writer2 = new StreamWriter(filePath2))
                {
                    writer.WriteLine($"Name,Description,WebId,IsDevice,TemplateId,Categories");
                    writer2.WriteLine($"Name,WebId,UoM,ValueType,Description,Path,Function");
                    ExportElement(writer, Elements, "", writer2);
                }
            }
        }

        private static void ExportElement(StreamWriter writer, AFElement node, string prefix = "", StreamWriter attributeWriter = null)
        {
            // Write current node to file
            writer.WriteLine($"{node.Name},{node.Description},{node.WebId},{node.IsDevice},{(node.Template == null ? "None" : node.Template.Id.ToString())},{string.Join('|', node.Categories)}");

            if (attributeWriter != null)
            {
                ExportElementAttributes(attributeWriter, node, "");
            }

            // Recursively write children
            foreach (var child in node.Children)
            {
                ExportElement(writer, child, prefix + "-", attributeWriter);
            }
        }

        private static void ExportElementAttributes(StreamWriter writer, AFElement node, string prefix = "")
        {
            // Recursively write children
            foreach (var att in node.Attribtues)
            {
                writer.WriteLine($"{att.Name},{att.WebId},{att.UoM},{att.Type},{att.Description},{att.Path},{att.ActiveDataGeneratorFunctionString(0)}");
            }
        }

    }
}
