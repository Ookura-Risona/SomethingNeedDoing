using FFXIVClientStructs.FFXIV.Client.Game.UI;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.LuaMacro.Wrappers;

public unsafe class LimitBreakWrapper : IWrapper
{
    [LuaDocs] public ushort CurrentUnits => UIState.Instance()->LimitBreakController.CurrentUnits;
    [LuaDocs] public uint BarUnits => UIState.Instance()->LimitBreakController.BarUnits;
    [LuaDocs] public byte BarCount => UIState.Instance()->LimitBreakController.BarCount;
}
