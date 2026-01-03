using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace Polaroid.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base("Configuration")
    {

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
    }
}
