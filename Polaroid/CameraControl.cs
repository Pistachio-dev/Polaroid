using Brio.Game.Actor.Extensions;
using Brio.Game.Actor.Interop;
using Brio.Game.Posing.Skeletons;
using Cammy;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.STD;
using Hypostasis.Game;
using Hypostasis.Game.Structures;
using SamplePlugin;
using Serilog;
using Swan;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using NativeBone = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Skeleton.Bone;

namespace Polaroid
{
    public class CameraControl
    {
        public static unsafe void SkeletonShenanigans()
        {
            IPlayerCharacter player = Plugin.ClientState.LocalPlayer;
            ICharacter asICharacter = (ICharacter)player;
            BrioCharacterBase* characterBasePointer = asICharacter.GetCharacterBase();
            Skeleton? skeleton = Skeleton.Create(characterBasePointer->CharacterBase.Skeleton);
            if (skeleton == null)
            {
                Plugin.Log.Error("Could not fetch current player skeleton. Calcium deficit, probably");
                return;
            }
            foreach (var bone in skeleton.Bones)
            {
                //Plugin.Log.Info($"{bone.FriendlyName}: {bone.Index} {bone.LastTransform.Humanize()}");
            }

            var leftIndexFingerTipBone = skeleton.Bones.FirstOrDefault(bone => bone.FriendlyName == "j_ko_b_l");
            if (leftIndexFingerTipBone == null)
            {
                Plugin.Log.Error("Could not get the index finger bone. Is this a yakuza movie?");
                return;
            }

            Plugin.Log.Info($"{leftIndexFingerTipBone.FriendlyName}: {leftIndexFingerTipBone.Index}");
            Plugin.Log.Info($"{leftIndexFingerTipBone.LastTransform.Humanize()}");
            Plugin.Log.Info($"{leftIndexFingerTipBone.LastRawTransform.Humanize()}");

            //var native = player.Native();

        }

        public static unsafe void EnumerateDrawEntities()
        {
            World world = *World.Instance();
            FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object o = world.Object;

            EnumerateDrawEntitiesRecursive(o, 0);
        }

        private static unsafe void EnumerateDrawEntitiesRecursive(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object o, int depth)
        {
            
            int safetyCounter = 0;
            var player = Plugin.ClientState.LocalPlayer;
            foreach (var child in o.ChildObjects)
            {
                if (safetyCounter >= 500000)
                {
                    Plugin.Log.Warning("Safety counter pulled us out!");
                    return;
                }
                safetyCounter++;
                var type = child->GetObjectType();
                if (type == ObjectType.CharacterBase)
                {
                    var chara = (CharacterBase*)child;
                    if (player != null)
                    {
                        if (AreEqual(chara->Position, player.Position))
                        {
                            Plugin.Log.Info("FOUND THE PLAYER AT " + safetyCounter);
                            Plugin.Log.Info($"Scene pos: {chara->Position}");
                            Plugin.Log.Info($"Scene rot: {chara->Rotation}");
                            Plugin.Log.Info($"Logic pos: {player.Position}");
                            Plugin.Log.Info($"Logic rot: {player.Rotation}");
                            Plugin.Log.Info($"Sklt pos: {chara->Skeleton->Transform.Position}");
                            Plugin.Log.Info($"Sklt rot: {chara->Skeleton->Transform.Rotation}");
                            SkeltonStuff(chara);
                            return;
                        }
                    }
                    
                }
                Plugin.Log.Info(type.ToString());
                //if (o.ChildObject != null)
                //{
                //    EnumerateDrawEntitiesRecursive(*o.ChildObject, depth + 1);
                //}
            }
        }

        private static unsafe void SkeltonStuff(CharacterBase* chara)
        {
            var skl = *chara->Skeleton;
            var bonePointer = skl.AttachBones;
            Plugin.Log.Info("Bone count: " + skl.AttachBoneCount);
            for (int i = 0; i < skl.AttachBoneCount; i++)
            {
                
                NativeBone b = *(bonePointer + sizeof(NativeBone) * i);
                Plugin.Log.Info($"Bone {b.BoneIndex}");//: {b.BoneName}");

                //StringBuilder s = new StringBuilder();
                //if (b.BoneName == null)
                //{
                //    continue;
                //}
                //int limit = (int)Math.Min(40, b.BoneName.Length);
                //for (int k = 0; k < limit; k++)
                //{
                //    s.Append(b.BoneName[k]);
                //}
                //Plugin.Log.Info($"Name: {s}");//: {b.BoneName}");
            }
        }

        private static bool AreEqual(Vector3 a, Vector3 b)
        {
            return AreEqual(a.X, b.X) && AreEqual(a.Y, b.Y); //&& AreEqual(a.Z, b.Z);
        }

