using System;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Helpers
{
    public static class EnumHelper
    {
        public static T GetRandomEnum<T>(Random random) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(random.Next(values.Length));
        }
    }
}
