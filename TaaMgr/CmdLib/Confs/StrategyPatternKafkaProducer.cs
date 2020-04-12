using System;
using System.Collections.Generic;
using System.Text;

namespace CmdLib.Confs
{
    namespace StrategyPatternKafkaProducer
    {
        using System;
        using System.Collections.Generic;

        using System.Globalization;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Converters;

        public partial class CStrategyPatternKafkaProducer
        {
            [JsonProperty("dllname")]
            public string Dllname { get; set; }

            [JsonProperty("classname")]
            public string Classname { get; set; }

            [JsonProperty("config")]
            public Config Config { get; set; }
        }

        public partial class Config
        {
            [JsonProperty("topic")]
            public string Topic { get; set; }

            [JsonProperty("partition")]
            public long Partition { get; set; }

            [JsonProperty("ssl.key.location")]
            public string SslKeyLocation { get; set; }

            [JsonProperty("ssl.ca.location")]
            public string SslCaLocation { get; set; }

            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("security.protocol")]
            public string SecurityProtocol { get; set; }

            [JsonProperty("ssl.key.password")]
            public string SslKeyPassword { get; set; }

            [JsonProperty("compression.codec")]
            public string CompressionCodec { get; set; }

            [JsonProperty("brokers")]
            public string Brokers { get; set; }

            [JsonProperty("message.max.bytes")]
            public long MessageMaxBytes { get; set; }

            [JsonProperty("queue_full_blocking")]
            public long QueueFullBlocking { get; set; }

            [JsonProperty("queue.buffering.max.kbytes")]
            public long QueueBufferingMaxKbytes { get; set; }

            [JsonProperty("queue.buffering.max.ms")]
            public long QueueBufferingMaxMs { get; set; }

            [JsonProperty("batch.num.messages")]
            public long BatchNumMessages { get; set; }

            [JsonProperty("ssl.certificate.location")]
            public string SslCertificateLocation { get; set; }

            [JsonProperty("cryptoalg")]
            public string Cryptoalg { get; set; }

            [JsonProperty("queue.buffering.max.messages")]
            public long QueueBufferingMaxMessages { get; set; }
        }

        public partial class CStrategyPatternKafkaProducer
        {
            public static CStrategyPatternKafkaProducer FromJson(string json) => JsonConvert.DeserializeObject<CStrategyPatternKafkaProducer>(json, StrategyPatternKafkaProducer.Converter.Settings);
        }

        public static class Serialize
        {
            public static string ToJson(this CStrategyPatternKafkaProducer self) => JsonConvert.SerializeObject(self, StrategyPatternKafkaProducer.Converter.Settings);
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
