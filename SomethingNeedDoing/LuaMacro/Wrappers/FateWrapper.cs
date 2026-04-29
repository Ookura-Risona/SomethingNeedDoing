using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using SomethingNeedDoing.Core.Interfaces;
using static SomethingNeedDoing.LuaMacro.Modules.FateModule;

namespace SomethingNeedDoing.LuaMacro.Wrappers;

public unsafe class FateWrapper(ushort id) : IWrapper
{
    private FateContext* Fate => FateManager.Instance()->GetFateById(Id);

    [LuaDocs][Changelog("12.22")] public ushort Id => id;
    [LuaDocs] public bool Exists => Fate != null;
    [LuaDocs] public bool InFate => FateManager.Instance()->CurrentFate->FateId == Id;
    [LuaDocs] public FateState State => Fate->State;
    [LuaDocs] public int StartTimeEpoch => Fate->StartTimeEpoch;
    [LuaDocs] public float Duration => Fate->Duration;
    [LuaDocs] public string Name => Fate->Name.ToString();
    [LuaDocs] public float HandInCount => Fate->HandInCount;
    [LuaDocs] public Vector3 Location => Fate->Location;
    [LuaDocs] public float Progress => Fate->Progress;
    [LuaDocs] public bool IsBonus => Fate->IsBonus;
    [LuaDocs] public float Radius => Fate->Radius;
    [LuaDocs] public FateRule Rule => (FateRule)Fate->Rule;
    [LuaDocs] public int Level => Fate->Level;
    [LuaDocs] public int MaxLevel => Fate->MaxLevel;
    [LuaDocs] public ushort FATEChain => Fate->FATEChain;
    [LuaDocs] public uint EventItem => Fate->EventItem;
    [LuaDocs][Changelog("12.22")] public uint IconId => Fate->IconId;
    [LuaDocs] public float DistanceToPlayer => Player.DistanceTo(Location);
}
