using OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities;
using System;
using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Utils
{
    public static class DataMethodFilters
    {
        public static List<AFAttributeValue> RecordedDataFilter(List<AFAttributeValue> rawPoints, double compdev, double compmax)
        {
            if (rawPoints.Count < 2)
            {
                throw new ArgumentException("Input vector must have a minimum length of 2");
            }

            // Initialize starting values
            List<double> xComp = new List<double>() { rawPoints[0].Unixtime };
            List<double> yComp = new List<double>() { rawPoints[0].Value };
            double lastXComp = rawPoints[0].Unixtime;
            double lastYComp = rawPoints[0].Value;
            double sMax = double.PositiveInfinity;
            double sMin = double.NegativeInfinity;

            for (int i = 1; i < rawPoints.Count; i++)
            {
                // Calculate the slope (rate of change) between the current raw point and the last compressed point
                double currentSlope = (rawPoints[i].Value - lastYComp) / (rawPoints[i].Unixtime - lastXComp);

                // Update the maximum and minimum slopes based on the current point
                double currentSMax = (rawPoints[i].Value + compdev - lastYComp) / (rawPoints[i].Unixtime - lastXComp);
                double currentSMin = (rawPoints[i].Value - compdev - lastYComp) / (rawPoints[i].Unixtime - lastXComp);

                // If the new slopes exceed the current max/min slopes, update the bounds
                sMax = System.Math.Min(sMax, currentSMax);
                sMin = System.Math.Max(sMin, currentSMin);

                // If the current slope is outside the max/min bounds, compress the point
                if (currentSlope > sMax || currentSlope < sMin)
                {
                    // Add the previous point to the compressed list
                    xComp.Add(rawPoints[i - 1].Unixtime);
                    yComp.Add(rawPoints[i - 1].Value);

                    // Reset the last compressed point to the one just added
                    lastXComp = rawPoints[i - 1].Unixtime;
                    lastYComp = rawPoints[i - 1].Value;

                    // Reset the max and min slopes for the new point
                    sMax = double.PositiveInfinity;
                    sMin = double.NegativeInfinity;
                }
            }

            // Add the final point if it wasn't already added
            if (!xComp.Contains(rawPoints[rawPoints.Count - 1].Unixtime))
            {
                xComp.Add(rawPoints[rawPoints.Count - 1].Unixtime);
                yComp.Add(rawPoints[rawPoints.Count - 1].Value);
            }


            // Combine results into a list of tuples
            List<AFAttributeValue> compressedPoints = new List<AFAttributeValue>();
            for (int i = 0; i < xComp.Count; i++)
            {
                compressedPoints.Add(new AFAttributeValue
                {
                    Value = yComp[i],
                    Unixtime = (long)xComp[i],
                    Good = true
                }
                );
            }

            return compressedPoints;
        }
        public static List<AFAttributeValue> PlotDataFilter(List<AFAttributeValue> rawPoints, int intervals)
        {
            return rawPoints;
        }
    }
}
