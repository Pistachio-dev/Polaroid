using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Polaroid.Windows.Widgets;
using System;

namespace Polaroid.Windows;

public unsafe class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    public ImageGallery imageGallery = new ImageGallery(600);

    public MainWindow(Plugin plugin, string goatImagePath)
        : base("Instapix Photo Gallery", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        //var angle = plugin.Configuration.Angle;
        //if (ImGui.DragFloat("Camera angle (degrees)", ref angle, 1, -90, 90))
        //{
        //    plugin.Configuration.Angle = angle;
        //    plugin.Configuration.Save();
        //}

        imageGallery.Draw();
    }
}
