﻿using FFXIVClientStructs.FFXIV.Client.Game;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Requires specific crafting conditions to be met.
/// </summary>
[GenericDoc(
    "Require a certain effect to be present before continuing",
    ["conditions"],
    ["/require \"Well Fed\""]
)]
public class RequireCommand(string text, string[] conditions) : RequireCommandBase(text)
{
    /// <inheritdoc/>
    protected override async Task<bool> CheckCondition(MacroContext context)
    {
        var result = false;
        await context.RunOnFramework(() => result = Svc.ClientState.LocalPlayer?.StatusList.Any(s => s.GameData.Value.Name.ExtractText().EqualsIgnoreCase(conditions.ToString() ?? string.Empty)) ?? false);
        return result;
    }

    /// <inheritdoc/>
    protected override string GetErrorMessage() => "Required condition not found";

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.WaitForCondition(() => CheckCondition(context).Result, MaxWaitModifier?.MaxWaitMilliseconds ?? DefaultTimeout, DefaultCheckInterval);
        await PerformWait(token);
    }

    private unsafe bool InInstance() => GameMain.Instance()->CurrentContentFinderConditionId != 0;
    private unsafe bool HasItem(uint id) => InventoryManager.Instance()->GetInventoryItemCount(id) > 0;
}
