using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using SomethingNeedDoing.Services;

namespace SomethingNeedDoing.Gui.Tabs;

public class HelpCliTab(CommandService cmds)
{
    public void DrawTab()
    {
        using var child = ImRaii.Child(nameof(HelpCliTab));
        ImGuiUtils.Section("命令行接口", () => ImGui.TextWrapped("以下命令可在聊天或宏文本中使用。"));

        ImGuiUtils.Section("主命令", () => ImGui.TextUnformatted(cmds.MainCommand), contentFont: UiBuilder.MonoFont);

        ImGuiUtils.Section("别名", () => cmds.Aliases.Each(a => ImGui.TextUnformatted(a)), contentFont: UiBuilder.MonoFont);

        ImGuiUtils.Section("命令列表", () =>
        {
            using var table = ImRaii.Table("CommandsTable", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersInner);
            if (!table) return;

            ImGui.TableSetupColumn("命令", ImGuiTableColumnFlags.WidthFixed, 180 * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("描述", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            foreach (var (name, desc) in cmds.GetCommandData())
            {
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.TextUnformatted($"{cmds.Aliases[0]} {name}");

                ImGui.TableSetColumnIndex(1);
                ImGui.TextWrapped(desc);
            }
        }, contentFont: UiBuilder.MonoFont);
    }
}
