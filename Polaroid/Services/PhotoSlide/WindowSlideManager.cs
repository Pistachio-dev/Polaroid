using Polaroid.Services.Image;
using Polaroid.Windows;
using System.Diagnostics;

namespace Polaroid.Services.PhotoSlide
{
    public class WindowSlideManager
    {
        private const int PhotoSlideDurationMs = 2847;
        private const int PhotoShowDurationMs = 4000;
        private Stopwatch stopwatch = new Stopwatch();
        private PhotoPrintWindow window;
        private PhotoSlideState photoSlideState = PhotoSlideState.NotShowing;
        private float windowHeight;

        public WindowSlideManager(PhotoPrintWindow window)
        {
            this.window = window;
            this.windowHeight = window.Height;
        }

        public void StartSlide(bool startTimer = true)
        {
            if (startTimer)
            {
                stopwatch.Restart();
            }

            if (ScreenshotService.LastPaddedPhotoPath == null)
            {
                Plugin.Framework.RunOnTick(() => StartSlide(false));
            }
            window.SetPicturePath(ScreenshotService.LastPaddedPhotoPath);
            window.IsOpen = true;            
            photoSlideState = PhotoSlideState.SlidingIn;            
            TickSlide();
        }

        private void TickSlide()
        {
            if (photoSlideState == PhotoSlideState.SlidingIn)
            {
                var factor = stopwatch.ElapsedMilliseconds / (float)PhotoSlideDurationMs;
                if (factor > 1)
                {
                    factor = 1;
                    photoSlideState = PhotoSlideState.FullyShown;
                    stopwatch.Restart();
                }

                window.Position = new System.Numerics.Vector2(0, -windowHeight + windowHeight * factor);
                Plugin.Framework.RunOnTick(TickSlide);
                return;
            }

            if (photoSlideState == PhotoSlideState.FullyShown && stopwatch.ElapsedMilliseconds >= PhotoShowDurationMs)
            {
                stopwatch.Reset();
                photoSlideState = PhotoSlideState.NotShowing;
                window.IsOpen = false;
                return;
            }

            Plugin.Framework.RunOnTick(TickSlide);

        }

    }
}
