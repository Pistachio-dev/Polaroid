using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using InputInjection;
using Polaroid.Services.Camera;
using Polaroid.Services.EmoteDetection;
using Polaroid.Services.Image;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Services
{
    public static class Orchestrator
    {
        public static void OnPhotographEmote(IPlayerCharacter playerCharacter)
        {
            Plugin.Framework.RunOnTick(() => OnPhotographFlash(playerCharacter),
                TimeSpan.FromMilliseconds(EmoteReaderHooks.PhotographScreenshotDealyMs));
        }

        public static void OnPhotographFlash(IPlayerCharacter playerCharacter)
        {
            InputFaker.PressHideHudKey();
            Vector2 cameraOffset = CameraOffsets.GetCameraOffset(playerCharacter);
            Vector3 newCameraPosition = CameraOffsets.ApplyOffset(playerCharacter, cameraOffset);
            CammyCameraAimService.MoveCamera(newCameraPosition, (float)(playerCharacter.Rotation + Math.PI), 0);
            Plugin.Framework.RunOnTick(() => OnHiddenHUD(playerCharacter), TimeSpan.FromSeconds(1));
        }

        public static void OnHiddenHUD(IPlayerCharacter playerCharacter)
        {
            Plugin.Framework.RunOnTick(() => ScreenshotService.TakeScreenshot(() => OnScreenshotTaken(playerCharacter)), delayTicks: 1);
        }

        public static void OnScreenshotTaken(IPlayerCharacter playerCharacter)
        {
            InputFaker.PressHideHudKey();
            CammyCameraAimService.DisableCodeMovable();
        }
    }
}
