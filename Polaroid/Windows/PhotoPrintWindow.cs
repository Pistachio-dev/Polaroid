using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace Polaroid.Windows
{
    public class PhotoPrintWindow: Window, IDisposable
    {
        const string Url = @"E:\Penumbra\Sign Holding [Hum] [Mittens]\Sign\photograph\vfx\Photos\ffxiv_28112025_123121_137_photoprint.png";

        public int Height { get; }

        public PhotoPrintWindow(int height) : base("New photo")
        {
            Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs;
            Size = new System.Numerics.Vector2(500, 500);
            Position = new System.Numerics.Vector2(0, 0);
            Height = height;
        }        

        public override void Draw()
        {
            var picture = Plugin.TextureProvider.GetFromFile(Url).GetWrapOrDefault();
            if (picture != null)
            {
                Size = new Vector2(picture.Width, picture.Height);
                float sizeFactor = Height / (float)picture.Height;
                ImGui.Image(picture.Handle, new Vector2((int)(picture.Width * sizeFactor), (int)(picture.Width * sizeFactor)));
            }
        }

        public void Dispose()
        {
        }

        private void SlideNewPhoto()
        {

        }
    }
}
