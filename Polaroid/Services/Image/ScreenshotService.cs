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
using System.Text;
using System.Threading.Tasks;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace Polaroid.Services.Image
{
    public static class ScreenshotService // TODO: Make it inherit IDisposable
    {
        public static string? LastScreenshotPath = null;
        public static string? LastPaddedPhotoPath = null;
        public unsafe static DateTime LastScreenshotDate => UnixTimeStampToLocalDateTime(ScreenShot.Instance()->ScreenShotTimestamp);
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
            WaitUntilScreenshotReadable(() => _ = ProcessLastScreenshot(), 0);
        }

        private static async System.Threading.Tasks.Task ProcessLastScreenshot()
        {
            await PenumbraModManager.EnsureModExists();
            
            string? screenshotPath = LastScreenshotPath;
            if (screenshotPath == null)
            {
                Log.Error("Screenshot path null when trying to generate texture!");
            }

            LastPaddedPhotoPath = null; // New one is being built

            var woodTexturePartPath = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, "signWithPoolWoodenTexturePart.png");
            
            using Image<Rgba32> woodTexturePart = await ImageSharpImage.LoadAsync<Rgba32>(woodTexturePartPath);
            Console.WriteLine($"Loaded wooden texture with dimensions {woodTexturePart.Width} {woodTexturePart.Height}");
            
            using ImageSharpImage img = await ImageSharpImage.LoadAsync<Rgba32>(screenshotPath);
            Console.WriteLine($"Loaded picture with dimensions {img.Width} {img.Height}");

            using ImageSharpImage paddedScreenshot = await GeneratePaddedPhoto(img);
            using ImageSharpImage finishedTexture = await ConvertToSignpostTexture(paddedScreenshot, woodTexturePart);

            await UpdateModPictures((Image<Rgba32>)img, (Image<Rgba32>)paddedScreenshot, (Image<Rgba32>)finishedTexture);
        }

        private static async Task<ImageSharpImage> ConvertToSignpostTexture(ImageSharpImage padded, ImageSharpImage woodTexturePart)
        {
            const int fullCompositeWidth = 1920;

            var guid = Guid.NewGuid();
            Plugin.Log.Warning("saving padded picture to " + Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "padded", $"{guid}.png"));
            //await padded.SaveAsync(Path.Combine(PenumbraModManager.GetIntermediatePicturesFolder(), "padded", $"{guid}_padded.png"));
            var result = new Image<Rgba32>(fullCompositeWidth, woodTexturePart.Height);
            result.Mutate(img => img.DrawImage(woodTexturePart, 1).DrawImage(padded, new Point(woodTexturePart.Width, 0), 1));
            //await result.SaveAsync(Path.Combine(PenumbraModManager.GetIntermediatePicturesFolder(), "signboardTexture", $"{guid}.png"));

            return result;
        }

        private static async Task<ImageSharpImage> GeneratePaddedPhoto(ImageSharpImage img)
        {
            const int resultHeightWithBorder = 1236;
            const int resultWidthWithBorder = 1495;
            const int borderWidth = 100;
            int resultHeight = resultHeightWithBorder - borderWidth; // I remove borderWidth to add as white padding
            int resultWidth = resultWidthWithBorder - borderWidth;
            int ratio = resultHeight / img.Height;
            int cropRectangleStart = Math.Max(0, (img.Width * ratio - resultWidth) / 2);
            Rectangle cropRectangle = new Rectangle(new Point(cropRectangleStart, 0), new Size(resultHeight, resultWidth));

            Plugin.Log.Info($"Crop rectangle size: {cropRectangle.Width}x{cropRectangle.Height}");
            Plugin.Log.Info($"Resized image size: {img.Width * ratio}x{img.Height * ratio}");
            Plugin.Log.Info($"Original image size: {img.Width}x{img.Height}");

            using Image<Rgba32> adapted = FitToRectangle(img, new Size(resultWidth, resultHeight));
            return adapted.Clone(img => img.Pad(resultWidthWithBorder, resultHeightWithBorder, Color.White));
        }


        private static Image<Rgba32> FitToRectangle(ImageSharpImage img, Size resultToFillSize)
        {
            var factorH =  resultToFillSize.Height / (float)img.Width;
            var factorV = resultToFillSize.Width / (float)img.Height;
            var dominatingFactor = Math.Max(factorH, factorV);
            Point cropRectangleStart = new Point((int)(img.Width * dominatingFactor - resultToFillSize.Width) / 2,
                (int)(img.Height * dominatingFactor - resultToFillSize.Height) / 2);

            int resizedW = (int)(img.Width * dominatingFactor);
            int resizedH = (int)(img.Height * dominatingFactor);
            var output = img.Clone(i => i.Resize(resizedW, resizedH).Crop(new Rectangle(cropRectangleStart, resultToFillSize)));
            return (Image<Rgba32>)output;
        }

        private static async Task<string> SaveOriginals(Image<Rgba32> originalScreenshot, string subfolder)
        {
            string textureFolderDir = PenumbraModManager.GetTextureFolder();
            string screenshotFileName = $"{Path.GetFileNameWithoutExtension(LastScreenshotPath)}.png";
            
            var dir = Directory.CreateDirectory(Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "Photos", subfolder));
            string screenshotPath = Path.Combine(dir.FullName, screenshotFileName);
            Plugin.Log.Info("Raw photo path: " + screenshotPath);
            await originalScreenshot.SaveAsync(screenshotPath);

            return screenshotPath;
        }

        private static async System.Threading.Tasks.Task UpdateModPictures(Image<Rgba32> originalScreenshot, Image<Rgba32> padded, Image<Rgba32> texture)
        {
            var originalPath = await SaveOriginals(originalScreenshot, "original");
            var paddedPath = await SaveOriginals(padded, "photoprint");
            LastPaddedPhotoPath = paddedPath;
            Plugin.Log.Info("Last padded photo path: " + LastPaddedPhotoPath);
            var textureAsPngPath = await SaveOriginals(texture, "signboardTexture");          

            string newTexturePath = PenumbraModManager.GetNewTextureFullPath();
            await PenumbraModManager.ModifyPhotographFileRoute();
            Plugin.Log.Info("New texture path: " + newTexturePath);

            BaseImage baseImage = new BaseImage(texture);
            var tm = new TextureManager(Plugin.DataManager, new OtterGui.Log.Logger(), Plugin.TextureProvider, Plugin.PluginInterface.UiBuilder);
            tm.SaveAs(CombinedTexture.TextureSaveType.BC7, false, true, textureAsPngPath, newTexturePath).Wait();
            PenumbraModManager.ReloadMod();
            //tm.SavePng(baseImage, texPath, rgba, width, height).Wait();
        }

        public static async void SetModTextureFromPadded(string paddedPictureRoute)
        {
            string newTexturePath = PenumbraModManager.GetNewTextureFullPath();
            await PenumbraModManager.ModifyPhotographFileRoute();

            var textureRoute = paddedPictureRoute.Replace("photoprint", "signboardTexture");
            using ImageSharpImage img = await ImageSharpImage.LoadAsync<Rgba32>(textureRoute);
            var tm = new TextureManager(Plugin.DataManager, new OtterGui.Log.Logger(), Plugin.TextureProvider, Plugin.PluginInterface.UiBuilder);
            tm.SaveAs(CombinedTexture.TextureSaveType.BC7, false, true, textureRoute, newTexturePath).Wait();
            PenumbraModManager.ReloadMod();
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

        private unsafe static string GetScreenshotFileName(DateTime scTime)
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
