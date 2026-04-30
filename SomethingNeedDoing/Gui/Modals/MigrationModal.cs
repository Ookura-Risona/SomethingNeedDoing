using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.Configuration;
using ECommons.ImGuiMethods;
using Newtonsoft.Json;
using System.IO;

namespace SomethingNeedDoing.Gui.Modals;

public static class MigrationModal
{
    private static Vector2 Size = new(800, 0);
    private static bool IsOpen = false;
    private static string? _oldConfigJson = string.Empty;
    private static readonly Dictionary<string, (ConfigMacro Macro, bool Selected)> newMacros = [];
    private static bool migrationValid = true;
    private static string errorMessage = string.Empty;
    private static bool selectAllNewMacros = true;
    private static readonly HashSet<string> expandedMacros = [];
    private static float _listHeight = 0f;

    public static void Open(string? oldConfigJson = null)
    {
        IsOpen = true;
        _oldConfigJson = oldConfigJson;
        expandedMacros.Clear();
        PreviewMigration();
        Size.Y = CalculateRequiredHeight();
    }

    public static void Close()
    {
        IsOpen = false;
        ImGui.CloseCurrentPopup();
    }

    public static void DrawModal()
    {
        if (!IsOpen) return;

        ImGui.OpenPopup($"迁移##{nameof(MigrationModal)}");

        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Size);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        using var popup = ImRaii.PopupModal($"迁移##{nameof(MigrationModal)}", ref IsOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar);
        if (!popup) return;

        if (!migrationValid)
        {
            ImGuiEx.Text(ImGuiColors.DalamudRed, "迁移预览失败");

            using (var errorBox = ImRaii.Child("ErrorBox", new Vector2(400, 100), false))
                ImGui.TextWrapped(errorMessage);

            ImGuiUtils.CenteredButtons(("关闭", Close));

            return;
        }

        ImGui.TextColored(ImGuiColors.DalamudViolet, "导入宏");
        ImGui.TextUnformatted("请确认将要从旧配置中导入的宏");
        ImGui.Separator();
        ImGui.Spacing();

