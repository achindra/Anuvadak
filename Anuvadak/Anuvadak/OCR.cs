using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using OCRResponse;
using SkiaSharp;
using TextTranslator;
using Xamarin.Forms;

namespace Anuvadak
{
    class OCR
    {
        readonly static string uri = "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/ocr?language=unk&detectOrientation=true";
        public static async Task<string> MakeRequest(Stream stream, SKCanvas canvas)
        {
            HttpResponseMessage response;
            string responseText;
            byte[] byteData;

            using (HttpClient client = new HttpClient())
            {

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    byteData = memoryStream.ToArray();
                }

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // Request headers
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", (string)Application.Current.Resources["BingImageSearchKey"]);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);
                    responseText = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonResponse.FromJson(responseText);

                    StringBuilder sb = new StringBuilder();
                    SKRect area = new SKRect();
                    StringBuilder allTranslatedText = new StringBuilder();

                    SKPaint textBrush = new SKPaint
                    {
                        TextSize = Globals.TextFontSize,
                        IsAntialias = true,
                        Color = Globals.TextColor,
                        Style = SKPaintStyle.StrokeAndFill,
                    };
                    textBrush.Typeface = SKFontManager.Default.MatchCharacter('अ');  //Somehow this is not working with Google results
                    if(null == textBrush.Typeface)
                        SKTypeface.FromFamilyName("Arial");
                    
                    SKPaint drawBrush = new SKPaint
                    {
                        IsAntialias = true,
                        Color = SKColors.DarkGray,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 5
                    };

                    if (Globals.DoFillBox)
                        drawBrush.Style = SKPaintStyle.StrokeAndFill;
                    else
                        drawBrush.Style = SKPaintStyle.Stroke;

                    foreach (Region region in jsonResponse.Regions)
                    {
                        string[] box = region.BoundingBox.Split(',');
                        area = SKRect.Create(float.Parse(box[0]), float.Parse(box[1]), float.Parse(box[2]), float.Parse(box[3]));
                        
                        canvas.DrawRect(float.Parse(box[0]), float.Parse(box[1]), float.Parse(box[2]), float.Parse(box[3]), drawBrush);

                        //TODO: revisit and fix this. Orientation is not working
                        switch (jsonResponse.Orientation.ToLower())
                        {
                            case "down":
                                canvas.RotateDegrees(180 + (float)jsonResponse.TextAngle);
                                break;
                            case "left":
                                canvas.RotateDegrees(90 + (float)jsonResponse.TextAngle);
                                break;
                            case "right":
                                canvas.RotateDegrees(-90 + (float)jsonResponse.TextAngle);
                                break;
                            case "up":
                                canvas.RotateDegrees((float)jsonResponse.TextAngle);
                                break;
                            default:
                                break;
                        }

                        foreach (OCRResponse.Line line in region.Lines)
                        {
                            foreach (Word word in line.Words)
                            {
                                sb.Append(word.Text); sb.Append(" ");
                            }
                            sb.AppendLine(); sb.Append(" ");
                        }
                        sb.AppendLine();
                        string translatedText = await Translator.Translate(sb.ToString(), Globals.UseGoogleTranslation);
                        TextDrawing.DrawText(canvas, translatedText, area, textBrush);
                        sb.Clear();
                        allTranslatedText.Append(translatedText);
                    }

                    

                    return allTranslatedText.ToString();
                }
            }

        }
    }
}
