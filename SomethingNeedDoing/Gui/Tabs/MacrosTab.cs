using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Gui.Modals;
using SomethingNeedDoing.Managers;

namespace SomethingNeedDoing.Gui.Tabs;

public class MacrosTab(IMacroScheduler scheduler, MacroEditor macroEditor, GitMacroManager gitManager)
{
    private static class UiConstants
    {
        public const float DefaultLeftPanelWidth = 250f;
        public const float MinLeftPanelWidth = 180f;
        public const float MaxLeftPanelWidth = 400f;
        public const string DefaultFolder = "General";
    }

    private class State
    {
        public string SelectedFolderId { get; set; } = string.Empty;
        public string SelectedMacroId { get; set; } = string.Empty;
        public string SearchText { get; set; } = string.Empty;
        public float LeftPanelWidth { get; set; } = UiConstants.DefaultLeftPanelWidth;
        public bool IsFolderSectionCollapsed { get; set; }
        public HashSet<string> ExpandedFolders { get; } = [];
    }

    private readonly State _state = new();
    private readonly CreateMacroModal _createMacroModal = new(gitManager);

    public void Draw()
    {
        using var table = ImRaii.Table("Main", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable);
        if (!table) return;

        ImGui.TableSetupColumn("Tree", ImGuiTableColumnFlags.WidthFixed, _state.LeftPanelWidth * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Editor", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        DrawLeftPanel();

        if (ImGui.TableGetColumnFlags(0).HasFlag(ImGuiTableColumnFlags.WidthFixed))
        {
            var currentWidth = ImGui.GetColumnWidth(0) / ImGuiHelpers.GlobalScale;
            currentWidth = Math.Clamp(currentWidth, UiConstants.MinLeftPanelWidth, UiConstants.MaxLeftPanelWidth);

            if (Math.Abs(_state.LeftPanelWidth - currentWidth) > 1f)
                _state.LeftPanelWidth = currentWidth;
        }

        ImGui.TableNextColumn();
        macroEditor.Draw(C.GetMacro(_state.SelectedMacroId));
    }

    private void DrawLeftPanel()
    {
        ImGui.SetNextItemWidth(-1);
        var searchText = _state.SearchText;
        if (ImGui.InputTextWithHint("##Search", "搜索文件夹和宏...", ref searchText, 100))
            _state.SearchText = searchText;
        ImGui.Separator();

        using var child = ImRaii.Child("MacroTreePanel", new(0, -1), true);
        if (!child) return;

        using var _ = ImRaii.PushColor(ImGuiCol.Header, new Vector4(0.3f, 0.3f, 0.4f, 0.7f))
            .Push(ImGuiCol.HeaderHovered, new Vector4(0.4f, 0.4f, 0.5f, 0.8f));

        if (string.IsNullOrEmpty(_state.SearchText))
            DrawMacroTree();
        else
            DrawSearchResults();
    }

    private void DrawMacroTree()
    {
        DrawMacroTreeHeader();
        if (!_state.IsFolderSectionCollapsed)
            DrawFolderTree();
    }

    private void DrawMacroTreeHeader()
    {
        using var group = ImRaii.Group();
        ImGui.TextColored(ImGuiColors.DalamudViolet, "文件夹");

        var textWidth = ImGui.CalcTextSize("文件夹").X;
        ImGui.SameLine(textWidth + 15);

        if (ImGuiUtils.IconButton(_state.IsFolderSectionCollapsed ? FontAwesomeIcon.AngleDown : FontAwesomeIcon.AngleUp,
            _state.IsFolderSectionCollapsed ? "展开" : "折叠"))
            _state.IsFolderSectionCollapsed ^= true;

        ImGui.SameLine(0, 5);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(4, 4));

        if (ImGuiUtils.IconButton(FontAwesomeIcon.FileAlt, "创建新宏"))
            _createMacroModal.Open();

        ImGui.SameLine(0, 5);

        if (ImGuiUtils.IconButton(FontAwesomeIcon.FolderPlus, "创建新文件夹"))
            CreateFolderModal.Open();
    }

    private void DrawFolderTree()
    {
        using var child = ImRaii.Child("文件夹树", new(-1, ImGui.GetContentRegionAvail().Y), false);
        if (!child) return;

        DrawCustomFolders();
    }

    private void DrawCustomFolders()
    {
        try
        {
            var folders = C.GetFolderPaths()
                .Where(f => !string.IsNullOrEmpty(f))
                .OrderBy(f => f)
                .ToList();

            var rootMacros = C.GetMacrosInFolder(string.Empty).ToList();
            if (rootMacros.Count != 0)
            {
                foreach (var macro in rootMacros)
                    DrawMacroTreeNode(macro, false);

                if (folders.Count != 0)
                    ImGui.Separator();
            }

            foreach (var folderPath in folders)
                DrawFolderNode(folderPath);
        }
        catch (Exception ex)
        {
            FrameworkLogger.Error(ex, "绘制文件夹时出错");
            ImGui.TextColored(ImGuiColors.DalamudRed, "加载文件夹时出错");
        }
    }