        // TODO: left align the text or something
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudViolet))
        using (ImRaii.PushColor(ImGuiCol.Button, Vector4.Zero).Push(ImGuiCol.ButtonHovered, Vector4.Zero).Push(ImGuiCol.ButtonActive, Vector4.Zero))
        //using (ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)))
        {
            var buttonHeight = ImGui.GetFrameHeight() * 1.5f;
            if (ImGui.Button("全选", new Vector2(-1, buttonHeight)))
            {
                selectAllNewMacros = !selectAllNewMacros;
                var keys = newMacros.Keys.ToList();
                foreach (var key in keys)
                {
                    var (macro, _) = newMacros[key];
                    newMacros[key] = (macro, selectAllNewMacros);
                }
            }
        }
        ImGui.Separator();

        using var child = ImRaii.Child("宏列表", new Vector2(-1, _listHeight), true);
        if (child)
        {
            foreach (var (name, (macro, selected)) in newMacros)
            {
                var isExpanded = expandedMacros.Contains(name);
                var newSelected = selected;

                if (ImGui.Checkbox($"##{name}", ref newSelected))
                    newMacros[name] = (macro, newSelected);

                ImGui.SameLine();

                using (var macroChild = ImRaii.Child($"宏##{name}", new Vector2(-1, ImGui.GetTextLineHeight() + ImGui.GetStyle().FramePadding.Y * 2), false))
                {
                    if (macroChild)
                    {
                        ImGuiEx.TextV($"{name} ({macro.Type})");
                        ImGui.SameLine();
                        ImGuiEx.TextV(ImGuiColors.DalamudGrey, $"位于 {macro.FolderPath}");
                    }
                }
                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    ExpandMacro(name);

                if (isExpanded)
                {
                    ImGui.Indent(20);

                    ImGui.TextUnformatted("内容:");
                    using (ImRaii.Child($"内容##{name}", new Vector2(-1, 100), false))
                        ImGui.TextWrapped(macro.Content);

                    ImGui.Spacing();
                    ImGui.TextUnformatted("设置:");
                    ImGui.BulletText($"制作循环: {macro.Metadata.CraftingLoop}");
                    if (macro.Metadata.CraftingLoop)
                        ImGui.BulletText($"循环次数: {macro.Metadata.CraftLoopCount}");
                    if (macro.Metadata.TriggerEvents.Count > 0)
                        ImGui.BulletText($"触发事件: {string.Join(", ", macro.Metadata.TriggerEvents)}");

                    ImGui.Unindent(20);
                }

                ImGui.Separator();
            }
        }

        child.Dispose();
        ImGui.Spacing();

        ImGuiUtils.CenteredButtons(("导入选中的宏", () => { ApplySelectedChanges(); Close(); }), ("取消", Close));
    }

    private static float CalculateRequiredHeight()
    {
        var style = ImGui.GetStyle();
        var height = 0f;

        // Header
        height += ImGui.GetTextLineHeight() * 2; // Title and description
        height += style.ItemSpacing.Y * 2;
        height += style.WindowPadding.Y * 2;

        if (!migrationValid)
        {
            // Error state
            height += ImGui.GetTextLineHeight(); // Error title
            height += 100; // Error box height
            height += style.ItemSpacing.Y * 2;
            height += ImGui.GetFrameHeight(); // Close button
            return height;
        }

        // Select all button
        height += ImGui.GetFrameHeight() * 1.5f;
        height += style.ItemSpacing.Y;

        _listHeight = UpdateListHeight();
        height += _listHeight;

        // Bottom buttons
        height += ImGui.GetFrameHeight();
        height += style.ItemSpacing.Y;

        return height;
    }

    private static float UpdateListHeight()
    {
        var _listHeight = 0f;
        foreach (var (name, (_, _)) in newMacros)
        {
            _listHeight += ImGui.GetFrameHeight(); // Checkbox and name
            if (expandedMacros.Contains(name))
            {
                _listHeight += ImGui.GetTextLineHeight() * 4; // Content preview
                _listHeight += ImGui.GetTextLineHeight() * 3; // Settings
                _listHeight += ImGui.GetStyle().ItemSpacing.Y * 2;
            }
            _listHeight += ImGui.GetStyle().ItemSpacing.Y;
        }
        return Math.Min(_listHeight, 400); // Cap at 400 pixels
    }

    private static void PreviewMigration()
    {
        try
        {
            dynamic? oldConfig = null;

            // Try to get config from clipboard first if provided
            if (!string.IsNullOrWhiteSpace(_oldConfigJson))
            {
                try
                {
                    oldConfig = JsonConvert.DeserializeObject<dynamic>(_oldConfigJson);
                }
                catch (JsonReaderException)
                {
                    FrameworkLogger.Warning("无法解析剪贴板内容为JSON");
                }
            }

            // If clipboard import failed or wasn't provided, try to load from file
            if (oldConfig == null)
            {
                var configPath = Path.Combine(EzConfig.GetPluginConfigDirectory(), "SomethingNeedDoing.json");
                if (File.Exists(configPath))
                {
                    try
                    {
                        FrameworkLogger.Info($"从 {configPath} 读取配置");
                        oldConfig = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(configPath));
                    }
                    catch (JsonReaderException)
                    {
                        FrameworkLogger.Warning("无法解析配置文件为JSON");
                    }
                }
            }

            if (oldConfig == null)
            {
                migrationValid = false;
                errorMessage = "在剪贴板或配置文件中未找到有效配置";
                return;
            }

            FrameworkLogger.Info($"旧配置类型: {oldConfig.GetType().Name}");

            if (oldConfig.RootFolder != null)
                PreviewMacrosFromOldStructure(oldConfig.RootFolder);
            else
                FrameworkLogger.Warning("旧配置中未找到宏");

            FrameworkLogger.Info($"迁移预览摘要:");
            FrameworkLogger.Info($"- 新宏数量: {newMacros.Count}");

            migrationValid = true;
        }
        catch (Exception ex)
        {
            migrationValid = false;
            errorMessage = $"预览迁移时出错: {ex.Message}";
            FrameworkLogger.Error(ex, "预览迁移失败");
        }
    }

    private static void PreviewMacrosFromOldStructure(dynamic rootFolder)
    {
        string? rootFolderName;
        try
        {
            if (rootFolder.Name != null)
            {
                rootFolderName = rootFolder.Name.ToString();
                FrameworkLogger.Info($"根文件夹名称: {rootFolderName}");
            }
            else
            {
                FrameworkLogger.Warning("根文件夹无名称，使用默认值");
                rootFolderName = ConfigMacro.Root;
            }
        }
        catch (Exception ex)
        {
            FrameworkLogger.Error(ex, "确定根文件夹名称时出错");
            rootFolderName = ConfigMacro.Root;
        }

        static void TraverseFolderStructure(dynamic folder, string currentPath, bool isRoot = false)
        {
            if (folder == null) return;

            FrameworkLogger.Info($"遍历文件夹: {currentPath}");

            try
            {
                var children = folder.Children;
                if (children == null)
                {
                    FrameworkLogger.Warning($"文件夹中未找到子项属性: {currentPath}");
                    return;
                }

                foreach (var node in children)
                {
                    try
                    {
                        // Check if this is a macro node by looking for Contents property
                        if (node.Contents != null)
                        {
                            var macro = new ConfigMacro
                            {
                                Name = node.Name ?? "未知",
                                Type = node.Language?.ToString() == "1" ? MacroType.Lua : MacroType.Native,
                                Content = node.Contents.ToString(),
                                FolderPath = isRoot ? "/" : currentPath,
                                Metadata = new MacroMetadata
                                {
                                    LastModified = DateTime.Now,
                                    CraftingLoop = node.CraftingLoop?.Value ?? false,
                                    CraftLoopCount = node.CraftLoopCount != null ? (int)(long)node.CraftLoopCount.Value : 0,
                                    TriggerEvents = (node.isPostProcess?.Value ?? false) ? [TriggerEvent.OnAutoRetainerCharacterPostProcess] : [],
                                }
                            };

                            FrameworkLogger.Info($"添加宏: {macro.Name} 位于 {macro.FolderPath}");
                            newMacros[macro.Name] = (macro, true);
                        }
                        else if (node.Name != null)
                        {
                            var folderName = node.Name.ToString();

                            // If this is the root folder's children, use "/" as the path
                            if (isRoot)
                                TraverseFolderStructure(node, "/");
                            else
                            {
                                // For other folders, build the path normally
                                var newPath = Path.Combine(currentPath, folderName).Replace('\\', '/');
                                TraverseFolderStructure(node, newPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FrameworkLogger.Error(ex, $"处理文件夹 {currentPath} 中的节点时出错");
                    }
                }
            }
            catch (Exception ex)
            {
                FrameworkLogger.Error(ex, $"遍历文件夹 {currentPath} 时出错");
            }
        }

        try
        {
            TraverseFolderStructure(rootFolder, rootFolderName, true);
        }
        catch (Exception ex)
        {
            FrameworkLogger.Error(ex, "遍历文件夹结构失败");
        }
    }

    private static void ApplySelectedChanges()
    {
        try
        {
            foreach (var (_, (macro, selected)) in newMacros.Where(m => m.Value.Selected))
                C.Macros.Add(macro);

            C.Save();
            Svc.Chat.Print("选中的宏已成功导入!");
        }
        catch (Exception ex)
        {
            Svc.Chat.PrintErrorMsg($"导入宏失败: {ex.Message}");
        }
    }

    private static void ExpandMacro(string name)
    {
        if (!expandedMacros.Remove(name))
            expandedMacros.Add(name);
        Size.Y = CalculateRequiredHeight();
    }
}
