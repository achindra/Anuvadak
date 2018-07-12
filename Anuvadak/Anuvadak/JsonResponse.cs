using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OCRResponse
{
    /// <summary>
    /// This class handles JsonResponse from Azure Vision API (OCR)
    /// </summary>
    public partial class JsonResponse
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("orientation")]
        public string Orientation { get; set; }

        [JsonProperty("textAngle")]
        public double TextAngle { get; set; }

        [JsonProperty("regions")]
        public Region[] Regions { get; set; }
    }

    public partial class Region
    {
        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("lines")]
        public Line[] Lines { get; set; }
    }

    public partial class Line
    {
        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("words")]
        public Word[] Words { get; set; }
    }

    public partial class Word
    {
        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public partial class JsonResponse
    {
        public static JsonResponse FromJson(string json) => JsonConvert.DeserializeObject<JsonResponse>(json, OCRResponse.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this JsonResponse self) => JsonConvert.SerializeObject(self, OCRResponse.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
