using Brio.Game.Actor.Extensions;
using Brio.Game.Posing.Skeletons;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Data.Parsing;
using SamplePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        internal void Reset()
        {

        }
    }
}
