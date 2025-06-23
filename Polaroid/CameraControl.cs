using Brio.Game.Actor.Extensions;
using Brio.Game.Posing.Skeletons;
using Cammy;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Hypostasis.Game;
using Hypostasis.Game.Structures;
using SamplePlugin;
using Serilog;
using System.Linq;
using System.Numerics;

namespace Polaroid
{
    public class CameraControl
    {
        public static unsafe void EnableCodeMovable()
        {
            if (Plugin.ClientState.LocalPlayer == null)
            {
                Plugin.Log.Error("No player detected, how did we get here?");
                return;
            }

            IPlayerCharacter player = Plugin.ClientState.LocalPlayer;
            ICharacter asICharacter = (ICharacter)player;
            var characterBasePointer = asICharacter.GetCharacterBase();
            Skeleton? skeleton = Skeleton.Create(characterBasePointer);
            if (skeleton == null)
            {
                Plugin.Log.Error("Could not fetch current player skeleton. Calcium deficit, probably");
                return;
            }
            foreach (var bone in skeleton.Bones)
            {
                Plugin.Log.Info($"{bone.FriendlyName}: {bone.Index}");
            }

            var leftIndexFingerTipBone = skeleton.Bones.FirstOrDefault(bone => bone.FriendlyName == "j_ko_b_l");
            if (leftIndexFingerTipBone == null)
            {
                Plugin.Log.Error("Could not get the index finger bone. Is this a yakuza movie?");
                return;
            }

            //var transform = leftIndexFingerTipBone.LastTransform;
            ////if (!Cammy.FreeCam.Enabled)
            ////{
            ////    Cammy.FreeCam.Toggle();
            ////}

            GameCamera* camera = Common.CameraManager->worldCamera;
            //camera->lockPosition = 0;
            //camera->mode = 1;
            //camera->viewY = 0;
            //camera->viewZ = 0;
            //camera->viewX = 0;
            //if (camera->mode == 1)
            //{
            //    camera->mode = 0;
            //}
            //else
            //{
            //    camera->mode = 1;
            //}
            
            //if (!Cammy.FreeCam.Enabled)
            //{
            //    Cammy.FreeCam.Toggle();
            //}
            //FreeCam.gameCamera->x += 199;
            CodeMovableCamera.Enable();
            Plugin.Log.Warning("Code movable camera ENABLED");
            DumpCameraOnLog();
            //FreeCam.position = new Vector3(0, 0, 0);
            //camera->x = camera->x + 100;
            //camera->viewY = camera->viewY + 100;
        }

        public unsafe static void NudgeCamera()
        {
            if (!CodeMovableCamera.Enabled)
            {
                Plugin.Log.Warning("Not in movable camera mode");
                return;
            }

            CodeMovableCamera.Position.Y += 50;
            DumpCameraOnLog();

        }
        private Vector3? OldPos;

        private unsafe static void DumpCameraOnLog()
        {
            var camera = CodeMovableCamera.gameCamera;
            Plugin.Log.Info($"Position: {camera->x} {camera->y} {camera->z}");
            Plugin.Log.Info($"View: {camera->viewX} {camera->viewY} {camera->viewZ}");
            Plugin.Log.Info($"Lookat: {camera->lookAtX} {camera->lookAtY} {camera->lookAtZ}");
        }

        public unsafe static void DisableCodeMovableCamera()
        {
            CodeMovableCamera.Disable();
            Plugin.Log.Warning("Code movable camera DISABLED and positions reset");
            //if (Cammy.FreeCam.Enabled)
            //{
            //    Cammy.FreeCam.Toggle();
            //}

            //var camera = CameraManager.Instance()->Camera;
            //var sceneCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance()->CurrentCamera;
            //sceneCamera->Position = new FFXIVClientStructs.FFXIV.Common.Math.Vector3(0, 0, 2);
            //sceneCamera->LookAtVector = new FFXIVClientStructs.FFXIV.Common.Math.Vector3(0, 0, 2);
            //CameraManager* cameraManager = Common.CameraManager;
            //GameCamera worldCamera = *(cameraManager->worldCamera);


            //Cammy.Cammy cammy = Plugin.CammyPlugin;
            //Cammy.FreeCam.Toggle();
        }

        internal void Reset()
        {

        }
    }
}
