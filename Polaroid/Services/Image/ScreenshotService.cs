using Dalamud.Game.ClientState.JobGauge.Enums;
using FFXIVClientStructs.FFXIV.Client.System.Photo;
using InputInjection;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace Polaroid.Services.Image
{
    public unsafe static class ScreenshotService // TODO: Make it inherit IDisposable
    {
        public static string? LastScreenshotPath = null;
        private const int ScreenshotRetries = 3;
        private const int ScreenshotRetryFrameDelay = 5;        

        static unsafe ScreenshotService()
        {
        }

        // The callback receives the full path of the screenshot.
        public static unsafe void TakeScreenshot(Action<string> callback)
        {
            InputFaker.PressScreenshotKey();
            Plugin.Framework.RunOnTick(() => HandleOrRetryScreenshot(0, callback), delayTicks: ScreenshotRetryFrameDelay);
        }

        private static unsafe Task HandleOrRetryScreenshot(int retryCounter, Action<string> callback)
        {
            if (retryCounter == ScreenshotRetries)
            {
                Plugin.Log.Warning("Screenshot retrieval failed!");
                return Task.CompletedTask;
            }
            string screenshotRoute = ScreenShot.Instance()->ThreadPtr->ScreenShotStorageDirectory.ToString();
            long timeStamp = ScreenShot.Instance()->ScreenShotTimestamp;
            string fileName = GetScreenshotFileName(timeStamp);
            Log.Information($"Looking for screenshot with timestamp {timeStamp} at {screenshotRoute}");
            Log.Information($"Expected file name: \"{fileName}\"");
            string? fullPath = Directory.EnumerateFiles(screenshotRoute, fileName,
                new EnumerationOptions() { MaxRecursionDepth = 0, MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = false })
                .FirstOrDefault();
            if (fullPath == null)
            {
                HandleOrRetryScreenshot(retryCounter++, callback);
            }
            else
            {                
                fullPath = fullPath.Replace( "/", "\\");
                LastScreenshotPath = fullPath;
                Plugin.Log.Info("Retrieved path: " + fullPath);         
            }

            return Task.CompletedTask;
        }

        public static void ConvertToPolaroidLook(string screenshotDir, string fileName)
        {
            string screenshotPath = Path.Combine(screenshotDir, fileName);
            using ImageSharpImage img = ImageSharpImage.Load<Rgba32>(screenshotPath);
            Console.WriteLine($"Loaded picture with dimensions {img.Width} {img.Height}");

            // crop
            int cropRectangleStart = (img.Width - img.Height) / 2;
            Rectangle cropRectangle = new Rectangle(new Point(cropRectangleStart, 0), new Size(img.Height, img.Height));
            int borderWidth = img.Height / 20;
            int dimensionsWithBorder = img.Height + borderWidth;

            using (Image<Rgba32> dest = (Image<Rgba32>)img.Clone(x => x.Crop(cropRectangle).Pad(dimensionsWithBorder, dimensionsWithBorder, Color.White)))
            {
                string resultGuid = Guid.NewGuid().ToString();
                dest.Save(Path.Combine(screenshotDir, $"{resultGuid}.png"));
            }
            
        }

        private static string GetScreenshotFileName(long timestamp)
        {
            StringBuilder s = new StringBuilder();
            DateTime scTime = UnixTimeStampToLocalDateTime(timestamp);
            s.Append("ffxiv_");
            s.Append(scTime.Day.ToString("D2"));
            s.Append(scTime.Month.ToString("D2"));
            s.Append(scTime.Year.ToString("D4"));
            s.Append("_");
            s.Append(scTime.Hour.ToString("D2"));
            s.Append(scTime.Minute.ToString("D2"));
            s.Append(scTime.Second.ToString("D2"));
            s.Append("_");
            s.Append(scTime.Millisecond.ToString("D3"));
            s.Append(".");
            string extension = ScreenShot.Instance()->ScreenShotFileFormat.ToString().ToLower();
            s.Append(extension);

            return s.ToString();
        }

        private static DateTime UnixTimeStampToLocalDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).DateTime.ToLocalTime();
            return dateTime;
        }

        public static void Dispose()
        {
        }
    }
}