    private void DrawFolderNode(string folderPath)
    {
        var isSelected = _state.SelectedFolderId == folderPath;
        var folderCount = C.GetMacroCount(folderPath);

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (isSelected) flags |= ImGuiTreeNodeFlags.Selected;
        if (_state.ExpandedFolders.Contains(folderPath))
            flags |= ImGuiTreeNodeFlags.DefaultOpen;

        ImGuiEx.Icon(FontAwesomeHelper.IconFolder);
        ImGui.SameLine();

        using var tree = ImRaii.TreeNode($"{folderPath} ({folderCount})##folder_{folderPath}", flags);
        if (tree)
        {
            if (ImGui.IsItemClicked())
            {
                _state.SelectedFolderId = folderPath;
                _state.SelectedMacroId = string.Empty;
            }

            DrawFolderContextMenu(folderPath);
            ImGui.Indent(10);
            DrawFolderContents(folderPath);
            ImGui.Unindent(10);
        }
        else if (ImGui.IsItemClicked())
        {
            _state.SelectedFolderId = folderPath;
            _state.SelectedMacroId = string.Empty;
        }

        if (tree.Success && !_state.ExpandedFolders.Contains(folderPath))
            _state.ExpandedFolders.Add(folderPath);
        else if (!tree.Success && _state.ExpandedFolders.Contains(folderPath))
            _state.ExpandedFolders.Remove(folderPath);
    }

    private void DrawFolderContents(string folderPath)
    {
        try
        {
            var macros = C.GetMacrosInFolder(folderPath).ToList();
            if (macros.Count == 0)
            {
                ImGui.TextColored(ImGuiColors.DalamudGrey, "此文件夹中无宏");
                return;
            }

            foreach (var macro in macros)
                DrawMacroTreeNode(macro, false);
        }
        catch (Exception ex)
        {
            FrameworkLogger.Error(ex, $"加载文件夹 {folderPath} 中的宏时出错");
            ImGui.TextColored(ImGuiColors.DalamudRed, "加载宏时出错");
        }
    }

    private void DrawFolderContextMenu(string folderPath)
    {
        using var popup = ImRaii.ContextPopupItem($"##FolderContextMenu_{folderPath}");
        if (!popup) return;

        ImGui.TextColored(ImGuiColors.DalamudViolet, $"文件夹: {folderPath}");
        ImGui.Separator();

        if (ImGui.MenuItem("重命名文件夹"))
            RenameFolderModal.Open(folderPath);

        if (ImGui.MenuItem("删除文件夹"))
        {
            try
            {
                DeleteFolder(folderPath);
            }
            catch (Exception ex)
            {
                FrameworkLogger.Error(ex, "删除文件夹时出错");
            }
        }
        ImGuiEx.Tooltip("删除此文件夹并将所有宏移至根文件夹");
    }

    private void DrawSearchResults()
    {
        ImGui.TextColored(ImGuiColors.DalamudViolet, "搜索结果");

        var allFolders = C.GetFolderPaths().Where(f => !string.IsNullOrEmpty(f));
        var foundAnyFolders = false;

        foreach (var folderPath in allFolders)
        {
            if (folderPath.Contains(_state.SearchText, StringComparison.OrdinalIgnoreCase))
            {
                foundAnyFolders = true;
                var folderCount = C.GetMacroCount(folderPath);
                var isSelected = _state.SelectedFolderId == folderPath;

                if (ImGui.Selectable($"📁 {folderPath} ({folderCount})", isSelected))
                {
                    _state.SelectedFolderId = folderPath;
                    _state.SelectedMacroId = string.Empty;
                }
            }
        }

        if (foundAnyFolders)
        {
            ImGui.Separator();
            ImGui.TextColored(ImGuiColors.DalamudViolet, "匹配的宏");
        }

        var foundAnyMacros = false;
        foreach (var macro in C.SearchMacros(_state.SearchText).ToList())
        {
            foundAnyMacros = true;
            DrawMacroTreeNode(macro, true);
        }

        if (!foundAnyFolders && !foundAnyMacros)
            ImGui.TextColored(ImGuiColors.DalamudGrey, "无匹配的文件夹或宏");
    }

    private void DrawMacroTreeNode(ConfigMacro macro, bool showFolder)
    {
        var icon = macro.IsGitMacro ? FontAwesomeHelper.IconGitMacro :
            macro.Type == MacroType.Lua ? FontAwesomeHelper.IconLuaMacro : FontAwesomeHelper.IconNativeMacro;

        ImGuiEx.Icon(icon);
        ImGui.SameLine();

        var displayName = showFolder ? $"{macro.Name} [{macro.FolderPath}]" : macro.Name;
        displayName += macro.Type == MacroType.Lua ? " (Lua)" : "";

        var isSelected = macro.Id == _state.SelectedMacroId;
        using var color = ImRaii.PushColor(ImGuiCol.Header, ImGuiColors.ParsedPurple, isSelected);
        if (ImGui.Selectable(displayName, isSelected))
        {
            _state.SelectedMacroId = macro.Id;
            if (showFolder)
                _state.SelectedFolderId = macro.FolderPath;
        }

        HandleMacroContextMenu(macro);
    }

