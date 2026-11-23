using OpenDCP.Framework.Datasource.Mock.Generator.Utils;
using OpenDCP.Framework.DataTypes.Equipment.Entities.EventframeContext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Equipment.BaseEntities
{
    public class AFEventFrame
    {
        public long Start;
        public long End;

        public AFEventFrameTemplate Template { get; set; }

        public AFEventFrame(AFElement element, long unixstart, long unixend, AFEventFrameTemplate template)
        {
            RefElement = element;
            Start = unixstart;
            End = unixend;

            Template = template;

            SetId();
        }
        public AFElement RefElement { get; set; }
        public List<EventFrameValueDTO> Values
        {
            get
            {
                return GenerateAttributeValues();
            }
        }
        public Guid Id { get; private set; }
        public string RefElementWebId
        {
            get
            {
                return RefElement.WebId;
            }
        }
        public DateTime StartTime
        {
            get
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(Start).UtcDateTime;
            }
        }
        public DateTime EndTime
        {
            get
            {
                var now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                var end = End <= now ? End : DateTimeOffset.MaxValue.ToUnixTimeMilliseconds();

                return DateTimeOffset.FromUnixTimeMilliseconds(end).UtcDateTime;
            }
        }
        public virtual string WebId
        {
            get
            {
                return "I1Fm" + WebIdEncoding.EncodeId(RefElement.Id) + WebIdEncoding.EncodeId(Id);
            }
        }
        public List<string> Categories
        {
            get
            {
                return Template.Categories;
            }
        }

        private List<EventFrameValueDTO> GenerateAttributeValues()
        {
            var attributes = Template.GetAttributeTempaltes();
            return attributes.Select(a =>
            {

                var time = a.TimeMethod == "EndTime" ? End : Start;

                var value = new EventFrameValueDTO
                {
                    Name = a.Name,
                    Good = true
                };


                if (a.Type != "String")
                {
                    var generator = a.NumericDataGeneratorFunction();
                    double dValue = generator(time / 1000);
                    value.Value = dValue;
                    value.Good = !(double.IsNaN(dValue) || double.IsInfinity(dValue));
                }
                else if (!a.Name.Contains("Id") && !a.Name.Contains("ID") && !a.Name.Contains("Order"))
                {
                    var generator = a.StringCategoryDataGeneratorFunction();
                    value.Value = generator(time / 1000);
                }
                else
                {
                    var generator = a.StringDataGeneratorFunction();
                    value.Value = generator(time / 1000);
                }

                return value;
            }).ToList();
        }
        private void SetId()
        {
            //First 7chars use element id
            var seedString = RefElement.Id.ToString().Replace("-", "").Substring(0, 7);

            //4chars for the yy(since 1970) and mm
            seedString += $"{(StartTime.Year - 1970).ToString().Substring(0, 2)}{StartTime.Month.ToString().PadLeft(2).Replace(" ", "0")}";

            var seed = IdUtils.GetSeed(
                new List<string> {
                    RefElement.Id.ToString(),
                    //Keep Ids consitent for downwards compbility
                    ((Start - 1)/1000).ToString(),
                    (End/1000).ToString()
            });

            var r = new Random(seed);
            seedString += IdUtils.GenerateHexId(r, 19);
            seedString = seedString.ToLower();

            var guidString = $"{seedString.Substring(0, 8)}-{seedString.Substring(8, 4)}-4{seedString.Substring(12, 3)}-a{seedString.Substring(15, 3)}-{seedString.Substring(18, 12)}";
            Id = new Guid(guidString);
        }
    }
}