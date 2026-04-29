using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace SomethingNeedDoing.LuaMacro.Modules;

public unsafe class QuestsModule : LuaModuleBase
{
    public override string ModuleName => "Quests";

    [LuaFunction] public bool IsQuestAccepted(ushort id) => QuestManager.Instance()->IsQuestAccepted(id);
    [LuaFunction] public List<uint> GetAcceptedQuests() => [.. Svc.Data.GetExcelSheet<Quest>(Svc.ClientState.ClientLanguage)!.Where(x => IsQuestAccepted((ushort)x.RowId)).Select(x => x.RowId)];
    [LuaFunction] public bool IsQuestComplete(ushort id) => QuestManager.IsQuestComplete(id);
    [LuaFunction] public byte GetQuestSequence(ushort id) => QuestManager.GetQuestSequence(id);
    [LuaFunction] public bool IsLeveAccepted(ushort id) => QuestManager.Instance()->LeveQuests.ToArray().Any(q => q.LeveId == id);
    [LuaFunction] public MonsterNoteRankInfo GetMonsterNoteRankInfo(int index) => MonsterNoteManager.Instance()->RankData[index];
}
