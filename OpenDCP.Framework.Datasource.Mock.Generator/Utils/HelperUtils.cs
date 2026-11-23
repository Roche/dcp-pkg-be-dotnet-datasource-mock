using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Utils
{
    public static class HelperUtils
    {
        public static long ParseDateToUnix(string date)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(date, null, System.Globalization.DateTimeStyles.RoundtripKind);
            return dateTimeOffset.ToUnixTimeMilliseconds();
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }

        public static long DateTimeToUnix(DateTime time)
        {
            return ((DateTimeOffset)time).ToUnixTimeMilliseconds();
        }

        public static string DataTypePromoting(List<string> dataTypes)
        {
            var nonNumericDataTypes = new List<string> { "GUID", "DateTime", "Byte", "String", "EnumerationValue", "Enum" };
            var numericDataTypes = new List<string> { "Double", "Single", "Int64", "Int32", "Int16" };

            var uniqueValues = dataTypes.Distinct().ToList();
            var intersectNonNumeric = uniqueValues.Intersect(nonNumericDataTypes).ToList();
            var intersectNumeric = numericDataTypes.Intersect(uniqueValues).ToList();

            //If all datatypes are the same
            if (uniqueValues.Count == 1)
            {
                return dataTypes.FirstOrDefault();
            }
            //Promoting to String if there is non numeric datatype
            else if (intersectNonNumeric.Count > 0)
            {
                return "String";
            }
            //Promoting to the highest numeric datatype
            else if (numericDataTypes.Count > 0)
            {
                return intersectNumeric.FirstOrDefault();
            }
            else
            {
                throw new Exception("DataType not known");
            }
        }
    }
}
