using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Utils
{
    public static class IdUtils
    {
        public static int GetSeed(List<string> initalizers)
        {
            using (var algo = SHA1.Create())
            {
                // Explicitly hardcode Windows line endings "\r\n" 
                // to maintain 100% byte compatibility with previous Windows output
                byte[] dataAsBytes = initalizers
                    .SelectMany(s => Encoding.UTF8.GetBytes(s + "\r\n"))
                    .ToArray();

                byte[] hashBytes = algo.ComputeHash(dataAsBytes);
                return BitConverter.ToInt32(hashBytes, 0);
            }
        }


        public static Guid GenerateGuid(Random random)
        {
            var nodeId = GenerateHexId(random, 30);
            var guidString = $"{nodeId.Substring(0, 8)}-{nodeId.Substring(8, 4)}-4{nodeId.Substring(12, 3)}-a{nodeId.Substring(15, 3)}-{nodeId.Substring(18, 12)}";
            Guid.TryParse(guidString, out Guid res);
            return res;
        }

        public static string GenerateHexId(Random random, int length = 30)
        {
            const string chars = "0123456789ABCDEF";
            char[] hexArray = new char[length];
            for (int i = 0; i < length; i++)
            {
                hexArray[i] = chars[random.Next(chars.Length)];
            }
            return new string(hexArray);
        }
    }
}
