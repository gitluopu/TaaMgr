﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdLib.Confs
{
    // <auto-generated />
    //
    // To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
    //
    //    using LocalRedisClient;
    //
    //    var cLocalRedisClient = CLocalRedisClient.FromJson(jsonString);

    namespace LocalRedisClient
    {
        using System;
        using System.Collections.Generic;

        using System.Globalization;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Converters;

        public partial class CLocalRedisClient
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
            [JsonProperty("host")]
            public string Host { get; set; }

            [JsonProperty("port")]
            public long Port { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }

            [JsonProperty("CheckInterval")]
            public long CheckInterval { get; set; }

            [JsonProperty("MaxDelayMin")]
            public long MaxDelayMin { get; set; }
        }

        public partial class CLocalRedisClient
        {
            public static CLocalRedisClient FromJson(string json) => JsonConvert.DeserializeObject<CLocalRedisClient>(json, LocalRedisClient.Converter.Settings);
        }

        public static class Serialize
        {
            public static string ToJson(this CLocalRedisClient self) => JsonConvert.SerializeObject(self, LocalRedisClient.Converter.Settings);
        }

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }
    }

}
