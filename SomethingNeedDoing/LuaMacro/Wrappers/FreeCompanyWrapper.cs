using FFXIVClientStructs.FFXIV.Client.UI.Info;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.LuaMacro.Wrappers;

public unsafe class FreeCompanyWrapper : IWrapper
{
    private InfoProxyFreeCompany* FreeCompanyProxy => (InfoProxyFreeCompany*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->UIModule->GetInfoModule()->GetInfoProxyById(InfoProxyId.FreeCompany);

    [LuaDocs] public FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany GrandCompany => FreeCompanyProxy->GrandCompany;
    [LuaDocs] public byte Rank => FreeCompanyProxy->Rank;
    [LuaDocs] public int OnlineMemebers => FreeCompanyProxy->OnlineMembers;
    [LuaDocs] public int TotalMembers => FreeCompanyProxy->TotalMembers;
    [LuaDocs][Changelog("13.4", ChangelogType.Fixed)] public string Name => FreeCompanyProxy->NameString;
    [LuaDocs] public ulong Id => FreeCompanyProxy->Id;
}
