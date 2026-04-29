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

        ImGui.OpenPopup($"MigrationPopup##{nameof(MigrationModal)}");

        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Size);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        using var popup = ImRaii.PopupModal($"MigrationPopup##{nameof(MigrationModal)}", ref IsOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar);
        if (!popup) return;

        if (!migrationValid)
        {
            ImGuiEx.Text(ImGuiColors.DalamudRed, "Migration Preview Failed");

            using (var errorBox = ImRaii.Child("ErrorBox", new Vector2(400, 100), false))
                ImGui.TextWrapped(errorMessage);

            ImGuiUtils.CenteredButtons(("Close", Close));

            return;
        }

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Import Macros");
        ImGui.TextUnformatted("Review the macros that will be imported from the old configuration.");
        ImGui.Separator();
        ImGui.Spacing();

        // TODO: left align the text or something
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudViolet))
        using (ImRaii.PushColor(ImGuiCol.Button, Vector4.Zero).Push(ImGuiCol.ButtonHovered, Vector4.Zero).Push(ImGuiCol.ButtonActive, Vector4.Zero))
        //using (ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)))
        {
            var buttonHeight = ImGui.GetFrameHeight() * 1.5f;
            if (ImGui.Button("Select All New Macros", new Vector2(-1, buttonHeight)))
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

        using var child = ImRaii.Child("MacrosList", new Vector2(-1, _listHeight), true);
        if (child)
        {
            foreach (var (name, (macro, selected)) in newMacros)
            {
                var isExpanded = expandedMacros.Contains(name);
                var newSelected = selected;

                if (ImGui.Checkbox($"##{name}", ref newSelected))
                    newMacros[name] = (macro, newSelected);

                ImGui.SameLine();

                using (var macroChild = ImRaii.Child($"Macro##{name}", new Vector2(-1, ImGui.GetTextLineHeight() + ImGui.GetStyle().FramePadding.Y * 2), false))
                {
                    if (macroChild)
                    {
                        ImGuiEx.TextV($"{name} ({macro.Type})");
                        ImGui.SameLine();
                        ImGuiEx.TextV(ImGuiColors.DalamudGrey, $"in {macro.FolderPath}");
                    }
                }
                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    ExpandMacro(name);

                if (isExpanded)
                {
                    ImGui.Indent(20);

                    ImGui.TextUnformatted("Content:");
                    using (ImRaii.Child($"Content##{name}", new Vector2(-1, 100), false))
                        ImGui.TextWrapped(macro.Content);

                    ImGui.Spacing();
                    ImGui.TextUnformatted("Settings:");
                    ImGui.BulletText($"Crafting Loop: {macro.Metadata.CraftingLoop}");
                    if (macro.Metadata.CraftingLoop)
                        ImGui.BulletText($"Loop Count: {macro.Metadata.CraftLoopCount}");
                    if (macro.Metadata.TriggerEvents.Count > 0)
                        ImGui.BulletText($"Trigger Events: {string.Join(", ", macro.Metadata.TriggerEvents)}");

                    ImGui.Unindent(20);
                }

                ImGui.Separator();
            }
        }

        child.Dispose();
        ImGui.Spacing();

        ImGuiUtils.CenteredButtons(("Import Selected Macros", () => { ApplySelectedChanges(); Close(); }), ("Cancel", Close));
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
                    FrameworkLogger.Warning("Failed to parse clipboard content as JSON");
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
                        FrameworkLogger.Info($"Reading config from {configPath}");
                        oldConfig = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(configPath));
                    }
                    catch (JsonReaderException)
                    {
                        FrameworkLogger.Warning("Failed to parse config file as JSON");
                    }
                }
            }

            if (oldConfig == null)
            {
                migrationValid = false;
                errorMessage = "No valid configuration found in clipboard or config file";
                return;
            }

            FrameworkLogger.Info($"Old config type: {oldConfig.GetType().Name}");

            if (oldConfig.RootFolder != null)
                PreviewMacrosFromOldStructure(oldConfig.RootFolder);
            else
                FrameworkLogger.Warning("No macros found in old config");

            FrameworkLogger.Info($"Migration preview summary:");
            FrameworkLogger.Info($"- New macros: {newMacros.Count}");

            migrationValid = true;
        }
        catch (Exception ex)
        {
            migrationValid = false;
            errorMessage = $"Error previewing migration: {ex.Message}";
            FrameworkLogger.Error(ex, "Failed to preview migration");
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
                FrameworkLogger.Info($"Root folder name: {rootFolderName}");
            }
            else
            {
                FrameworkLogger.Warning("Root folder has no name, using default");
                rootFolderName = ConfigMacro.Root;
            }
        }
        catch (Exception ex)
        {
            FrameworkLogger.Error(ex, "Error determining root folder name");
            rootFolderName = ConfigMacro.Root;
        }

        static void TraverseFolderStructure(dynamic folder, string currentPath, bool isRoot = false)
        {
            if (folder == null) return;

            FrameworkLogger.Info($"Traversing folder: {currentPath}");

            try
            {
                var children = folder.Children;
                if (children == null)
                {
                    FrameworkLogger.Warning($"No Children property found in folder: {currentPath}");
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
                                Name = node.Name ?? "Unknown",
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

                            FrameworkLogger.Info($"Adding macro: {macro.Name} in {macro.FolderPath}");
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
                        FrameworkLogger.Error(ex, $"Error processing node in folder {currentPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                FrameworkLogger.Error(ex, $"Error traversing folder {currentPath}");
            }
        }

        try
        {
            TraverseFolderStructure(rootFolder, rootFolderName, true);
        }
        catch (Exception ex)
        {
            FrameworkLogger.Error(ex, "Failed to traverse folder structure");
        }
    }

    private static void ApplySelectedChanges()
    {
        try
        {
            foreach (var (_, (macro, selected)) in newMacros.Where(m => m.Value.Selected))
                C.Macros.Add(macro);

            C.Save();
            Svc.Chat.Print("Selected macros imported successfully!");
        }
        catch (Exception ex)
        {
            Svc.Chat.PrintErrorMsg($"Failed to import macros: {ex.Message}");
        }
    }

    private static void ExpandMacro(string name)
    {
        if (!expandedMacros.Remove(name))
            expandedMacros.Add(name);
        Size.Y = CalculateRequiredHeight();
    }
}
