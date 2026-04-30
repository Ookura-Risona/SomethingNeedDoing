using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Managers;

namespace SomethingNeedDoing.Gui.Tabs;

public static class HelpGeneralTab
{
    public static void DrawTab()
    {
        using var child = ImRaii.Child(nameof(HelpGeneralTab));
        ImGuiUtils.Section(P.Name, () =>
        {
            ImGui.TextWrapped($"{P.Name} 是对原生宏系统的扩展，提供智能辅助、额外命令和修饰符，以及无限制的宏数量。");
            ImGui.TextWrapped("它还支持使用 Lua 编写脚本，让您可以编写比原生系统更复杂的宏。");
        });

        ImGuiUtils.Section("状态监控", () =>
        {
            ImGui.TextWrapped("状态窗口显示所有当前正在运行的宏及其当前状态");
            ImGui.Spacing();

            ImGuiEx.Text(ImGuiColors.DalamudOrange, "宏状态");
            ImGui.BulletText("就绪: 宏已加载但尚未开始运行");
            ImGui.BulletText("运行中: 宏正在执行中");
            ImGui.BulletText("已暂停: 宏执行已被临时停止");
            ImGui.BulletText("已完成: 宏执行已完成");
            ImGui.BulletText("失败: 宏执行过程中遇到错误");
        });

        ImGuiUtils.Section("触发事件", () =>
        {
            ImGui.TextWrapped("宏可以配置为根据特定游戏事件自动触发:");
            Enum.GetNames<TriggerEvent>().Each(name => ImGui.BulletText(name));

            ImGui.TextWrapped("Lua 宏也可以配置为让单个函数自动触发（前提是脚本已在运行中）。");
            ImGui.TextWrapped($"任何以 TriggerEvent 名称开头的函数，在脚本启动时都会注册到 {nameof(TriggerEventManager)}。");
            ImGui.TextWrapped($"特别是对于 {TriggerEvent.OnAddonEvent}，事件名称后必须跟随插件名称和事件类型，例如");
            ImGui.SameLine();
            ImGuiEx.Text(ImGuiColors.DalamudOrange, "OnAddonEvent_SelectYesno_PostSetup");
        });

        ImGuiUtils.Section("宏元数据", () =>
        {
            ImGuiEx.Text(ImGuiColors.DalamudOrange, "常规");
            ImGui.TextWrapped("宏可以配置元数据，为框架提供有关宏执行的特定配置。");
            ImGui.TextWrapped("元数据可以在宏库中选择宏时，在宏设置部分进行编辑。");
            ImGui.TextWrapped("可以使用上述部分提供的按钮将元数据写入文件。这对于远程存储的宏（如 GitHub）至关重要，框架需要知道导入时应使用的设置");

            ImGuiEx.Text(ImGuiColors.DalamudOrange, "依赖与冲突");
            ImGui.TextWrapped("宏也可以配置为需要其他插件才能运行，如果未满足要求将打印消息。");
            ImGui.TextWrapped("类似地，宏可以配置为在运行时禁用其他插件，但这要求插件已在框架中预定义以支持此功能。");
            ImGui.TextWrapped("与插件依赖类似，宏可以支持依赖其他宏，无论是本地宏（已加载到 SND 中）还是远程宏（如 GitHub）");
        });

        ImGuiUtils.Section("Git 集成", () =>
        {
            ImGui.TextWrapped("宏现在可以连接到 GitHub URL 并自动转换为 \"Git 宏\"");
            ImGui.TextWrapped("Git 宏支持在发布新版本时自动更新（在启动时检查），并通过版本历史模态（在宏设置部分中找到）在不同版本间切换");
            ImGui.TextWrapped("当宏元数据中包含存储库 URL 时，会自动识别为 Git 宏。要将 Git 宏转换回本地宏，只需清除 URL 或在设置中单击重置 Git 数据按钮");
        });
    }
}