    private void HandleMacroContextMenu(ConfigMacro macro)
    {
        using var popup = ImRaii.ContextPopupItem($"##ContextMenu_{macro.Id}");
        if (!popup) return;

        ImGui.TextColored(ImGuiColors.DalamudViolet, macro.Name);
        ImGui.Separator();

        if (ImGuiUtils.IconMenuItem(FontAwesomeHelper.IconPlay, "运行"))
        {
            scheduler.StartMacro(macro);
            ImGui.CloseCurrentPopup();
        }

        if (ImGuiUtils.IconMenuItem(FontAwesomeHelper.IconCopy, "复制内容"))
        {
            ImGui.SetClipboardText(macro.Content);
            ImGui.CloseCurrentPopup();
        }

        if (ImGuiUtils.IconMenuItem(FontAwesomeHelper.IconRename, "重命名"))
            RenameModal.Open(macro);

        if (ImGuiUtils.IconMenuItem(FontAwesomeHelper.IconDelete, "删除"))
        {
            var currentFolderId = _state.SelectedFolderId;
            var expandedFoldersCopy = new HashSet<string>(_state.ExpandedFolders);

            macro.Delete();

            if (_state.SelectedMacroId == macro.Id)
                _state.SelectedMacroId = string.Empty;

            if (!string.IsNullOrEmpty(currentFolderId))
                _state.SelectedFolderId = currentFolderId;

            _state.ExpandedFolders.Clear();
            foreach (var folder in expandedFoldersCopy)
                _state.ExpandedFolders.Add(folder);

            _state.ExpandedFolders.Add(currentFolderId);
        }

        if (macro is ConfigMacro configMacro && !macro.IsGitMacro)
        {
            ImGui.Separator();
            using var typeMenu = ImRaii.Menu("类型");
            if (typeMenu)
            {
                var isNative = macro.Type == MacroType.Native;
                var isLua = macro.Type == MacroType.Lua;

                if (ImGui.MenuItem("Native", null, isNative))
                {
                    configMacro.Type = MacroType.Native;
                    C.Save();
                }

                if (ImGui.MenuItem("Lua", null, isLua))
                {
                    configMacro.Type = MacroType.Lua;
                    C.Save();
                }
            }
        }

        ImGui.Separator();
        using var folderMenu = ImRaii.Menu("移动到文件夹");
        if (folderMenu)
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, "选择目标文件夹:");
            ImGui.Separator();

            var isInRoot = string.IsNullOrEmpty(macro.FolderPath);
            if (ImGui.MenuItem("根目录", null, isInRoot))
            {
                if (!isInRoot)
                {
                    var expandedFoldersCopy = new HashSet<string>(_state.ExpandedFolders);
                    MoveMacroToFolder(macro.Id, string.Empty);
                    _state.ExpandedFolders.Clear();
                    foreach (var folder in expandedFoldersCopy)
                        _state.ExpandedFolders.Add(folder);
                }
                ImGui.CloseCurrentPopup();
            }

            var folders = C.GetFolderPaths()
                .Where(f => !string.IsNullOrEmpty(f))
                .OrderBy(f => f)
                .ToList();

            if (folders.Any())
            {
                ImGui.Separator();
                foreach (var folder in folders)
                {
                    var isCurrentFolder = macro.FolderPath == folder;
                    if (ImGui.MenuItem($"{folder}{(isCurrentFolder ? " (当前)" : "")}", null, isCurrentFolder))
                    {
                        if (!isCurrentFolder)
                        {
                            var expandedFoldersCopy = new HashSet<string>(_state.ExpandedFolders);
                            MoveMacroToFolder(macro.Id, folder);
                            _state.ExpandedFolders.Clear();
                            foreach (var f in expandedFoldersCopy)
                                _state.ExpandedFolders.Add(f);
                            _state.ExpandedFolders.Add(folder);
                        }
                        ImGui.CloseCurrentPopup();
                    }
                }
            }

            ImGui.Separator();
            if (ImGui.MenuItem("创建新文件夹..."))
                CreateFolderModal.Open();
        }
    }

    private void MoveMacroToFolder(string macroId, string folderPath)
    {
        if (C.GetMacro(macroId) is ConfigMacro configMacro)
        {
            var oldFolder = configMacro.FolderPath;
            if (oldFolder == folderPath) return;

            configMacro.FolderPath = folderPath;
            C.Save();

            if (_state.SelectedFolderId == oldFolder)
                _state.SelectedFolderId = folderPath;

            _state.ExpandedFolders.Add(folderPath);
        }
    }

    private void DeleteFolder(string folderPath)
    {
        foreach (var macro in C.GetMacrosInFolder(folderPath).ToList())
        {
            if (macro is ConfigMacro configMacro)
                configMacro.FolderPath = string.Empty;
        }

        C.Save();

        if (_state.SelectedFolderId == folderPath)
            _state.SelectedFolderId = string.Empty;

        _state.ExpandedFolders.Remove(folderPath);
    }
}
