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

        private void pasteButton_Click(object sender, RoutedEventArgs e)
        {
            var wb = GetSignatureBitmapFull(inkCanvas);
            WriteableBitmapToStorageFile(wb, FileFormat.Png);

            //Size size = new Size(surface.Width, surface.Height);
            //RenderTargetBitmap renderBitmap = new RenderTargetBitmap();
            //renderBitmap.RenderAsync(inkCanvas);


            //(int)size.Width,
            //(int)size.Height,
            //96,
            //96,
            //PixelFormats.Default);
            //renderBitmap.Render(surface);
            //using (FileStream fs = File.Open(path, FileMode.Create))
            //{
            //    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            //    encoder.Save(fs);
            //}
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

        private enum FileFormat
        {
            Jpeg,
            Png,
            Bmp,
            Tiff,
            Gif
        }

        private async Task<StorageFile> WriteableBitmapToStorageFile(WriteableBitmap WB, FileFormat fileFormat)
        {
            string FileName = "MyFile.";
            Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
            switch (fileFormat)
            {
            case FileFormat.Jpeg:
                FileName += "jpeg";
                BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
                break;

            case FileFormat.Png:
                FileName += "png";
                BitmapEncoderGuid = BitmapEncoder.PngEncoderId;
                break;

            case FileFormat.Bmp:
                FileName += "bmp";
                BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;
                break;

            case FileFormat.Tiff:
                FileName += "tiff";
                BitmapEncoderGuid = BitmapEncoder.TiffEncoderId;
                break;

            case FileFormat.Gif:
                FileName += "gif";
                BitmapEncoderGuid = BitmapEncoder.GifEncoderId;
                break;
            }

            var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(FileName, CreationCollisionOption.GenerateUniqueName);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = WB.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                    (uint)WB.PixelWidth,
                                    (uint)WB.PixelHeight,
                                    96.0,
                                    96.0,
                                    pixels);
                await encoder.FlushAsync();
            }
            return file;
        }

    }
}
