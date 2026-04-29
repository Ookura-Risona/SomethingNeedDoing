using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui.Modals;

public static class RenameFolderModal
{
    private static Vector2 Size = new(400, 150);
    private static bool IsOpen = false;

    private static string _renameFolderBuffer = string.Empty;
    private static string _folderToRename = string.Empty;

    public static void Open(string folderPath)
    {
        IsOpen = true;
        _renameFolderBuffer = folderPath;
        _folderToRename = folderPath;
    }

    public static void Close()
    {
        IsOpen = false;
        ImGui.CloseCurrentPopup();
    }

    public static void DrawModal()
    {
        if (!IsOpen) return;

        ImGui.OpenPopup($"RenameFolderPopup##{nameof(RenameFolderModal)}");

        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Size);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        using var popup = ImRaii.PopupModal($"RenameFolderPopup##{nameof(RenameFolderModal)}", ref IsOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar);
        if (!popup) return;

        ImGuiEx.Icon(FontAwesomeHelper.IconRename);
        ImGui.SameLine();
        ImGui.Text("Rename Folder");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text("Enter new folder name:");
        ImGui.SetNextItemWidth(-1);
        ImGuiUtils.SetFocusIfAppearing();

        var enterPressed = ImGui.IsKeyPressed(ImGuiKey.Enter) && ImGui.IsWindowFocused();

        using (ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f)))
            ImGui.InputText("##RenameFolderInput", ref _renameFolderBuffer, 100);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var invalid = false;
        if (!string.IsNullOrEmpty(_renameFolderBuffer) && C.GetFolderPaths().Any(f => f == _renameFolderBuffer))
        {
            invalid = true;
            ImGuiEx.Text(ImGuiColors.DalamudRed, $"Folder name '{_renameFolderBuffer}' already exists.");
        }

        var confirmed = false;
        using (ImRaii.Disabled(invalid))
        using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.3f, 0.5f, 0.3f, 1.0f)).Push(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 0.4f, 1.0f)))
            confirmed = ImGui.Button("Rename", new Vector2(150, 0)) || enterPressed;

        if (confirmed && !string.IsNullOrWhiteSpace(_renameFolderBuffer))
        {
            try
            {
                C.RenameFolder(_folderToRename, _renameFolderBuffer);
                Close();
            }
            catch (Exception ex)
            {
                FrameworkLogger.Error(ex, "Error renaming folder");
            }
        }

        ImGui.SameLine();

        using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.5f, 0.3f, 0.3f, 1.0f)).Push(ImGuiCol.ButtonHovered, new Vector4(0.6f, 0.4f, 0.4f, 1.0f)))
            if (ImGui.Button("Cancel", new Vector2(150, 0)) || (ImGui.IsKeyPressed(ImGuiKey.Escape) && ImGui.IsWindowFocused()))
                Close();
    }
}
