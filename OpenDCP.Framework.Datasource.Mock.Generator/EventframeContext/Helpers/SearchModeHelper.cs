using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using OpenDCP.Framework.DataTypes.Equipment.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Generator.EventframeContext.Helpers
{
    public static class SearchModeHelper
    {
        public static IEnumerable<AFEventFrame> FilterBySearchMode(
            this IEnumerable<AFEventFrame> eventframes,
            DateTime rangeStart,
            DateTime rangeEnd,
            EventframeSearchMode searchMode)
        {
            var maxThreshold = DateTime.MaxValue.AddDays(-1);

            return searchMode switch
            {
                EventframeSearchMode.StartInclusive => eventframes.Where(ef =>
                    ef.StartTime >= rangeStart && ef.StartTime <= rangeEnd),

                EventframeSearchMode.EndInclusive => eventframes.Where(ef =>
                    ef.EndTime >= rangeStart && ef.EndTime <= rangeEnd),

                EventframeSearchMode.Inclusive => eventframes.Where(ef =>
                    ef.StartTime >= rangeStart && ef.StartTime <= rangeEnd &&
                    ef.EndTime >= rangeStart && ef.EndTime <= rangeEnd),

                EventframeSearchMode.Overlapped => eventframes.Where(ef =>
                    ef.StartTime >= rangeStart && ef.StartTime <= rangeEnd ||
                    ef.EndTime >= rangeStart && ef.EndTime <= rangeEnd ||
                    ef.StartTime <= rangeStart && ef.EndTime >= rangeEnd),

                EventframeSearchMode.InProgress => eventframes.Where(ef =>
                    ef.StartTime >= rangeStart &&
                    ef.StartTime <= rangeEnd &&
                    ef.EndTime >= maxThreshold),

                _ => eventframes
            };
        }
    }
}
