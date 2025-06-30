using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Services.Camera
{
    public static class CalibrationService
    {
        private static readonly uint Red = ImGui.GetColorU32(new Vector4(1, 0, 0, 1));
        private static Vector3 Position = Vector3.Zero;
        private static float DotSize = 4;

        public static void Draw(IPlayerCharacter pc)
        {
            ImGui.DragFloat3("Position", ref Position, 0.01f);
            ImGui.DragFloat("Dot size", ref DotSize, 0.1f);
            ImGui.TextUnformatted($"Character rot: {pc.Rotation * 180f /Math.PI}");
            var distance = CalculateDistance(pc);
            string distanceString = $"Distance: X:{distance.X} Y:{distance.Y}";
            ImGui.TextUnformatted(distanceString);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                ImGui.SetClipboardText(distanceString);
                Plugin.Log.Info($"Distance copied to clipboard");
            }
            DrawDot();
        }

        public static void StraightenCharacter(IPlayerCharacter pc)
        {
        }
        public static void MoveDot(Vector3 newPosition)
        {
            Position = newPosition;
        }


        private static Vector2 CalculateDistance(IPlayerCharacter pc)
        {
            var playerPos = pc.Position;
            // I'm assuming we're working on the plane of Z, so it remains constant
            return new Vector2(playerPos.X - Position.X, playerPos.Y - Position.Y);
        }

        private static void DrawDot()
        {
            if (!Plugin.GameGui.WorldToScreen(Position, out Vector2 screenPos)){
                return;
            }

            ImGui.GetBackgroundDrawList().PushClipRect(ImGuiHelpers.MainViewport.Pos, ImGuiHelpers.MainViewport.Pos + ImGuiHelpers.MainViewport.Size, false);

            ImGui.GetBackgroundDrawList().AddCircleFilled(screenPos, DotSize, Red);

            ImGui.GetBackgroundDrawList().PopClipRect();
        }
    }
}
