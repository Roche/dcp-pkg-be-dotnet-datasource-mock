using System;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Utils
{
    public static class WebIdEncoding
    {
        public static string EncodeId(Guid id)
        {
            return Convert.ToBase64String(id.ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 22);
        }

        public static Guid WebId2Id(string webId)
        {
            string elId;
            if (webId.StartsWith("I1Em"))
            {
                elId = webId.Substring(26, 22);
            }
            else if (webId.StartsWith("I1Ab"))
            {
                elId = webId.Substring(27, 22);
            }
            else
            {
                elId = webId.Substring(webId.Length - 22);
            }
            return new Guid(Convert.FromBase64String(elId.Replace("-", "+").Replace("_", "/") + "=="));
        }

        public static Guid WebId2ElementId(string webId)
        {
            string elId;
            if (webId.StartsWith("I1Em"))
            {
                elId = webId.Substring(26, 22);
            }
            else if (webId.StartsWith("I1Ab"))
            {
                elId = webId.Substring(27, 22);
            }
            else
            {
                throw new Exception("Unkown webId Type");
            }
            return new Guid(Convert.FromBase64String(elId.Replace("-", "+").Replace("_", "/") + "=="));
        }

        public static string Id2NodeId(Guid Id)
        {
            var nodeIdString = Id.ToString().Substring(0, 13) + Id.ToString().Substring(15, 3) + Id.ToString().Substring(20, 16);
            return nodeIdString.Replace("-", "");
        }

        public static string WebId2NodeId(string webId)
        {
            if (webId.StartsWith("I1Ab"))
            {
                webId = webId.Substring(27, 22);
            }
            webId = WebId2Id(webId).ToString();
            var nodeIdString = webId.Substring(0, 13) + webId.Substring(15, 3) + webId.Substring(20, 16);
            return nodeIdString.Replace("-", "");
        }
    }
}
