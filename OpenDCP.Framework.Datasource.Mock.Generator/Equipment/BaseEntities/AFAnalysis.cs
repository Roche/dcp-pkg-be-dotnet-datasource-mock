using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using System;
using System.Collections.Generic;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities
{
    public enum EventFrameClass
    {
        Parent = 1,
        Child = 2,
        Concurent = 3,
    }

    public class AFAnalysis
    {
        public
            AFAnalysis(AFEventFrameTemplate template, int basePeriod, bool addNoise = true, EventFrameClass eventframeClass = EventFrameClass.Parent)
        {
            Template = template;
            BasePeriod = basePeriod;
            AddNoise = addNoise;
            EventFrameClass = eventframeClass;
        }
        public AFEventFrameTemplate Template { get; set; }
        public int BasePeriod { get; set; }
        public bool AddNoise { get; set; }
        public EventFrameClass EventFrameClass { get; set; }

        public List<AFEventFrame> GetAnalysisEventFrames(long unixstart, long unixend, AFElement RefElement)
        {
            var result = new List<AFEventFrame>();

            //find closest point in the search period - to start iteration
            var closestBasePeriodStart = unixstart - unixstart % BasePeriod;

            for (var basePeriodStart = closestBasePeriodStart; basePeriodStart <= unixend; basePeriodStart += BasePeriod)
            {
                long startTime = 0;
                long endTime = 0;

                if (AddNoise)
                {
                    // Eventframes in this class very there start and endtimes, resulting in periods with no active eventframes - default behavior

                    //Check if base Period has eventframe or just noise
                    var periodSeed = IdUtils.GetSeed(
                        new List<string> {
                            RefElement.Template.Id.ToString(),
                        (basePeriodStart / BasePeriod).ToString()
                    });

                    var randomPeriod = new Random(periodSeed);

                    if (randomPeriod.NextDouble() > 0.5)
                    {
                        //The base period contains EventFrames - get actual start and end in period
                        startTime = (long)(basePeriodStart + 0.2 * BasePeriod * randomPeriod.NextDouble());
                        endTime = (long)(basePeriodStart + BasePeriod * (1 - 0.2 * randomPeriod.NextDouble()));

                        if (EventFrameClass.Concurent == EventFrameClass)
                        {
                            //A concurent EF should be generated - this is done by adding "padding"
                            startTime = (long)(startTime + 0.5 * BasePeriod);
                            endTime = (long)(endTime + 0.5 * BasePeriod);
                        }

                        if (EventFrameClass.Child == EventFrameClass)
                        {
                            //A child eventframe is created, it has the span of 40% of the parent eventframe
                            endTime = (int)(endTime - 0.6 * (endTime - startTime));
                        }
                    }
                    else
                    {
                        //The base period does not contain eventframes, no further actions are required
                        continue;
                    }
                }
                else
                {
                    //To test actives batches reproducible there is no variation in the batch length for this analysis and a batch is always active
                    if (EventFrameClass.Concurent == EventFrameClass)
                    {
                        startTime = basePeriodStart + (long)(BasePeriod * 0.05);
                        endTime = basePeriodStart + (long)(BasePeriod * 0.8);
                    }
                    else
                    {
                        startTime = basePeriodStart;
                        endTime = RefElement.Categories.Contains("realtime") ? basePeriodStart + (long)(BasePeriod * 0.75) : basePeriodStart + BasePeriod;
                    }

                }

                //Don't create furture efs
                var now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                if (startTime <= now)
                {
                    //An EventFrame starts one milliseconds later to avoid some plotting problems
                    result.Add(
                        new AFEventFrame(RefElement, startTime + 1, endTime, Template)
                    );
                }
            }

            return result;
        }
    }
}
