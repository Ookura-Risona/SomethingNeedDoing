using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using SomethingNeedDoing.Core.Interfaces;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentOceanFishing;

namespace SomethingNeedDoing.LuaMacro.Wrappers;

public unsafe class OceanFishingWrapper : IWrapper
{
    [LuaDocs] public uint CurrentRoute => EventFramework.Instance()->GetInstanceContentOceanFishing()->CurrentRoute;
    [LuaDocs] public byte TimeOfDay => Svc.Data.GetExcelSheet<IKDRoute>()?.GetRow(CurrentRoute).Time[CurrentZone].Value.TimeOfDay ?? 0;
    [LuaDocs] public OceanFishingStatus Status => EventFramework.Instance()->GetInstanceContentOceanFishing()->Status;
    [LuaDocs][Changelog("12.54", ChangelogType.Changed, "Changed name")] public int CurrentZone => (int)EventFramework.Instance()->GetInstanceContentOceanFishing()->CurrentZone;
    [LuaDocs] public float TimeLeft => EventFramework.Instance()->GetInstanceContentDirector()->ContentDirector.ContentTimeLeft - TimeOffset;
    [LuaDocs] public uint TimeOffset => EventFramework.Instance()->GetInstanceContentOceanFishing()->TimeOffset;
    [LuaDocs] public uint WeatherId => EventFramework.Instance()->GetInstanceContentOceanFishing()->WeatherId;
    [LuaDocs] public bool SpectralCurrentActive => EventFramework.Instance()->GetInstanceContentOceanFishing()->SpectralCurrentActive;
    [LuaDocs] public uint Mission1Type => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission1Type;
    [LuaDocs] public uint Mission2Type => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission2Type;
    [LuaDocs] public uint Mission3Type => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission3Type;
    [LuaDocs] public byte Mission1Goal => GetRow<IKDPlayerMissionCondition>(Mission1Type)!.Value.Unknown1;
    [LuaDocs] public byte Mission2Goal => GetRow<IKDPlayerMissionCondition>(Mission2Type)!.Value.Unknown1;
    [LuaDocs] public byte Mission3Goal => GetRow<IKDPlayerMissionCondition>(Mission3Type)!.Value.Unknown1;
    [LuaDocs] public uint Mission1Progress => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission1Progress;
    [LuaDocs] public uint Mission2Progress => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission2Progress;
    [LuaDocs] public uint Mission3Progress => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission3Progress;
    [LuaDocs] public uint Points => AgentModule.Instance()->GetAgentIKDFishingLog()->Points;
    [LuaDocs] public uint Score => AgentModule.Instance()->GetAgentIKDResult()->Data->Score;
    [LuaDocs] public uint TotalScore => AgentModule.Instance()->GetAgentIKDResult()->Data->TotalScore;
}
