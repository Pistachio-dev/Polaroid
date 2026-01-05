using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using InputInjection;
using Polaroid.Services.Camera;
using Polaroid.Services.EmoteDetection;
using Polaroid.Services.Image;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

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

        public async Task OnPhotographFlash(IPlayerCharacter playerCharacter)
        {
            HideHud();
            MoveCamera(playerCharacter);
            await System.Threading.Tasks.Task.Delay(500);
            var success = await ScreenshotService.TakeScreenshot();
            ResetCameraAndHUD(playerCharacter);
            if (success)
            {
                await ScreenshotService.GeneratePhotoTexture();
            }            
        }

        private void HideHud()
        {
            InputFaker.PressHideHudKey();
        }
        private void MoveCamera(IPlayerCharacter playerCharacter)
        {
            
            Vector2 cameraOffset = CameraOffsets.GetCameraOffset(playerCharacter);
            Vector3 newCameraPosition = CameraOffsets.ApplyOffset(playerCharacter, cameraOffset);
            //float angle = (float)(plugin.Configuration.Angle * Math.PI / 180);
            CammyCameraAimService.MoveCamera(newCameraPosition, (float)(playerCharacter.Rotation + Math.PI), 0);
        }

        public void ResetCameraAndHUD(IPlayerCharacter playerCharacter)
        {
            InputFaker.PressHideHudKey();
            CammyCameraAimService.DisableCodeMovable();
        }
    }
}
