using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.UIHelpers.AddonMasterImplementations;
using System.Reflection;

namespace SomethingNeedDoing.Gui.Tabs;

public static class HelpClicksTab
{
    public static void DrawTab()
    {
        using var child = ImRaii.Child(nameof(HelpClicksTab));
        ImGuiUtils.Section("点击命令", () =>
        {
            ImGui.TextWrapped("点击命令可用于与游戏UI元素交互。您可以在宏中使用这些命令。");
            ImGui.TextWrapped("红色项是属性，它们本身具有方法（不可直接调用）。");
        });

        ImGuiUtils.Section("可用的点击命令", () =>
        {
            using var _ = ImRaii.Child("ClicksList", new(-1, 300), true);
            foreach (var name in typeof(AddonMaster).Assembly.GetTypes()
            .Where(type => type.FullName!.StartsWith($"{typeof(AddonMaster).FullName}+") && type.DeclaringType == typeof(AddonMaster))
            .SelectMany(type => type.GetMembers()
                .Where(m => (m is MethodInfo info && !info.IsSpecialName && info.DeclaringType != typeof(object)) || (m is PropertyInfo prop && prop.GetAccessors().Length > 0 && prop.PropertyType.IsClass && prop.PropertyType.Namespace == type.Namespace))
                .Select(member => $"{(member is MethodInfo ? "m" : "p")}{type.Name} {member.Name}")))
            {
                var isProperty = name.StartsWith('p');
                var color = isProperty ? ImGuiColors.DalamudRed : ImGui.GetStyle().Colors[(int)ImGuiCol.Text];

                using var textColor = ImRaii.PushColor(ImGuiCol.Text, color);
                if (ImGui.Selectable($"/click {name[1..]}"))
                    Copy($"/click {name[1..]}");
            }
        });
    }
}
