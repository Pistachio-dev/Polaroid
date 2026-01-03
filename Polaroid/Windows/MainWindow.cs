using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using InputInjection;
using Polaroid.Services;
using Polaroid.Services.Camera;
using Polaroid.Services.Image;
using Polaroid.Services.Penumbra;
using Polaroid.Windows.Widgets;
using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace Polaroid.Windows;

public unsafe class MainWindow : Window, IDisposable
{
    public ImageGallery imageGallery = new ImageGallery(600);

    public MainWindow(Plugin plugin, string goatImagePath)
        : base("Instapix Photo Gallery", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
    }

    public void Dispose() { }

    public override void Draw()
    {
        imageGallery.Draw();
    }
}
