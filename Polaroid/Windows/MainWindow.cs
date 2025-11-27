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
    private Plugin Plugin;
    private CameraControlUI cameraControlUI;
    bool screenShotAvailable = false;
    

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        cameraControlUI= new CameraControlUI();
    }

    public void Dispose() { }

    public override void Draw()
    {
        
    }
}
