using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Services.Camera
{
    public static class CameraOffsets
    {
        public static void LogTargetTribeAndGender()
        {
            if (Plugin.ClientState.LocalPlayer == null
                || Plugin.ClientState.LocalPlayer.TargetObject == null
                || !(Plugin.ClientState.LocalPlayer.TargetObject is ICharacter targetChar))
            {
                Plugin.ChatGui.PrintError("Current target is not a character");
                return;
            }
            var race = targetChar.Customize[(int)CustomizeIndex.Race];
            var tribe = targetChar.Customize[(int)CustomizeIndex.Tribe];
            var gender = targetChar.Customize[(int)CustomizeIndex.Gender];
            Plugin.Log.Info($"{targetChar.Name}: Race: {race} Tribe: {tribe} Gender: {gender}");
        }

        public static Vector3 ApplyOffset(IPlayerCharacter player, Vector2 offset)
        {
            var xOffset = (float)Math.Sin(player.Rotation) * offset.X;
            var zOffset = (float)Math.Cos(player.Rotation) * offset.X;
            
            return new Vector3(player.Position.X - xOffset, player.Position.Y - offset.Y, player.Position.Z - zOffset);
        }

        public static Vector2 GetCameraOffset(ICharacter character)
        {
            var tribe = character.Customize[(int)CustomizeIndex.Tribe];
            var gender = character.Customize[(int)CustomizeIndex.Gender];
            var key = $"{GetRaceWord(tribe)}_{GetGenderWord(gender)}";
            Plugin.Log.Debug("Camera offset key: " + key);
            if (!Offsets.TryGetValue(key, out Vector2 offset))
            {
                Plugin.Log.Warning($"Failed to retrieve camera offset for tribe {tribe} and gender {gender}, key {key}");
                return Offsets["Midlander_Male"];
            }

            return offset;
        }

        private static string GetGenderWord(int customizeIndexGender)
        {
            return customizeIndexGender == 0 ? "Male" : "Female";
        }
        private static string GetRaceWord(int customizeIndexTribe)
        {
            switch (customizeIndexTribe)
            {
                case 1: return "Midlander";
                case 2: return "Highlander";
                case 3:
                case 4: return "Elezen";
                case 5:
                case 6: return "Lalafell";
                case 7:
                case 8: return "Miqote";
                case 9:
                case 10: return "Roegadyn";
                case 11:
                case 12: return "AuRa";
                case 13:
                case 14: return "Hrothgar";
                case 15:
                case 16: return "Viera";
                default: return "Midlander";
            }

        }
        public static readonly Dictionary<string, Vector2> Offsets = new Dictionary<string, Vector2>()
        {
            {"Midlander_Male", new Vector2(-0.48007202f, -1.480011f) },
            {"Midlander_Female", new Vector2(-0.34005737f, -1.4500122f) },
            {"Highlander_Male", new Vector2(-0.5600586f, -1.6300049f) },
            {"Highlander_Female", new Vector2(-0.4100647f, -1.6300049f) },
            {"Miqote_Male", new Vector2(-0.47006226f, -1.3800049f) },
            {"Miqote_Female", new Vector2(-0.35006714f, -1.4200134f) },
            {"Roegadyn_Male", new Vector2(-0.52008057f, -1.9200134f) },
            {"Roegadyn_Female", new Vector2(-0.460083f, -1.9500122f) },
            {"Lalafell_Male", new Vector2(-0.20007324f, -0.76000977f) },
            {"Lalafell_Female", new Vector2(-0.20007324f, -0.76000977f) },
            {"AuRa_Male", new Vector2(-0.6600647f, -1.730011f) },
            {"AuRa_Female", new Vector2(-0.38006592f, -1.3500061f) },
            {"Hrothgar_Male", new Vector2(-0.55007935f, -1.7900085f) },
            {"Hrothgar_Female", new Vector2(-0.45007324f, -1.730011f) },
            {"Viera_Male", new Vector2(-0.5100708f, -1.4700012f) },
            {"Viera_Female", new Vector2(-0.5100708f, -1.4700012f) },
            {"Elezen_Male", new Vector2(-0.5800781f, -1.6900024f) },
            {"Elezen_Female", new Vector2(-0.42007446f, -1.7399902f) },
        };
    }
}
