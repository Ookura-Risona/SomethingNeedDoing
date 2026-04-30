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

        ImGui.OpenPopup($"首次使用警告##{nameof(FirstTimeWarningModal)}");

        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Size);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        using var popup = ImRaii.PopupModal($"首次使用警告##{nameof(FirstTimeWarningModal)}", ref isOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar);
        if (!popup) return;

        ImGui.TextWrapped($"{P.Name} 已完全重写以支持API 12的框架变更。");
        ImGui.BulletText("原生宏(Native macros)应该和以前一样工作。");
        ImGui.BulletText("Lua宏将完全无法使用。脚本作者需要编写新的脚本。");
        ImGui.BulletText("设置菜单中提供了旧版宏导入工具。");

        ImGui.Spacing();
        ImGuiEx.TextCentered(ImGuiColors.DalamudGrey, $"此消息仅显示一次，并在API13发布后不再显示。");

        var group = new ImGuiEx.EzButtonGroup() { IsCentered = true };
        group.Add("确认并关闭", Close);
        group.Draw();
    }
}
