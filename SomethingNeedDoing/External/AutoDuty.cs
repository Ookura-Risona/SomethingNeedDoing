using ECommons.EzIpcManager;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.External;

public class AutoDuty : IPC
{
    public override string Name => "AutoDuty";
    public override string Repo => Repos.Herc;

    [EzIPC]
    [LuaFunction(description: "Opens the list configuration")]
    public readonly Action ListConfig = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets a configuration value",
        parameterDescriptions: ["key"])]
    public readonly Func<string, string?> GetConfig = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets a configuration value",
        parameterDescriptions: ["key", "value"])]
    [Changelog("12.9", ChangelogType.Fixed)]
    [Changelog("13.50", ChangelogType.Changed, "Changed value type to object to accept arrays")]
    public readonly Action<string, object> SetConfig = null!;

    [EzIPC]
    [LuaFunction(
        description: "Runs a specific duty",
        parameterDescriptions: ["territoryType", "loops", "bareMode"])]
    [Changelog("12.9", ChangelogType.Fixed)]
    public readonly Action<uint, int, bool> Run = null!;

    [EzIPC]
    [LuaFunction(description: "Starts the auto duty process", parameterDescriptions: ["startFromZero"])]
    [Changelog("12.9", ChangelogType.Fixed)]
    public readonly Action<bool> Start = null!;

    [EzIPC]
    [LuaFunction(description: "Stops the auto duty process")]
    public readonly Action Stop = null!;

    [EzIPC]
    [LuaFunction(description: "Checks if currently navigating")]
    public readonly Func<bool?> IsNavigating = null!;

    [EzIPC]
    [LuaFunction(description: "Checks if currently looping")]
    public readonly Func<bool?> IsLooping = null!;

    [EzIPC]
    [LuaFunction(description: "Checks if the process is stopped")]
    public readonly Func<bool?> IsStopped = null!;

    [EzIPC]
    [LuaFunction(
        description: "Checks if a content has a path",
        parameterDescriptions: ["contentId"])]
    public readonly Func<uint, bool?> ContentHasPath = null!;
}
