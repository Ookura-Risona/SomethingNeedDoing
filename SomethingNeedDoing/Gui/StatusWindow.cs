using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Managers;

namespace SomethingNeedDoing.Gui;

public class StatusWindow : Window
{
    private readonly IMacroScheduler _scheduler;
    private readonly MacroHierarchyManager _macroHierarchy;
    private readonly TriggerEventManager _triggerEventManager;
    private readonly TitleBarButton _minimiseBtn;
    private bool _minimised;
    private bool _showTriggerEvents;

    public StatusWindow(IMacroScheduler scheduler, MacroHierarchyManager macroHierarchy, TriggerEventManager triggerEventManager) : base($"{P.Name} - 宏状态###{P.Name}_{nameof(StatusWindow)}", ImGuiWindowFlags.NoScrollbar)
    {
        _scheduler = scheduler;
        _macroHierarchy = macroHierarchy;
        _triggerEventManager = triggerEventManager;
        Size = new Vector2(500, 300);
        SizeCondition = ImGuiCond.FirstUseEver;
        _minimiseBtn = new TitleBarButton()
        {
            Icon = FontAwesomeIcon.Minus,
            IconOffset = new Vector2(1.5f, 1),
            Priority = int.MinValue,
            Click = _ =>
            {
                _minimised = !_minimised;
                _minimiseBtn!.Icon = _minimised ? FontAwesomeIcon.WindowMaximize : FontAwesomeIcon.Minus;
            },
            ShowTooltip = () => { using var _ = ImRaii.Tooltip(); ImGuiEx.Text(_minimised ? "显示所有宏" : "仅显示运行中的宏"); },
            AvailableClickthrough = true,
        };
        TitleBarButtons.Add(_minimiseBtn);
    }

    public override void Draw()
    {
        if (ImGui.Button(_showTriggerEvents ? "隐藏触发事件" : "显示触发事件"))
            _showTriggerEvents = !_showTriggerEvents;

        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "|");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "宏状态");

        ImGui.Separator();

        if (_showTriggerEvents)
        {
            DrawTriggerEventsSection();
            ImGui.Separator();
        }

        var macros = _minimised ? _scheduler.GetMacros().Where(m => m.State is MacroState.Running or MacroState.Paused) : _scheduler.GetMacros();
        var parents = macros.Where(m => _macroHierarchy.GetParentMacro(m.Id) == null).ToList();

        foreach (var parent in parents)
        {
            DrawMacro(parent);

            if (_macroHierarchy.GetChildMacros(parent.Id) is { Count: > 0 } children)
                foreach (var childMacro in children)
                    DrawMacro(childMacro, true);
        }
    }

    private void DrawMacro(IMacro macro, bool indent = false)
    {
        using var _ = ImRaii.PushIndent(condition: indent); // TODO: this doesn't work?
        using var id = ImRaii.PushId(macro.Id);
        var (statusColor, statusIcon) = GetStatusInfo(macro.State);
        ImGuiEx.Icon(statusColor, statusIcon);
        ImGui.SameLine();
        ImGuiEx.IconWithText(ImGuiUtils.Icons.GetMacroIcon(macro), macro.Name);
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 100);
        DrawControlButtons(macro);
    }

    private void DrawControlButtons(IMacro macro)
    {
        if (macro.State == MacroState.Paused)
        {
            if (ImGuiUtils.IconButton(FontAwesomeIcon.Play, "继续"))
                _scheduler.ResumeMacro(macro.Id);
        }
        else
        {
            if (ImGuiUtils.IconButton(FontAwesomeIcon.Pause, "暂停"))
                _scheduler.PauseMacro(macro.Id);
        }

        ImGui.SameLine();
        if (ImGuiUtils.IconButton(FontAwesomeIcon.Stop, "停止"))
            _scheduler.StopMacro(macro.Id);
    }

    private (Vector4 color, FontAwesomeIcon icon) GetStatusInfo(MacroState state) => state switch
    {
        MacroState.Running => (ImGuiColors.HealerGreen, FontAwesomeIcon.Spinner),
        MacroState.Paused => (ImGuiColors.DalamudOrange, FontAwesomeIcon.Pause),
        MacroState.Error => (ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationTriangle),
        MacroState.Completed => (ImGuiColors.ParsedBlue, FontAwesomeIcon.CheckCircle),
        MacroState.Ready => (ImGuiColors.DalamudGrey, FontAwesomeIcon.Circle),
        _ => (ImGuiColors.DalamudGrey, FontAwesomeIcon.QuestionCircle)
    };

    private void DrawTriggerEventsSection()
    {
        ImGuiEx.Text(ImGuiColors.DalamudOrange, "已注册的触发事件");
        ImGui.Spacing();

        var triggerEvents = _triggerEventManager.EventHandlers;
        if (triggerEvents.Count == 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudGrey, "没有已注册的触发事件");
            return;
        }

        foreach (var kvp in triggerEvents.OrderBy(x => x.Key.ToString()))
        {
            using var tree = ImRaii.TreeNode($"{kvp.Key} ({kvp.Value.Count})");
            if (!tree) return;
            foreach (var function in kvp.Value.OrderBy(f => f.Macro.Name))
            {
                using var id = ImRaii.PushId($"{function.Macro.Id}_{function.FunctionName}");

                var displayText = string.IsNullOrEmpty(function.FunctionName) ? function.Macro.Name : $"{function.Macro.Name} → {function.FunctionName}";
                ImGuiEx.IconWithText(ImGuiUtils.Icons.GetMacroIcon(function.Macro), displayText);

                if (kvp.Key == TriggerEvent.OnAddonEvent && !string.IsNullOrEmpty(function.AddonName))
                {
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.DalamudGrey, $"({function.AddonName}: {function.AddonEventType})");
                }
            }
        }
    }
}
