using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Google.Cloud.Translation.V2;
using Xamarin.Forms;

namespace TextTranslator
{
    class Translator
    {
        public async static Task<string> Translate(string text, Boolean useGoogleTranslation)
        {
            if (text.Trim() == "")
                return text;

            try
            {
                if (useGoogleTranslation)
                {
                    return await GoogTranslator.TranslateTextAsync(text, LanguageCodes.Hindi);
                }
                else
                {
                    return await BingTranslator.TranslateTextAsync(text, LanguageCodes.Hindi);
                }
            }
            catch (Exception ex)
            {
                return ex.Message + " \nSTACK:\n " + ex.StackTrace;
            }
        }
    }


    internal static class GoogTranslator
    {
        public static async Task<string> TranslateTextAsync(string text, string lang)
        {
            TranslationClient gClient = TranslationClient.CreateFromApiKey((string)Application.Current.Resources["GoogTranslationKey"]);
            TranslationResult result = await gClient.TranslateTextAsync(text, lang);
            return result.TranslatedText;
        }
    }
    internal static class BingTranslator
    {
        static readonly string uri = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=";
        public static async Task<string> TranslateTextAsync(string text, string lang)
        {
            System.Object[] body = new System.Object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri + lang);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", (string)Application.Current.Resources["BingTranslationKey"]);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                TranslatedText[] translatedText = TranslatedText.FromJson(responseBody);

                StringBuilder sb = new StringBuilder();
                foreach (TranslatedText textBlock in translatedText)
                {
                    foreach (Translation translation in textBlock.Translations)
                        sb.Append(translation.Text);
                    sb.Append("\n");
                }
                return sb.ToString();
            }
        }
    }

    public partial class TranslatedText
    {
        [JsonProperty("detectedLanguage")]
        public DetectedLanguage DetectedLanguage { get; set; }

        [JsonProperty("translations")]
        public Translation[] Translations { get; set; }
    }

    public partial class DetectedLanguage
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }
    }

    public partial class Translation
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }
    }

    public partial class TranslatedText
    {
        public static TranslatedText[] FromJson(string json) => JsonConvert.DeserializeObject<TranslatedText[]>(json, TextTranslator.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this TranslatedText[] self) => JsonConvert.SerializeObject(self, TextTranslator.Converter.Settings);
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
