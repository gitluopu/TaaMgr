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
    //    using FmeNetworkAnalysis;
    //
    //    var cFmeNetworkAnalysis = CFmeNetworkAnalysis.FromJson(jsonString);

    namespace FmeNetworkAnalysis
    {
        using System;
        using System.Collections.Generic;

        using System.Globalization;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Converters;

        public partial class CFmeNetworkAnalysis
        {
            [JsonProperty("dllname")]
            public string Dllname { get; set; }

            [JsonProperty("classname")]
            public string Classname { get; set; }

            [JsonProperty("depends")]
            public string[] Depends { get; set; }

            [JsonProperty("config")]
            public Config Config { get; set; }
        }

        public partial class Config
        {
            [JsonProperty("socket_request_max_bytes")]
            public long SocketRequestMaxBytes { get; set; }

            [JsonProperty("pushinterval")]
            public long Pushinterval { get; set; }

            [JsonProperty("resetnetworkinterval")]
            public long Resetnetworkinterval { get; set; }
        }

        public partial class CFmeNetworkAnalysis
        {
            public static CFmeNetworkAnalysis FromJson(string json) => JsonConvert.DeserializeObject<CFmeNetworkAnalysis>(json, FmeNetworkAnalysis.Converter.Settings);
        }

        public static class Serialize
        {
            public static string ToJson(this CFmeNetworkAnalysis self) => JsonConvert.SerializeObject(self, FmeNetworkAnalysis.Converter.Settings);
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
