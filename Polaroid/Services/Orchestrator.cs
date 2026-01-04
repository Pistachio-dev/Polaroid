using Dalamud.Game.ClientState.Objects.SubKinds;
using InputInjection;
using Polaroid.Services.Camera;
using Polaroid.Services.EmoteDetection;
using Polaroid.Services.Image;
using System;
using System.Numerics;

namespace Polaroid.Services
{
    public class Orchestrator
    {
        public Orchestrator(Plugin plugin)
        {
            this.plugin = plugin;
        }
        private readonly Plugin plugin;

        public void OnPhotographEmote(IPlayerCharacter playerCharacter)
        {
            Plugin.Framework.RunOnTick(() => OnPhotographFlash(playerCharacter),
                TimeSpan.FromMilliseconds(EmoteReaderHooks.PhotographScreenshotDelayMs));
            Plugin.Framework.RunOnTick(() => Plugin.WindowSlideManager.StartSlide(), TimeSpan.FromMilliseconds(EmoteReaderHooks.PhotographScreenshotDelayMs + EmoteReaderHooks.TimeFromFlashToPhoto));
        }

        public void OnPhotographFlash(IPlayerCharacter playerCharacter)
        {
            InputFaker.PressHideHudKey();
            Vector2 cameraOffset = CameraOffsets.GetCameraOffset(playerCharacter);
            Vector3 newCameraPosition = CameraOffsets.ApplyOffset(playerCharacter, cameraOffset);
            //float angle = (float)(plugin.Configuration.Angle * Math.PI / 180);
            CammyCameraAimService.MoveCamera(newCameraPosition, (float)(playerCharacter.Rotation + Math.PI), 0);
            Plugin.Framework.RunOnTick(() => OnHiddenHUD(playerCharacter), TimeSpan.FromSeconds(0.5));
        }

        public void OnHiddenHUD(IPlayerCharacter playerCharacter)
        {
            Plugin.Framework.RunOnTick(() => ScreenshotService.TakeScreenshot(() => OnScreenshotTaken(playerCharacter)), delayTicks: 1);
        }

        public void OnScreenshotTaken(IPlayerCharacter playerCharacter)
        {
            InputFaker.PressHideHudKey();
            CammyCameraAimService.DisableCodeMovable();
            ScreenshotService.GeneratePhotoTexture();
        }
    }
}
