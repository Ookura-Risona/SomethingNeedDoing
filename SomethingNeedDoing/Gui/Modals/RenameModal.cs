using Dalamud.Interface.Utility.Raii;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.Gui.Modals;

public static class RenameModal
{
    private static Vector2 Size = new(350, 130);
    private static bool IsOpen = false;

    private static string _renameMacroBuffer = string.Empty;
    private static string _macroToRename = string.Empty;

    public static void Open(IMacro macro)
    {
        IsOpen = true;
        _renameMacroBuffer = macro.Name;
        _macroToRename = macro.Id;
    }

    public static void Close()
    {
        IsOpen = false;
        ImGui.CloseCurrentPopup();
    }

    public static void DrawModal()
    {
        if (!IsOpen) return;

        ImGui.OpenPopup($"重命名宏##{nameof(RenameModal)}");

        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Size);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        using var popup = ImRaii.PopupModal($"重命名宏##{nameof(RenameModal)}", ref IsOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar);
        if (!popup) return;

        ImGui.Text("输入新名称:");
        ImGui.SetNextItemWidth(-1);
        ImGuiUtils.SetFocusIfAppearing();

        var enterPressed = ImGui.IsKeyPressed(ImGuiKey.Enter) && ImGui.IsWindowFocused();

        using (ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f)))
            ImGui.InputText("##RenameMacroInput", ref _renameMacroBuffer, 100);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var confirmed = false;
        using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.3f, 0.5f, 0.3f, 1.0f)).Push(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 0.4f, 1.0f)))
            confirmed = ImGui.Button("确认", new Vector2(150, 0)) || enterPressed;

        if (confirmed && !string.IsNullOrWhiteSpace(_renameMacroBuffer))
        {
            if (C.GetMacro(_macroToRename) is ConfigMacro macro)
                macro.Rename(_renameMacroBuffer);
            Close();
        }

        ImGui.SameLine();

        using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.5f, 0.3f, 0.3f, 1.0f)).Push(ImGuiCol.ButtonHovered, new Vector4(0.6f, 0.4f, 0.4f, 1.0f)))
            if (ImGui.Button("取消", new Vector2(150, 0)) || (ImGui.IsKeyPressed(ImGuiKey.Escape) && ImGui.IsWindowFocused()))
                Close();
    }
}
