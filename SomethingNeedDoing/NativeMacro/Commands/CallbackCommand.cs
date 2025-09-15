﻿using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Executes a callback on a game addon.
/// </summary>
[GenericDoc(
    "Execute a callback on a game addon",
    ["addonName", "updateState", "values"],
    ["/callback \"Synthesis\" true 0", "/callback \"Synthesis\" true 0 <errorif.addonnotfound>"]
)]
public class CallbackCommand(string text, string addonName, bool updateState, object[] values) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            unsafe
            {
                if (!TryGetAddonByName<AtkUnitBase>(addonName, out var addon))
                {
                    if (ErrorIfModifier?.Condition == Modifiers.ErrorCondition.AddonNotFound)
                        throw new MacroException($"Addon {addonName} not found");
                    return;
                }

                if (IsAddonReady(addon))
                {
                    FrameworkLogger.Debug($"Sending callback to {addonName} with args [{string.Join(", ", values)}]");
                    Callback.Fire(addon, updateState, values);
                }
            }
        });

        await PerformWait(token);
    }
}
