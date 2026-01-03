using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;
using Polaroid.Services.Image;
using Polaroid.Services.Penumbra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid.Windows.Widgets
{
    public class ImageGallery
    {
        List<string> imagePaths = new();
        int current = 0;
        private readonly int height;

        public ImageGallery(int height)
        {
            this.height = height;
        }
        public void Draw()
        {
            ImGui.BeginDisabled(!CanGoBackwards);
            if (ImGuiComponents.IconButton(Dalamud.Interface.FontAwesomeIcon.ArrowLeft))
            {
                GoPrev();
            }
            ImGui.SameLine();
            ImGui.EndDisabled();
            ImGui.BeginDisabled(!CanGoForward);
            if (ImGuiComponents.IconButton(Dalamud.Interface.FontAwesomeIcon.ArrowRight))
            {
                GoNext();
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Refresh")){
                RefreshImages();
            }
            ImGui.SameLine();
            ImGui.TextUnformatted("Photo gallery");
            DrawImage();
            if (imagePaths.Count > 0)
            {
                if (ImGuiComponents.IconButtonWithText(Dalamud.Interface.FontAwesomeIcon.Sign, "Set as sign texture"))
                {
                    ScreenshotService.SetModTextureFromPadded(imagePaths[current]);
                }
            }
            
        }

        public void RefreshImages()
        {
            imagePaths.Clear();
            imagePaths.AddRange(GetPaddedImagesRoutes());
        }

        public void DrawImage()
        {
            if (imagePaths.Count == 0)
            {
                return;
            }
            var route = imagePaths[current];
            var picture = Plugin.TextureProvider.GetFromFile(route).GetWrapOrDefault();
            float sizeFactor = height / (float)(picture?.Height ?? 1);
            if (picture != null)
            {
                ImGui.Image(picture.Handle, new System.Numerics.Vector2((int)(picture.Width * sizeFactor), (int)(picture.Height * sizeFactor)));
            }
            else
            {
                Plugin.Log.Warning("Can't show picture in gallery. It is supposed to be in " + route);
            }
        }

        private bool CanGoForward => current < imagePaths.Count - 1;
        private bool CanGoBackwards => current > 0;

        private void GoNext()
        {
            current++;
        }

        private void GoPrev()
        {
            current--;
        }

        private IEnumerable<string> GetPaddedImagesRoutes()
        {
            return Directory.EnumerateFiles(Path.Combine(PenumbraModManager.GetIntermediatePicturesFolder(), "photoprint"), "*.png");
        }


    }
}
