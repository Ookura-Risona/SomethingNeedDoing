namespace SomethingNeedDoing.LuaMacro.Modules;

public class DalamudModule : LuaModuleBase
{
    public override string ModuleName => "Dalamud";

    [LuaFunction] public void Log(object msg) => Svc.Log.Info(msg.ToString() ?? string.Empty);
    [LuaFunction] public void LogDebug(object msg) => Svc.Log.Debug(msg.ToString() ?? string.Empty);
    [LuaFunction] public void LogVerbose(object msg) => Svc.Log.Verbose(msg.ToString() ?? string.Empty);
}
