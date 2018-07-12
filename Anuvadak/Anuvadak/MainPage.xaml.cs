using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Anuvadak
{
    public static class Globals
    {
        public static float TextFontSize;
        public static Boolean DoFillBox;
        public static Boolean UseGoogleTranslation;
        public static SKColor TextColor;

        public static Dictionary<string, SKColor> nameToColor = new Dictionary<string, SKColor>
        {
            { "Aqua", SKColors.Aqua }, { "Black", SKColors.Black },
            { "Blue", SKColors.Blue }, { "GhostWhite", SKColors.GhostWhite },
            { "Gray", SKColors.Gray }, { "Green", SKColors.Green },
            { "Lime", SKColors.Lime }, { "Maroon", SKColors.Maroon },
            { "Navy", SKColors.Navy }, { "Olive", SKColors.Olive },
            { "Purple", SKColors.Purple }, { "Red", SKColors.Red },
            { "Silver", SKColors.Silver }, { "Teal", SKColors.Teal },
            { "White", SKColors.White }, { "Yellow", SKColors.Yellow }
        };
    }

    public partial class MainPage : ContentPage
    {

        private MediaFile photo;
        private bool _isBusy;

        public bool IsVeryBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                //OnPropertyChanged(); <-- didn't work
                BusyIndicator.IsVisible = _isBusy;
            }
        }

        public MainPage()
        {
            InitializeComponent();
            Globals.UseGoogleTranslation = false;
            Globals.TextFontSize = 64;
            Globals.DoFillBox = false;
            foreach(string colorName in Globals.nameToColor.Keys)
            {
                ColorPicker.Items.Add(colorName);
            }
            Globals.TextColor = SKColors.Cyan;
        }

        private async void BtnCamera_Clicked(object sender, EventArgs e)
        {
            IsVeryBusy = true;
            try
            {
                await CrossMedia.Current.Initialize();
                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("No Camera?", "No camera avaialble or permission to access.", "OK");
                    IsVeryBusy = false;
                    return;
                }
                photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    AllowCropping = true, //doesn't work with Android
                    Directory = "Anuvadak",
                    SaveToAlbum = true,
                    DefaultCamera = CameraDevice.Rear

                });
                
                await TranslateAndLoad(photo);
            }
            catch (Exception ex)
            {
                lblReaderText.Text = ex.Message;
                lblReaderText.Text += ex.StackTrace;
            }
            finally
            {
                IsVeryBusy = false;
            }
        }

        private async void BtnGallery_Clicked(object sender, EventArgs e)
        {
            IsVeryBusy = true;
            try
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert("Photos Not Supported", "No Permission to access Gallery.", "OK");
                    IsVeryBusy = false;
                    return;
                }
                photo = await CrossMedia.Current.PickPhotoAsync();
                await TranslateAndLoad(photo);
            }
            catch (Exception ex)
            {
                lblReaderText.Text = ex.Message;
                lblReaderText.Text += ex.StackTrace;
            }
            finally
            {
                IsVeryBusy = false;
            }
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            Globals.UseGoogleTranslation = e.Value;
        }

        private async void BtnReload_Clicked(object sender, EventArgs e)
        {
            IsVeryBusy = true;
            try
            {
                await TranslateAndLoad(photo);
            }
            catch (Exception ex)
            {
                lblReaderText.Text = ex.Message;
                lblReaderText.Text += ex.StackTrace;
            }
            finally
            {
                IsVeryBusy = false;
            }
        }

        private async Task TranslateAndLoad(MediaFile photo)
        {
            if (photo == null)
                return;

            SKBitmap bitmap = SKBitmap.Decode(photo.GetStream());
            SKCanvas canvas = new SKCanvas(bitmap);
            SKImage imageSK = SKImage.FromBitmap(bitmap);
            imgViewer.Source = (SKImageImageSource)imageSK;

            var text = OCR.MakeRequest(photo.GetStream(), canvas);
            string response = await text;
            lblReaderText.Text = response;

            //reload image with text
            imageSK = SKImage.FromBitmap(bitmap);
            imgViewer.Source = (SKImageImageSource)imageSK;
        }
        
        private void TxtMode_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                ImgScroller.IsVisible = false;
                txtScroller.IsVisible = true;
            }
            else
            {
                ImgScroller.IsVisible = true;
                txtScroller.IsVisible = false;
            }
        }

        private void FontSize_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Globals.TextFontSize = (float) e.NewValue;
            FontSizeLabel.Text = Globals.TextFontSize.ToString();
        }

        private void Fill_Toggled(object sender, ToggledEventArgs e)
        {
            Globals.DoFillBox = e.Value;
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.TextColor = Globals.nameToColor[ColorPicker.Items[ColorPicker.SelectedIndex]];
        }
    }
}
