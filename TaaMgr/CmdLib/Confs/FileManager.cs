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
    //    using FileManager;
    //
    //    var cFileManager = CFileManager.FromJson(jsonString);

    namespace FileManager
    {
        using System;
        using System.Collections.Generic;

        using System.Globalization;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Converters;

        public partial class CFileManager
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
            [JsonProperty("DiskFreeSpace")]
            public long DiskFreeSpace { get; set; }

            [JsonProperty("TrafficDataRetentionTime")]
            public long TrafficDataRetentionTime { get; set; }

            [JsonProperty("PatternDataRetentionTime")]
            public long PatternDataRetentionTime { get; set; }

            [JsonProperty("CheckInterval")]
            public long CheckInterval { get; set; }

            [JsonProperty("EnableFreeStrategy")]
            public bool EnableFreeStrategy { get; set; }

            [JsonProperty("HoldDiskSpace")]
            public long HoldDiskSpace { get; set; }

            [JsonProperty("HoldRatio")]
            public long HoldRatio { get; set; }
        }

        public partial class CFileManager
        {
            public static CFileManager FromJson(string json) => JsonConvert.DeserializeObject<CFileManager>(json, FileManager.Converter.Settings);
        }

        public static class Serialize
        {
            public static string ToJson(this CFileManager self) => JsonConvert.SerializeObject(self, FileManager.Converter.Settings);
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
