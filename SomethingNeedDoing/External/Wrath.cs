using ECommons.EzIpcManager;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.External;

public class Wrath : IPC
{
    public override string Name => "WrathCombo";
    public override string Repo => Repos.Punish;

    [EzIPC]
    [LuaFunction(
        description: "Checks that Wrath's IPC is completely ready for use")]
    public readonly Func<bool> IPCReady = null!;

    [EzIPC]
    private readonly Func<string, string, Guid?> RegisterForLease = null!;

    [LuaFunction(
        description: "Registers for lease",
        parameterDescriptions: ["scriptName"])]
    [Changelog("12.67")]
    public Guid? Register(string scriptName) => RegisterForLease(Svc.PluginInterface.InternalName, scriptName);

    [EzIPC]
    [LuaFunction(
        description: "Gets the Auto Rotation state")]
    public readonly Func<bool> GetAutoRotationState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the Auto Rotation state",
        parameterDescriptions: ["leaseId", "enabled"])]
    public readonly Func<Guid, bool, SetResult> SetAutoRotationState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Checks if the current job is Auto Rotation ready (as in, `SetAutoRotationState` would set no new Combos/Options, it would only Lock them)")]
    public readonly Func<bool> IsCurrentJobAutoRotationReady = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the current job to be Auto Rotation ready",
        parameterDescriptions: ["leaseId"])]
    public readonly Func<Guid, SetResult> SetCurrentJobAutoRotationReady = null!;

    [EzIPC]
    [LuaFunction(
        description: "Releases control",
        parameterDescriptions: ["leaseId"])]
    public readonly Action<Guid> ReleaseControl = null!;

    [EzIPC]
    [LuaFunction(
        description: "Lists all internal names of combos for the given job ID",
        parameterDescriptions: ["jobId"])]
    [Changelog("13.3")]
    public readonly Func<uint, List<string>?> GetComboNamesForJob = null!;

    [EzIPC]
    [LuaFunction(
        description: "Lists all internal names of options (in a dictionary, keyed to the parent combo's internal name) for the given job ID",
        parameterDescriptions: ["jobId"])]
    [Changelog("13.3")]
    public readonly Func<uint, Dictionary<string, List<string>>?> GetComboOptionNamesForJob = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the state of a Combo, given its internal name (or ID, as a string) (both the newComboState and the newComboAutoModeState should be true to enable them)",
        parameterDescriptions: ["leaseId", "comboInternalName", "newComboState", "newComboAutoModeState"])]
    [Changelog("13.3")]
    public readonly Func<Guid, string, bool, bool, SetResult> SetComboState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets the state of a Combo, given its internal name (or ID, as a string)\n(this returns a table accessible via ComboStateKeys as keys)",
        parameterDescriptions: ["comboInternalName"])]
    [Changelog("13.3")]
    public readonly Func<string, Dictionary<ComboStateKeys, bool>?> GetComboState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the state of a Combo's Option, given its internal name (or ID, as a string)",
        parameterDescriptions: ["leaseId", "optionInternalName", "newOptionState"])]
    [Changelog("13.3")]
    public readonly Func<Guid, string, bool, SetResult> SetComboOptionState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets the state of a Combo's Option, given its internal name (or ID, as a string)",
        parameterDescriptions: ["optionInternalName"])]
    [Changelog("13.3")]
    public readonly Func<string, bool> GetComboOptionState = null!;

    [EzIPC]
    [LuaFunction(
        description: $"Gets the auto rotation config state for the given {nameof(AutoRotationConfigOption)}",
        parameterDescriptions: ["configOption"])]
    public readonly Func<AutoRotationConfigOption, object?> GetAutoRotationConfigState = null!;

    [EzIPC]
    [LuaFunction(
        description: $"Sets the auto rotation config state for the given {nameof(AutoRotationConfigOption)} to the given value (must be of the expected type)",
        parameterDescriptions: ["leaseId", "configOption", "configValue"])]
    public readonly Func<Guid, AutoRotationConfigOption, object, SetResult> SetAutoRotationConfigState = null!;

    public enum AutoRotationConfigOption
    {
        InCombatOnly = 0, //bool
        DPSRotationMode = 1, //DPSRotationMode Enum (or int of enum value)
        HealerRotationMode = 2, //HealerRotationMode Enum (or int of enum value)
        FATEPriority = 3, //bool
        QuestPriority = 4, //bool
        SingleTargetHPP = 5, //int
        AoETargetHPP = 6, //int
        SingleTargetRegenHPP = 7, //int
        ManageKardia = 8, //bool
        AutoRez = 9, //bool
        AutoRezDPSJobs = 10, //bool
        AutoCleanse = 11, //bool
        IncludeNPCs = 12, //bool
        OnlyAttackInCombat = 13, // bool
        OrbwalkerIntegration = 14, // bool
        AutoRezOutOfParty = 15, // bool
        DPSAoETargets = 16, // int
        SingleTargetExcogHPP = 17, // int
    }

    public enum SetResult
    {
        IGNORED = -1,
        Okay = 0,
        OkayWorking = 1,
        IPCDisabled = 10,
        InvalidLease = 11,
        BlacklistedLease = 12,
        Duplicate = 13,
        PlayerNotAvailable = 14,
        InvalidConfiguration = 15,
        InvalidValue = 16,
    }

    public enum DPSRotationMode
    {
        Manual = 0,
        Highest_Max = 1,
        Lowest_Max = 2,
        Highest_Current = 3,
        Lowest_Current = 4,
        Tank_Target = 5,
        Nearest = 6,
        Furthest = 7,
    }

    public enum HealerRotationMode
    {
        Manual = 0,
        Highest_Current = 1,
        Lowest_Current = 2,
    }

    public enum ComboStateKeys
    {
        Enabled,
        AutoMode,
    }
}
