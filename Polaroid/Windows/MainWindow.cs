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

    private void UpdateLastScreenshot(string fullPath)
    {
        //Plugin.Log.Info("Updating screenshot on main window");
        //Plugin.Log.Info("FullPath: " + fullPath);
        //lastScreenshot = Plugin.TextureProvider.GetFromFileAbsolute(fullPath).GetWrapOrDefault();
        //Plugin.Log.Info($"Loaded: {lastScreenshot?.Size.X}x{lastScreenshot?.Size.Y}");
    }

    public override void Draw()
    {
        cameraControlUI.Draw();
        ImGui.Separator();

        if (ImGui.Button("Log positions"))
        {
            CameraControl.LogPositionForPlayerAndTarget();
        }
        if (ImGui.BeginTabBar("Main tooling tabs", ImGuiTabBarFlags.None)){
            if (ImGui.BeginTabItem("Image"))
            {
                //if (ImGui.Button("Convert screenshot to polaroid format"))
                //{
                //    ScreenshotService.ConvertToPolaroidLook(@"E:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XIV Online\game\screenshots", "ffxiv_dx11 2025-06-25 21-10-38.png");
                //}
                if (ImGui.Button("Hide HUD"))
                {
                    InputFaker.PressHideHudKey();
                }
                //if (ImGui.Button("Take screenshot"))
                //{
                //    ScreenshotService.TakeScreenshot(UpdateLastScreenshot);
                //}
                using (var screenshotContainer = ImRaii.Child("ScreenshotContainer", Vector2.Zero, false))
                {
                    if (screenshotContainer.Success)
                    {
                        if (ScreenshotService.LastScreenshotPath != null){

                            ImGui.TextUnformatted("Image goes here");

                            if (screenShotAvailable || !Utilities.IsFileInUse(ScreenshotService.LastScreenshotPath))
                            {
                                screenShotAvailable = true;
                                IDalamudTextureWrap? screenshot = Plugin.TextureProvider.GetFromFile(ScreenshotService.LastScreenshotPath)
                                    .GetWrapOrDefault();
                                if (screenshot != null)
                                {
                                    ImGui.Image(screenshot.Handle, new Vector2(300, 300));
                                }
                            }
                        }
                        else
                        {
                            ImGui.TextUnformatted("No screenshot detected yet");
                        }
                    }
                }
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Camera"))
            {
                if (ImGui.Button("Race check"))
                {
                    var race= Plugin.ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Race];
                    Plugin.Log.Info($"Race: {race}");
                    var tribe = Plugin.ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Tribe];
                    Plugin.Log.Info($"Tribe: {tribe}");
                    var gender = Plugin.ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Gender];
                    Plugin.Log.Info($"Gender: {gender}");
                }
                if (ImGui.Button("Enable code movable"))
                {                   
                    CameraControl.EnableCodeMovable();
                }

                if (ImGui.Button("Set to character"))
                {
                    CameraControl.SetToCharacterPos();
                }

                if (ImGui.Button("Nudge camera"))
                {
                    CameraControl.NudgeCamera();
                }

                if (ImGui.Button("Disable code movable"))
                {
                    CameraControl.DisableCodeMovableCamera();
                }

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Timing"))
            {
                if (ImGui.Button("Stop stopwatch"))
                {
                    Plugin.CountTime = false;
                }

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Scanning"))
            {
                if (ImGui.Button("Get offsets"))
                {
                    CameraOffsets.GetCameraOffset(Plugin.ClientState.LocalPlayer?.TargetObject as ICharacter);
                }
                if (ImGui.Button("Log race, tribe, gender"))
                {
                    CameraOffsets.LogTargetTribeAndGender();
                }
                if (ImGui.Button("Scan target"))
                {
                    Utilities.DumpGameObject(Plugin.ClientState.LocalPlayer?.TargetObject);
                }
                if (ImGui.Button("Enumerate draw entities"))
                {
                    CameraControl.EnumerateDrawEntities();
                }

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("HUD"))
            {
                if (ImGui.Button("Show HUD"))
                {
                    ShowHud();
                }

                if (ImGui.Button("Hide HUD"))
                {
                    HideHud();
                }
            }
            if (ImGui.BeginTabItem("Calibration"))
            {
                if (ImGui.Button("Move to player pos") && Plugin.ClientState.LocalPlayer != null) {
                    CalibrationService.MoveDot(Plugin.ClientState.LocalPlayer.Position);
                }
                if (ImGui.Button("Move to target pos") && Plugin.ClientState.LocalPlayer?.TargetObject != null)
                {
                    CalibrationService.MoveDot(Plugin.ClientState.LocalPlayer.TargetObject.Position);
                }
                CalibrationService.Draw(Plugin.ClientState.LocalPlayer!);

            }

            ImGui.EndTabBar();
        }
        ImGui.Spacing();
    }

    private bool uiHidden = false;
    private AgentHUD* savedAgentHud = null;

    private unsafe void ToggleHud()
    {
        if (uiHidden)
        {
            AgentHUD.Instance()->Show();
        }
        else
        {
            AgentHUD.Instance()->Hide();
        }
        uiHidden = !uiHidden;
    }

    private unsafe void ShowHud()
    {

        Plugin.UIVisControl.UpdateAddonVisibility(true);
    }

    private unsafe void HideHud()
    {
        Plugin.UIVisControl.UpdateAddonVisibility(false);
    }
}
