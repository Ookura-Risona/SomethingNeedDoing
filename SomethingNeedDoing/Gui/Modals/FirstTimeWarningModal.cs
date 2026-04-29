using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace SomethingNeedDoing.Gui.Modals;

public static class FirstTimeWarningModal
{
    private static Vector2 Size = new(600, 600);

    public static void Close()
    {
        C.AcknowledgedLegacyWarning = true;
        ImGui.CloseCurrentPopup();
    }

    public static unsafe void DrawModal()
    {
        if (C.AcknowledgedLegacyWarning || !AgentLobby.Instance()->IsLoggedIntoZone) return;
        var isOpen = !C.AcknowledgedLegacyWarning;

        ImGui.OpenPopup($"FirstTimeWarningPopup##{nameof(FirstTimeWarningModal)}");

        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Size);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        using var popup = ImRaii.PopupModal($"FirstTimeWarningPopup##{nameof(FirstTimeWarningModal)}", ref isOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar);
        if (!popup) return;

        ImGui.TextWrapped($"{P.Name} has been fully rewritten to support the framework changes from API 12.");
        ImGui.BulletText("Native macros should work much the same as before.");
        ImGui.BulletText("Lua macros will not work at all. Scripts authors will need to write new scripts.");
        ImGui.BulletText("There is a legacy macro importer located in the settings menu.");

        ImGui.Spacing();
        ImGuiEx.TextCentered(ImGuiColors.DalamudGrey, $"This message will only be displayed once and will stop showing upon the release of API13.");

        var group = new ImGuiEx.EzButtonGroup() { IsCentered = true };
        group.Add("Acknowledge and Close", Close);
        group.Draw();
    }
}
