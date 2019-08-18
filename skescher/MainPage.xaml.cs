using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SketchPad2
{

    public enum FileFormat
    {
        Jpeg,
        Png,
        Bmp,
        Tiff,
        Gif
    }

    public static class FileFormatExtensions
    {
        public static string ToFileType(this FileFormat format)
        {
            switch (format)
            {
            case FileFormat.Jpeg:
                return "jpeg";
            case FileFormat.Png:
                return "png";
            case FileFormat.Bmp:
                return  "bmp";
            case FileFormat.Tiff:
                return  "tiff";
            case FileFormat.Gif:
                return "gif";
            }
            throw new Exception();
        }

        public static Guid GetBitmapEncoder(this FileFormat format)
        {
            switch (format)
            {
            case FileFormat.Jpeg:
                return BitmapEncoder.JpegEncoderId;
            case FileFormat.Png:
                return BitmapEncoder.PngEncoderId;
            case FileFormat.Bmp:
                return BitmapEncoder.BmpEncoderId;
            case FileFormat.Tiff:
                return BitmapEncoder.TiffEncoderId;
            case FileFormat.Gif:
                return BitmapEncoder.GifEncoderId;
            }
            throw new Exception();
        }

        public static FileFormat FromFileType(string fileType)
        {
            if (fileType.StartsWith('.'))
            {
                fileType = fileType.Substring(1);
            }
            switch (fileType)
            {
            case "jpeg":
            case "jpg":
                return FileFormat.Jpeg;
            case "png":
                return FileFormat.Png;
            case "bmp":
                return FileFormat.Bmp;
            case "tiff":
                return FileFormat.Tiff;
            case "gif":
                return FileFormat.Gif;
            }
            throw new ArgumentException($"Invalid file type {fileType}", "fileType");
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();

            // Enable mouse-based drawing in addition of pen
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Mouse;
        }

        private void cutButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void pasteButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            savePicker.FileTypeChoices.Add("JPEG image", new List<string>() { ".jpeg" });
            savePicker.FileTypeChoices.Add("PNG image", new List<string>() { ".png" });
            savePicker.FileTypeChoices.Add("BMP image", new List<string>() { ".bmp" });
            savePicker.FileTypeChoices.Add("TIFF image", new List<string>() { ".tiff" });
            savePicker.FileTypeChoices.Add("GIF image", new List<string>() { ".gif" });
            savePicker.SuggestedFileName = "sketch";
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var bitmap = GetSignatureBitmapFull(inkCanvas);
                FileFormat format = FileFormatExtensions.FromFileType(file.FileType);
                WriteableBitmapToStorageFile(bitmap, file, format);
            }
        }

        private byte[] ConvertInkCanvasToByteArray(InkCanvas canvas)
        {
            var canvasStrokes = canvas.InkPresenter.StrokeContainer.GetStrokes();

            if (canvasStrokes.Count > 0)
            {
                var width = (int)canvas.ActualWidth;
                var height = (int)canvas.ActualHeight;
                var device = CanvasDevice.GetSharedDevice();
                var renderTarget = new CanvasRenderTarget(device, width, height, 96);

                using (var ds = renderTarget.CreateDrawingSession())
                {
                    ds.Clear(Windows.UI.Colors.White);
                    ds.DrawInk(canvas.InkPresenter.StrokeContainer.GetStrokes());
                }

                return renderTarget.GetPixelBytes();
            }
            else
            {
                return null;
            }
        }

        private WriteableBitmap GetSignatureBitmapFull(InkCanvas canvas)
        {
            var bytes = ConvertInkCanvasToByteArray(canvas);
            if (bytes != null)
            {
                var width = (int)canvas.ActualWidth;
                var height = (int)canvas.ActualHeight;

                var bmp = new WriteableBitmap(width, height);
                using (var stream = bmp.PixelBuffer.AsStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    return bmp;
                }
            }
            else
            {
                return null;
            }
        }

        private async void WriteableBitmapToStorageFile(WriteableBitmap bitmap, StorageFile file, FileFormat fileFormat)
        {
            Guid BitmapEncoderGuid = fileFormat.GetBitmapEncoder();
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = bitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                    (uint)bitmap.PixelWidth,
                                    (uint)bitmap.PixelHeight,
                                    96.0,
                                    96.0,
                                    pixels);
                await encoder.FlushAsync();
            }
        }

    }
}
