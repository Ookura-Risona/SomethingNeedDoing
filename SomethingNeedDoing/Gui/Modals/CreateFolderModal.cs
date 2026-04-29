using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui.Modals;

public static class CreateFolderModal
{
    private static Vector2 Size = new(400, 200);
    private static bool IsOpen = false;

    private static string _newFolderName = "New Folder";

    public static void Open()
    {
        IsOpen = true;
        _newFolderName = "New Folder";
    }

    public static void Close()
    {
        IsOpen = false;
        ImGui.CloseCurrentPopup();
    }

    public static void DrawModal()
    {
        if (!IsOpen) return;

        ImGui.OpenPopup($"CreateFolderPopup##{nameof(CreateFolderModal)}");

        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Size);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        using var popup = ImRaii.PopupModal($"CreateFolderPopup##{nameof(CreateFolderModal)}", ref IsOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar);
        if (!popup) return;

        ImGuiEx.Icon(FontAwesomeHelper.IconFolder);
        ImGui.SameLine();
        ImGui.Text("Create New Folder");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name:");

        ImGui.SameLine();
        ImGuiUtils.SetFocusIfAppearing();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputText("##FolderName", ref _newFolderName, 100);

        ImGui.Spacing();
        ImGui.Spacing();

        ImGuiUtils.CenteredButtons(("Create", () =>
        {
            var folderExists = false;
            foreach (var macro in C.Macros)
            {
                if (macro.FolderPath == _newFolderName)
                {
                    folderExists = true;
                    break;
                }
            }

            if (!folderExists && !string.IsNullOrWhiteSpace(_newFolderName))
            {
                // Create a dummy macro in the folder to ensure it exists (TODO: find a way around this?)
                var dummyMacro = new ConfigMacro
                {
                    Name = C.GetUniqueMacroName($"{_newFolderName} Template"),
                    Content = "// Add your macro commands here",
                    Type = MacroType.Native,
                    FolderPath = _newFolderName
                };

                C.Macros.Add(dummyMacro);
                C.Save();
                Close();
            }
        }
        ), ("Cancel", Close));
    }
}
