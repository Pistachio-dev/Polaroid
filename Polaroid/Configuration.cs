using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Polaroid;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public float Angle { get; set; } // should go -90 to 90

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
