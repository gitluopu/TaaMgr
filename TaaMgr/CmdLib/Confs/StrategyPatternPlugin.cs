using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdLib.Confs
{
    namespace StrategyPatternPlugin
    {
        using System;
        using System.Collections.Generic;

        using System.Globalization;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Converters;

        public partial class CStrategyPatternPlugin
        {
            [JsonProperty("config")]
            public Config Config { get; set; }

            [JsonProperty("dllname")]
            public string Dllname { get; set; }

            [JsonProperty("classname")]
            public string Classname { get; set; }

            [JsonProperty("depends")]
            public string[] Depends { get; set; }
        }

        public partial class Config
        {
            [JsonProperty("FileDir")]
            public string FileDir { get; set; }

            [JsonProperty("StrategyThreadsNum")]
            public long StrategyThreadsNum { get; set; }

            [JsonProperty("queue_full_blocking")]
            public long QueueFullBlocking { get; set; }

            [JsonProperty("FileMaxInfoNum")]
            public long FileMaxInfoNum { get; set; }

            [JsonProperty("FileName")]
            public string FileName { get; set; }

            [JsonProperty("isNeedSaveFile")]
            public long IsNeedSaveFile { get; set; }

            [JsonProperty("flow_direction")]
            public string FlowDirection { get; set; }

            [JsonProperty("to_file")]
            public string ToFile { get; set; }

            [JsonProperty("trigger_mode")]
            public string TriggerMode { get; set; }

            [JsonProperty("kafka_audit")]
            public KafkaAudit KafkaAudit { get; set; }
        }

        public partial class KafkaAudit
        {
            [JsonProperty("topic")]
            public string Topic { get; set; }
        }

        public partial class CStrategyPatternPlugin
        {
            public static CStrategyPatternPlugin FromJson(string json) => JsonConvert.DeserializeObject<CStrategyPatternPlugin>(json, StrategyPatternPlugin.Converter.Settings);
        }

        public static class Serialize
        {
            public static string ToJson(this CStrategyPatternPlugin self) => JsonConvert.SerializeObject(self, StrategyPatternPlugin.Converter.Settings);
        }

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Formatting = Formatting.Indented,
                Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }
    }
}
