using Cammy;
using Polaroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Services.Camera
{
    public static class CammyCameraAimService
    {
        public static void MoveCamera(Vector3 position, float hRotation, float vRotation)
        {
            if (Plugin.ClientState.LocalPlayer == null)
            {
                Plugin.Log.Error("No player detected, how did we get here?");
                return;
            }

            EnableCodeMovable();
            SetCodeMovableCameraToLocation(position, hRotation, vRotation);
        }

        private static unsafe void EnableCodeMovable()
        {
            CodeMovableCamera.Enable();
            Plugin.Log.Warning("Code movable camera ENABLED");
            DumpCameraOnLog();
        }

        private static void SetCodeMovableCameraToLocation(Vector3 position, float hRotation, float vRotation)
        {
            if (!CodeMovableCamera.Enabled)
            {
                Plugin.Log.Warning("Not in movable camera mode");
                return;
            }

            CodeMovableCamera.Position = position;
            CodeMovableCamera.HRotation = hRotation;
            CodeMovableCamera.VRotation = vRotation;
        }

        public static unsafe void DisableCodeMovable()
        {
            CodeMovableCamera.Disable();
            Plugin.Log.Warning("Code movable camera DISABLED and positions reset");
        }

        private static unsafe void DumpCameraOnLog()
        {
            var camera = CodeMovableCamera.gameCamera;
            Plugin.Log.Info($"Position: {camera->x} {camera->y} {camera->z}");
            Plugin.Log.Info($"View: {camera->viewX} {camera->viewY} {camera->viewZ}");
            Plugin.Log.Info($"Lookat: {camera->lookAtX} {camera->lookAtY} {camera->lookAtZ}");
            Plugin.Log.Info($"Rotation: H: {camera->currentHRotation} V: {camera->currentVRotation}");
        }

    }
}
