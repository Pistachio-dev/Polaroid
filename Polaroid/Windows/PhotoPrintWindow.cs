using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Windows
{
    internal class PhotoPrintWindow: Window, IDisposable
    {
        public PhotoPrintWindow() : base("New photo")
        {
            Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs;
            Size = new System.Numerics.Vector2(500, 500);
        }        

        public override void Draw()
        {
        }

        public void Dispose()
        {
        }
    }
}
