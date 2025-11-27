using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FaderPlugin;
using Polaroid.Services.EmoteDetection;
using Polaroid.Services.Penumbra;
using Polaroid.Windows;
using System;
using System.Diagnostics;
using System.IO;
namespace Polaroid;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider SigScanner { get; private set; } = null!;

    public UIVisibilityControl UIVisControl { get; }
    public static EmoteReaderHooks emoteReader;
    public static Cammy.Cammy CammyPlugin { get; private set; }
    private const string CommandName = "/polaroid";
    private const string TinkeringCommandName = "/polaroidtinker";

    public static PenumbraIpc PenumbraIpc { get; private set; }

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Polaroid");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
#if DEBUG
    private TinkeringWindow TinkeringWindow { get; init; }
#endif

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        CammyPlugin = new Cammy.Cammy(PluginInterface);
        UIVisControl = new UIVisibilityControl(Condition, ClientState, GameGui);
        emoteReader = new EmoteReaderHooks();
        PenumbraIpc = new PenumbraIpc(PluginInterface);

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnMainWindowCommand)
        {
            HelpMessage = "Open the Polaroid main window"
        });

#if DEBUG
        TinkeringWindow = new TinkeringWindow(this);
        WindowSystem.AddWindow(TinkeringWindow);
        CommandManager.AddHandler(TinkeringCommandName, new CommandInfo(OnTinkeringWindowCommand)
        {
            HelpMessage = "Use /polaroid to open settings"
        });
#endif

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    private static Stopwatch PhotoStopwatch = new Stopwatch();
    internal static bool CountTime = false;
    


    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnMainWindowCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();        
    }

#if DEBUG
    private void OnTinkeringWindowCommand(string command, string args)
    {
        TinkeringWindow.Toggle();
#endif
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
