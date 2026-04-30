using FFXIVClientStructs.FFXIV.Client.Game;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.LuaMacro.Wrappers;

public unsafe class ActionWrapper(uint actionId) : IWrapper
{
    private ActionManager* Am => ActionManager.Instance();

    [LuaDocs] public uint AdjustedActionId => Am->GetAdjustedActionId(actionId);
    [LuaDocs] public float RecastTimeElapsed => Am->GetRecastTimeElapsed(ActionType.Action, AdjustedActionId);
    [LuaDocs] public float RealRecastTimeElapsed => Am->GetRecastTimeElapsed(ActionType.Action, actionId);
    [LuaDocs] public float RecastTime => Am->GetRecastTime(ActionType.Action, AdjustedActionId);
    [LuaDocs] public float RealRecastTime => Am->GetRecastTime(ActionType.Action, actionId);
    [LuaDocs] public float SpellCooldown => Math.Abs(RecastTime - RecastTimeElapsed);
    [LuaDocs] public float RealSpellCooldown => Math.Abs(RealRecastTime - RealRecastTimeElapsed);

    [LuaDocs(description: "Returns LogMessage id")]
    [Changelog("12.69")]
    public uint GetActionStatus() => Am->GetActionStatus(ActionType.Action, actionId);
}
