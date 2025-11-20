using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using EmbedIO.Utilities;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Photo;
using InputInjection;
using Penumbra.Import.Textures;
using Polaroid.Services.Penumbra;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace Polaroid.Services.Image
{
    public unsafe static class ScreenshotService // TODO: Make it inherit IDisposable
    {
        public static string? LastScreenshotPath = null;
        public static DateTime LastScreenshotDate => UnixTimeStampToLocalDateTime(ScreenShot.Instance()->ScreenShotTimestamp);
        private const int ScreenshotRetries = 10;
        private const int ScreenshotRetryFrameDelay = 5;        

        static unsafe ScreenshotService()
        {
        }

        // The callback receives the full path of the screenshot.
        public static unsafe void TakeScreenshot(Action callback)
        {
            InputFaker.PressScreenshotKey();
            Plugin.Framework.RunOnTick(() => TakeScreenshotWithRetry(callback), delayTicks: ScreenshotRetryFrameDelay);
        }

        private static unsafe void TakeScreenshotWithRetry(Action callback, int retryCounter = 0)
        {
            InputFaker.PressScreenshotKey();
            if (retryCounter >= ScreenshotRetries)
            {
                Plugin.Log.Warning("Screenshot retrieval failed!");
            }

            string screenshotRoute = ScreenShot.Instance()->ThreadPtr->ScreenShotStorageDirectory.ToString();
            long timeStamp = ScreenShot.Instance()->ScreenShotTimestamp;
            DateTime scTime = UnixTimeStampToLocalDateTime(timeStamp);
            if (DateTime.Now - scTime > TimeSpan.FromSeconds(10))
            {
                Log.Verbose("Screenshot not yet taken. Timestamp is old.");
                TakeScreenshotWithRetry(callback, retryCounter++);
            }

            string fileName = GetScreenshotFileName(scTime);
            string? fullPath = Directory.EnumerateFiles(screenshotRoute, fileName,
                new EnumerationOptions() { MaxRecursionDepth = 0, MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = false })
                .FirstOrDefault();
            if (fullPath == null)
            {
                Log.Verbose($"Screenshot not yet found. Retrying ({retryCounter})");
                TakeScreenshotWithRetry(callback, retryCounter++);
            }
            else
            {
                fullPath = fullPath.Replace("/", "\\");
                LastScreenshotPath = fullPath;
                Plugin.Log.Info("Retrieved path: " + fullPath);
                callback();
            }
        }

        public static void GeneratePhotoTexture()
        {
            WaitUntilScreenshotReadable(() => ConvertToPolaroidLook(), 0);
        }
        private static void ConvertToPolaroidLook()
        {
            string? screenshotPath = LastScreenshotPath;
            if (screenshotPath == null)
            {
                Log.Error("Screenshot path null when trying to generate texture!");
                return;
            }
            using ImageSharpImage img = ImageSharpImage.Load<Rgba32>(screenshotPath);
            Console.WriteLine($"Loaded picture with dimensions {img.Width} {img.Height}");

            // crop
            int cropRectangleStart = (img.Width - img.Height) / 2;
            var ratio = img.Width / img.Height;
            int borderWidth = img.Height / 20;
            Rectangle cropRectangle = new Rectangle(new Point(cropRectangleStart, 0), new Size(img.Width - 2 * cropRectangleStart - borderWidth, img.Height));
            int heightWithBorder = img.Height + borderWidth;
            int widthWithBorder = img.Height + borderWidth;

            int resizeSize = heightWithBorder >= 1024 ? 1024 : 512;
            using (Image<Rgba32> dest = (Image<Rgba32>)img.Clone(x =>
                x
                .Crop(cropRectangle)
                //.Pad(widthWithBorder, heightWithBorder, Color.White)
                .Resize(new Size(resizeSize, resizeSize))))
            {
                string textureFolderDir = PenumbraModManager.GetTextureFolder();
                string polaroidFormatScreenshotFileName = Path.GetFileNameWithoutExtension(LastScreenshotPath) + ".png";
                Directory.CreateDirectory(Path.Combine(textureFolderDir, "Photos"));
                string pathSquarePhoto = Path.Combine(textureFolderDir, "Photos", polaroidFormatScreenshotFileName);
                Plugin.Log.Info("Square photo path: " + pathSquarePhoto);
                dest.Save(pathSquarePhoto);
                string newTexturePath = PenumbraModManager.GetNewTextureFullPath();
                PenumbraModManager.ModifyPhotographFileRoute();
                Plugin.Log.Info(newTexturePath);
                //string resultGuid2 = Guid.NewGuid().ToString();
                //string texPath = Path.Combine(screenshotDir, $"{resultGuid2}.tex");
                BaseImage baseImage = new BaseImage(dest);
                var tm = new TextureManager(Plugin.DataManager, new OtterGui.Log.Logger(), Plugin.TextureProvider, Plugin.PluginInterface.UiBuilder);
                //(byte[] rgba, int width, int height) = baseImage.GetPixelData();
                tm.SaveAs(CombinedTexture.TextureSaveType.BC7, false, true, pathSquarePhoto, newTexturePath).Wait();
                PenumbraModManager.ReloadMod();
                //tm.SavePng(baseImage, texPath, rgba, width, height).Wait();
            }            
        }

        private static void WaitUntilScreenshotReadable(Action callback, int retryCounter)
        {
            if (LastScreenshotPath == null)
            {
                Log.Error("Last screenshot path is not set!");
                return;
            }

            if (retryCounter >= ScreenshotRetries)
            {
                Log.Error("Exceeded retry maximum when trying to convert screenshot to .tex");
            }

            bool inUse = Utilities.IsFileInUse(LastScreenshotPath);
            if (!inUse)
            {
                callback();
                return;
            }

            Plugin.Framework.RunOnTick(() => WaitUntilScreenshotReadable(callback, retryCounter++), delayTicks: ScreenshotRetryFrameDelay);
        }

        private static string GetScreenshotFileName(DateTime scTime)
        {
            StringBuilder s = new StringBuilder();
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
