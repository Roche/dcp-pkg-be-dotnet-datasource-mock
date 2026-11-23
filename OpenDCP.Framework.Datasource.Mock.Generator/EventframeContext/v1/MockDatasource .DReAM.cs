using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Generator
{
    public partial class MockDatasource_v_1_0
    {

        public string GetDisplayId()
        {
            return _configuration.Settings.AssetHierarchy.ConfigurationItemAttributeName;
        }
        public string EventFrameId2WebId(string eventFrameId)
        {
            if (string.IsNullOrEmpty(eventFrameId))
            {
                throw new ArgumentNullException(nameof(eventFrameId));
            }

            var systemId = WebIdEncoding.EncodeId(new Guid(_configuration.SystemId));

            var eventFrameEncoded = "I1Fm" + systemId + Convert.ToBase64String(Guid.Parse(eventFrameId).ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 22);

            return eventFrameEncoded;
        }

        public string WebId2EventFrameId(string webId)
        {
            if (string.IsNullOrEmpty(webId))
            {
                throw new ArgumentNullException(nameof(webId));
            }

            var guidPart = webId.Substring(26, 22)
                .Replace("_", "/")
                .Replace("-", "+");

            guidPart += "==";

            var bytes = Convert.FromBase64String(guidPart);
            var guid = new Guid(bytes);

            return guid.ToString();
        }

        public List<string> EventFrameId2WebId(List<string> id)
        {
            return id.Select(x => EventFrameId2WebId(x)).ToList();
        }

        public List<string> WebId2EventFrameId(List<string> webId)
        {
            return webId.Select(x => WebId2EventFrameId(x)).ToList();
        }
    }
}
