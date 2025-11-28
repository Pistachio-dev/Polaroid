using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace Polaroid.Windows
{
    public class PhotoPrintWindow: Window, IDisposable
    {
        private string? PicturePath = null;

        public int Height { get; }

        public PhotoPrintWindow(int height) : base("New photo")
        {
            Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs;
            Size = new System.Numerics.Vector2(500, 500);
            Position = new System.Numerics.Vector2(0, 0);
            Height = height;
        }        

        public void SetPicturePath(string? path)
        {
            PicturePath = path;
        }

        public override void Draw()
        {
            if ( PicturePath == null)
            {
                Plugin.Log.Info("null photo");
                return;
            }
            var picture = Plugin.TextureProvider.GetFromFile(PicturePath).GetWrapOrDefault();
            //Plugin.Log.Info("Picture path: "+ PicturePath);
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
