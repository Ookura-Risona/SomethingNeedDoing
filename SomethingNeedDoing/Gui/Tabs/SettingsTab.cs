using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Gui.Modals;

namespace SomethingNeedDoing.Gui.Tabs;

public static class SettingsTab
{
    public static void DrawTab()
    {
        using var _ = ImRaii.Child("SettingsTab", Vector2.Create(-1), false);

        ImGuiUtils.Section("常规设置", () =>
        {
            var chatChannel = C.ChatType;
            if (ImGuiEx.EnumCombo("输出频道", ref chatChannel))
            {
                C.ChatType = chatChannel;
                C.Save();
            }

            var errorChannel = C.ErrorChatType;
            if (ImGuiEx.EnumCombo("报错输出频道", ref errorChannel))
            {
                C.ErrorChatType = errorChannel;
                C.Save();
            }

            var propagatePause = C.PropagateControlsToChildren;
            if (ImGui.Checkbox("将控制应用到子宏", ref propagatePause))
            {
                C.PropagateControlsToChildren = propagatePause;
                C.Save();
            }
            ImGuiEx.Tooltip("启用时，暂停、继续和停止宏操作也会影响其子宏。");

            var autoOpenStatusWindow = C.AutoOpenStatusWindow;
            if (ImGui.Checkbox("Auto-open Status Window on Script Start", ref autoOpenStatusWindow))
            {
                C.AutoOpenStatusWindow = autoOpenStatusWindow;
                C.Save();
            }
            ImGuiEx.Tooltip("When enabled, the macro status window will automatically open when a script starts running.");
        });

        ImGuiUtils.Section("制作设置", () =>
        {
            var craftSkip = C.CraftSkip;
            if (ImGui.Checkbox("非制作时跳过制作技能", ref craftSkip))
            {
                C.CraftSkip = craftSkip;
                C.Save();
            }

            var smartWait = C.SmartWait;
            if (ImGui.Checkbox("智能等待制作技能", ref smartWait))
            {
                C.SmartWait = smartWait;
                C.Save();
            }

            var qualitySkip = C.QualitySkip;
            if (ImGui.Checkbox("HQ率100%时跳过加工技能", ref qualitySkip))
            {
                C.QualitySkip = qualitySkip;
                C.Save();
            }

            var loopTotal = C.LoopTotal;
            if (ImGui.Checkbox("将/loop计数视为总迭代次数", ref loopTotal))
            {
                C.LoopTotal = loopTotal;
                C.Save();
            }

            var loopEcho = C.LoopEcho;
            if (ImGui.Checkbox("始终回显/loop命令", ref loopEcho))
            {
                C.LoopEcho = loopEcho;
                C.Save();
            }

            var useCraftLoopTemplate = C.UseCraftLoopTemplate;
            if (ImGui.Checkbox("使用CraftLoop模板", ref useCraftLoopTemplate))
            {
                C.UseCraftLoopTemplate = useCraftLoopTemplate;
                C.Save();
            }

            if (useCraftLoopTemplate)
            {
                var craftLoopTemplate = C.CraftLoopTemplate;
                if (ImGui.InputTextMultiline("CraftLoop模板", ref craftLoopTemplate, 1000, new Vector2(0, 100)))
                {
                    C.CraftLoopTemplate = craftLoopTemplate;
                    C.Save();
                }

                var craftLoopFromRecipeNote = C.CraftLoopFromRecipeNote;
                if (ImGui.Checkbox("从制作笔记窗口开始制作循环", ref craftLoopFromRecipeNote))
                {
                    C.CraftLoopFromRecipeNote = craftLoopFromRecipeNote;
                    C.Save();
                }

                var craftLoopMaxWait = C.CraftLoopMaxWait;
                if (ImGui.SliderInt("CraftLoop最大等待值", ref craftLoopMaxWait, 1, 10))
                {
                    C.CraftLoopMaxWait = craftLoopMaxWait;
                    C.Save();
                }

                var craftLoopEcho = C.CraftLoopEcho;
                if (ImGui.Checkbox("CraftLoop echo", ref craftLoopEcho))
                {
                    C.CraftLoopEcho = craftLoopEcho;
                    C.Save();
                }
            }
        });

        ImGuiUtils.Section("错误处理", () =>
        {
            var stopOnError = C.StopOnError;
            if (ImGui.Checkbox("出错时停止", ref stopOnError))
            {
                C.StopOnError = stopOnError;
                C.Save();
            }
            ImGuiEx.Tooltip("仅适用于原生宏。");

            var maxTimeoutRetries = C.MaxTimeoutRetries;
            if (ImGui.SliderInt("最大超时重试次数", ref maxTimeoutRetries, 0, 10))
            {
                C.MaxTimeoutRetries = maxTimeoutRetries;
                C.Save();
            }

            var noisyErrors = C.NoisyErrors;
            if (ImGui.Checkbox("错误提示音", ref noisyErrors))
            {
                C.NoisyErrors = noisyErrors;
                C.Save();
            }

            if (noisyErrors)
            {
                var beepFrequency = C.BeepFrequency;
                if (ImGui.SliderInt("提示音频率", ref beepFrequency, 0, 1000))
                {
                    C.BeepFrequency = beepFrequency;
                    C.Save();
                }

                var beepDuration = C.BeepDuration;
                if (ImGui.SliderInt("提示音时长", ref beepDuration, 0, 1000))
                {
                    C.BeepDuration = beepDuration;
                    C.Save();
                }

                var beepCount = C.BeepCount;
                if (ImGui.SliderInt("提示音次数", ref beepCount, 0, 10))
                {
                    C.BeepCount = beepCount;
                    C.Save();
                }
            }
        });

        ImGuiUtils.Section("Lua选项", () =>
        {
            ImGui.TextWrapped("Lua require路径 (Lua模块搜索路径):");

            var paths = C.LuaRequirePaths.ToArray();
            using (ImRaii.Table("LuaRequirePaths", 2, ImGuiTableFlags.SizingStretchProp))
            {
                for (var index = 0; index < paths.Length; index++)
                {
                    var path = PathHelper.NormalizePath(paths[index]);

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();

                    var isValid = PathHelper.ValidatePath(path);
                    ImGui.TextColored(isValid ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed, $"Path #{index}");
                    ImGuiEx.Tooltip(isValid ? "此路径有效" : "此路径无效");

                    ImGui.TableNextColumn();

                    if (ImGui.InputText($"##Path{index}", ref path, 200))
                    {
                        var newPaths = paths.ToList();
                        newPaths[index] = PathHelper.NormalizePath(path);
                        C.LuaRequirePaths = [.. newPaths.Where(p => !string.IsNullOrWhiteSpace(p))];
                        C.Save();
                    }
                }
            }

            if (ImGui.Button("添加路径"))
            {
                var newPaths = paths.ToList();
                newPaths.Add(string.Empty);
                C.LuaRequirePaths = [.. newPaths];
                C.Save();
            }
        });

        ImGuiUtils.Section("旧版宏导入", () =>
        {
            ImGui.TextWrapped($"从旧版{P.Name}导入宏。不保证完全兼容，但可作为参考导入。\n" +
            "可将旧配置复制到剪贴板后点击导入按钮，或自动尝试查找旧配置文件。");
            ImGui.Spacing();

            if (ImGuiUtils.IconButton(FontAwesomeHelper.IconImport, "导入"))
                MigrationModal.Open(ImGui.GetClipboardText());
        });
    }
}
