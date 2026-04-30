using FFXIVClientStructs.FFXIV.Client.Game;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.LuaMacro.Wrappers;

public unsafe class StatusWrapper(Status status) : IWrapper
{
    [LuaDocs][Changelog("12.22")] public ushort StatusId => status.StatusId;
    [LuaDocs][Changelog("12.22")] public ushort Param => status.Param;
    [LuaDocs][Changelog("12.22")] public float RemainingTime => status.RemainingTime;
    [LuaDocs][Changelog("12.22")] public EntityWrapper SourceObject => new(status.SourceObject);
}
