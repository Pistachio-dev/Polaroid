using Brio.Game.Actor.Extensions;
using Brio.Game.Posing.Skeletons;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using SamplePlugin;
using System.Linq;
using System.Numerics;

namespace Polaroid
{
    public class CameraControl
    {
        public static unsafe void PositionOnPhotographCamera()
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
            //foreach (var bone in skeleton.Bones)
            //{
            //    Plugin.Log.Info(bone.FriendlyName);
            //}
            var leftIndexFingerTipBone = skeleton.Bones.FirstOrDefault(bone => bone.FriendlyName == "j_ko_b_l");
            if (leftIndexFingerTipBone == null)
            {
                Plugin.Log.Error("Could not get the index finger bone. Is this a yakuza movie?");
                return;
            }

            var transform = leftIndexFingerTipBone.LastTransform;            
        }

        private Vector3 OldPos = new Vector3(0, 0, 0);
        public unsafe static void TestCameraSetting()
        {
            //var camera = CameraManager.Instance()->Camera;
            //var sceneCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance()->CurrentCamera;
            //sceneCamera->Position = new FFXIVClientStructs.FFXIV.Common.Math.Vector3(0, 0, 2);
            //sceneCamera->LookAtVector = new FFXIVClientStructs.FFXIV.Common.Math.Vector3(0, 0, 2);
        }

        internal void Reset()
        {

        }
    }
}
