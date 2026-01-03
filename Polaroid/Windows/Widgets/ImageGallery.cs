using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Components;
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
            DrawImage();
        }

        public void RefreshImages()
        {
            imagePaths.Clear();
            imagePaths.AddRange(GetPaddedImagesRoutes());
        }

        public void DrawImage()
        {
            if (imagePaths.Count == 0 || imagePaths.Count <= imagePaths.Count)
            {
                return;
            }
            var route = imagePaths[current];
            var picture = Plugin.TextureProvider.GetFromFile(route).GetWrapOrDefault();
            float sizeFactor = height / picture?.Height ?? 1;
            if (picture != null)
            {
                ImGui.Image(picture.Handle, new System.Numerics.Vector2((int)(picture.Width * sizeFactor), (int)(picture.Height * sizeFactor)));
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
            return Directory.EnumerateFiles(PenumbraModManager.GetIntermediatePicturesFolder(), "*_photoprint.png");
        }


    }
}