        private static bool AreEqual(float a, float b)
        {
            return Math.Abs(a - b) <= 0.0001;
        }

        public static unsafe void EnableCodeMovable()
        {
            if (Plugin.ClientState.LocalPlayer == null)
            {
                Plugin.Log.Error("No player detected, how did we get here?");
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

        public unsafe static void SetToCharacterPos()
        {
            var character = Plugin.ClientState.LocalPlayer;
            var pos = character.Position;
            float rotation = (float)(character.Rotation  + Math.PI); // Player orientation and camera orientation seem reversed
            Plugin.Log.Info("Character pos: " + pos.ToString());
            Plugin.Log.Info("Character rotation: " + rotation); // Radians
            float charHeight = 1.58f;
            //float charHeight = 2.17f;
            // no magic formula. I'll have to adjust each race individually. Bet set a slider in the test ui to do it fast.
            // Apply camera height
            CodeMovableCamera.MovePosition(pos.X, pos.Y + (charHeight * 9 / 10), pos.Z);
            CodeMovableCamera.HRotation = rotation;
            // Apply camera distance
            CodeMovableCamera.ApplyDistance(0.5f, 0, 0);
            CodeMovableCamera.Zoom = 0;
            // Make it work well for Au Ra first, then you can go back to adjusting.

            Plugin.Log.Info($"Current target: {(character.TargetObject == null ? "Nobody"
                : character.TargetObject.GetFriendlyName())}");
            // Apply LookTo
            if (character.TargetObject != null)
            {
                IGameObject x = character.TargetObject;
                CodeMovableCamera.Target = character.TargetObject.Native();
                //var target = character.TargetObject;
                //var lookAtX = target.Position.X;
                //var lookAtY = target.Position.Y;
                //var lookAtZ = target.Position.Z;
                //var lookAtY2 = target.Position.Y;
                //CodeMovableCamera.LookAtPosition = new Vector4(lookAtX, lookAtY, lookAtZ, lookAtY2);
            }
            
        }

        public unsafe static void SetPositionToCameraLens()
        {
            if (!CodeMovableCamera.Enabled)
            {
                Plugin.Log.Warning("Not in movable camera mode");
                return;
            }

            var bone = GetReferenceBone();
            if (bone == null)
            {
                Plugin.Log.Warning("Could not fetch the player finger bone.");
                return;
            }

            var lastPos = bone.LastTransform.Position;
            var lastRawPos = bone.LastRawTransform.Position;
            Plugin.Log.Info($"Last transform: {lastPos.X}, {lastPos.Y}, {lastPos.Z}");
            Plugin.Log.Info($"Last raw transform: {lastRawPos.X}, {lastRawPos.Y}, {lastRawPos.Z}");
            //CodeMovableCamera.Position = bone.LastRawTransform.Position;
            DumpCameraOnLog();

        }
        private Vector3? OldPos;


        private static unsafe Bone? GetReferenceBone()
        {
            IPlayerCharacter player = Plugin.ClientState.LocalPlayer;
            ICharacter asICharacter = player;
            var characterBasePointer = asICharacter.GetCharacterBase();
            Skeleton? skeleton = Skeleton.Create(characterBasePointer);
            if (skeleton == null)
            {
                Plugin.Log.Error("Could not fetch current player skeleton. Calcium deficit, probably");
                return null;
            }
            foreach (var bone in skeleton.Bones)
            {
                Plugin.Log.Info($"{bone.FriendlyName}: {bone.Index}");
            }

            //12:35:39.639 | INF | [Polaroid] j_naka_b_l: 96

            string leftMiddleFinger = "j_naka_b_l";
            string face = "j_kao";
            // https://xivmodding.com/books/ff14-asset-reference-document/page/bone-list-and-bone-scaling-notes
            var leftMiddleFingerTipBone = skeleton.Bones.FirstOrDefault(bone => bone.FriendlyName == face);
            
            if (leftMiddleFingerTipBone == null)
            {
                Plugin.Log.Error("Could not get the middle finger bone. Is this a yakuza movie?");
                return null;
            }

            return leftMiddleFingerTipBone;
        }

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

        public unsafe static void LogPositionForPlayerAndTarget()
        {
            var player = Plugin.ClientState.LocalPlayer;
            if (player == null)
            {
                Plugin.Log.Info("Player: null");
                return;
            }

            Plugin.Log.Info($"Player: {player.Position}");
            if (player.TargetObject == null)
            {
                Plugin.Log.Info("Target: null");
                return;
            }
        }

        internal void Reset()
        {

        }
    }
}
