using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Generic.Generators
{
    public static class IdHelper
    {
        public static Guid NewGuid(List<string> initalizers)
        {
            var nodeId = GenerateHexCode(GetSeed(initalizers));
            var guidString = $"{nodeId.Substring(0, 8)}-{nodeId.Substring(8, 4)}-4{nodeId.Substring(12, 3)}-a{nodeId.Substring(15, 3)}-{nodeId.Substring(18, 12)}";
            Guid.TryParse(guidString, out Guid res);
            return res;
        }
        public static int GetSeed(List<string> initalizers)
        {
            var algo = SHA1.Create();
            byte[] dataAsBytes = initalizers.SelectMany(s =>
            Encoding.UTF8.GetBytes(s + "\r\n")).ToArray();
            var hash = BitConverter.ToInt32(algo.ComputeHash(dataAsBytes), 0);
            return hash;
        }

        public static string GenerateHexCode(int seed)
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
                string hexCode = hexBuilder.ToString().Substring(0, 30);

                return hexCode;
            }
        }
    }
}
