using Polaroid.Services.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;


namespace Polaroid.Windows.Widgets
{
    
    public class CameraControlUI
    {
        private float x, y, z, rotationH, rotationV = 0;

        public void Draw()
        {
            ImGui.BeginGroup();
            ImGui.TextUnformatted("Cam position:");
            ImGui.InputFloat("X", ref x);
            ImGui.InputFloat("Y", ref y);
            ImGui.InputFloat("Z", ref z);
            ImGui.InputFloat("Rotation H", ref rotationH);
            ImGui.InputFloat("Rotation V", ref rotationV);

            if (ImGui.Button("Move camera"))
            {
                CammyCameraAimService.MoveCamera(new System.Numerics.Vector3(x, y, z), rotationH, rotationV);
            }
            if (ImGui.Button("Reset camera"))
            {
                CammyCameraAimService.DisableCodeMovable();
            }
        }
    }
}
