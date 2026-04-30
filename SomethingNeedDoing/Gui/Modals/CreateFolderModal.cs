using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui.Modals;

public static class CreateFolderModal
{
    private static Vector2 Size = new(400, 200);
    private static bool IsOpen = false;

    private static string _newFolderName = "新建文件夹";

    public static void Open()
    {
        IsOpen = true;
        _newFolderName = "新建文件夹";
    }

    public static void Close()
    {
        IsOpen = false;
        ImGui.CloseCurrentPopup();
    }

    public static void DrawModal()
    {
        if (!IsOpen) return;

        ImGui.OpenPopup($"创建文件夹##{nameof(CreateFolderModal)}");

        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Size);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        using var popup = ImRaii.PopupModal($"创建文件夹##{nameof(CreateFolderModal)}", ref IsOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar);
        if (!popup) return;

        ImGuiEx.Icon(FontAwesomeHelper.IconFolder);
        ImGui.SameLine();
        ImGui.Text("创建新文件夹");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.AlignTextToFramePadding();
        ImGui.Text("名称:");

        ImGui.SameLine();
        ImGuiUtils.SetFocusIfAppearing();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputText("##FolderName", ref _newFolderName, 100);

        ImGui.Spacing();
        ImGui.Spacing();

        ImGuiUtils.CenteredButtons(("创建", () =>
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
                    Name = C.GetUniqueMacroName($"{_newFolderName} 模板"),
                    Content = "// 在此处添加宏命令",
                    Type = MacroType.Native,
                    FolderPath = _newFolderName
                };

                C.Macros.Add(dummyMacro);
                C.Save();
                Close();
            }
        }
        ), ("取消", Close));
    }
}
