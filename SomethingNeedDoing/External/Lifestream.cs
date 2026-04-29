using ECommons.EzIpcManager;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.External;

public class Lifestream : IPC
{
    public override string Name => "Lifestream";
    public override string Repo => Repos.Limiana;

    [EzIPC]
    [LuaFunction(
        description: "Executes a command",
        parameterDescriptions: ["command"])]
    public Action<string> ExecuteCommand = null!;

    [EzIPC]
    [LuaFunction(description: "Checks if the plugin is busy")]
    public Func<bool> IsBusy = null!;

    [EzIPC]
    [LuaFunction(description: "Aborts the current operation")]
    public Action Abort = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["world"])]
    [Changelog("12.12")]
    public Func<string, bool> CanVisitSameDC = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["world"])]
    [Changelog("12.12")]
    public Func<string, bool> CanVisitCrossDC = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["world", "isDcTransfer", "secondaryTeleport", "noSecondaryTeleport", "gateway", "doNotify", "returnToGateway"])]
    [Changelog("12.12")]
    public Action<string, bool, string, bool, int?, bool?, bool?> TPAndChangeWorld = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["territoryType"])]
    [Changelog("12.12")]
    public Func<int?> GetWorldChangeAetheryteByTerritoryType = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["world"])]
    [Changelog("12.12")]
    public Func<string, bool> ChangeWorld = null!;

    [EzIPC]
    [LuaFunction(
        description: "Requests Lifestream to change world of current character to a different one.",
        parameterDescriptions: ["worldId"])]
    [Changelog("12.12")]
    public Func<uint, bool> ChangeWorldById = null!;

    [EzIPC]
    [LuaFunction(
        description: "Requests aethernet teleport to be executed by name, if possible. Must be within an aetheryte or aetheryte shard range.",
        parameterDescriptions: ["destination"])]
    public Func<string, bool> AethernetTeleport = null!;

    [EzIPC]
    [LuaFunction(
        description: "Requests aethernet teleport to be executed by Place Name ID from <see cref=\"PlaceName\"/> sheet, if possible. Must be within an aetheryte or aetheryte shard range.",
        parameterDescriptions: ["placeNameId"])]
    [Changelog("12.12")]
    public Func<uint, bool> AethernetTeleportByPlaceNameId = null!;

    [EzIPC]
    [LuaFunction(
        description: "Requests aethernet teleport to be executed by ID from <see cref=\"Aetheryte\"/> sheet, if possible. Must be within an aetheryte or aetheryte shard range.",
        parameterDescriptions: ["aethernetSheetRowId"])]
    [Changelog("12.12")]
    public Func<uint, bool> AethernetTeleportById = null!;

    [EzIPC]
    [LuaFunction(
        description: "Requests aethernet teleport to be executed by ID from <see cref=\"HousingAethernet\"/> sheet, if possible. Must be within an aetheryte shard range.",
        parameterDescriptions: ["housingAethernetSheetRow"])]
    [Changelog("12.12")]
    public Func<uint, bool> HousingAethernetTeleportById = null!;

    [EzIPC]
    [LuaFunction(description: "Requests aethernet teleport to Firmament. Must be within a Foundation aetheryte range.")]
    [Changelog("12.12")]
    public Func<bool> AethernetTeleportToFirmament = null!;

    [EzIPC]
    [LuaFunction(description: "Retrieves active aetheryte/aetheryte shard ID if present.")]
    [Changelog("12.12")]
    public Func<uint> GetActiveAetheryte = null!;

    [EzIPC]
    [LuaFunction(description: "Retrieves active custom aetheryte/aetheryte shard ID if present.")]
    [Changelog("12.61")]
    public Func<uint> GetActiveCustomAetheryte = null!;

    [EzIPC]
    [LuaFunction(description: "Retrieves active housing aetheryte shard ID if present")]
    [Changelog("12.12")]
    public Func<uint> GetActiveResidentialAetheryte = null!;

    [EzIPC]
    [LuaFunction(
        description: "Teleports to a specific location",
        parameterDescriptions: ["aetheryteId", "subIndex"])]
    public Func<uint, byte, bool> Teleport = null!;

    [EzIPC]
    [LuaFunction(description: "Teleports to free company")]
    public Func<bool> TeleportToFC = null!;

    [EzIPC]
    [LuaFunction(description: "Teleports to home")]
    public Func<bool> TeleportToHome = null!;

    [EzIPC]
    [LuaFunction(description: "Teleports to apartment")]
    public Func<bool> TeleportToApartment = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["territory", "plot"])]
    [Changelog("12.12")]
    public Func<uint, int, Vector3?> GetPlotEntrance = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["enter"])]
    [Changelog("12.12")]
    public Action<bool> EnterApartment = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["innIndex"])]
    [Changelog("12.12")]
    public Action<int?> EnqueueInnShortcut = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["innIndex"])]
    [Changelog("12.12")]
    public Action<int?> EnqueueLocalInnShortcut = null!;

    [EzIPC]
    [LuaFunction]
    [Changelog("12.12")]
    public Func<bool> CanChangeInstance = null!;

    [EzIPC]
    [LuaFunction]
    [Changelog("12.12")]
    [Changelog("12.58", ChangelogType.Fixed)]
    public Func<int> GetNumberOfInstances = null!;

    [EzIPC]
    [LuaFunction(parameterDescriptions: ["number"])]
    [Changelog("12.12")]
    public Action<int> ChangeInstance = null!;

    [EzIPC]
    [LuaFunction]
    [Changelog("12.12")]
    public Func<int> GetCurrentInstance = null!;

    [EzIPC]
    [LuaFunction]
    [Changelog("12.12")]
    public Func<bool?> HasApartment = null!;

    [EzIPC]
    [LuaFunction]
    [Changelog("12.12")]
    public Func<bool?> HasPrivateHouse = null!;

    [EzIPC]
    [LuaFunction]
    [Changelog("12.12")]
    public Func<bool?> HasFreeCompanyHouse = null!;

    [EzIPC]
    [LuaFunction]
    [Changelog("12.12")]
    public Func<bool> CanMoveToWorkshop = null!;

    [EzIPC]
    [LuaFunction]
    [Changelog("12.12")]
    public Action MoveToWorkshop = null!;

    [EzIPC]
    [LuaFunction]
    [Changelog("12.12")]
    public Func<uint> GetRealTerritoryType = null!;
}
