using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui.Tabs;

public class HelpTab(HelpLuaTab _luaTab, HelpCliTab _cliTab, HelpCommandsTab _commandsTab)
{
    public void Draw()
    {
        ImGuiEx.EzTabBar("Tabs",
            ("General", HelpGeneralTab.DrawTab, null, false),
            ("Commands", _commandsTab.DrawTab, null, false),
            ("Lua", _luaTab.DrawTab, null, false),
            ("Cli", _cliTab.DrawTab, null, false),
            ("Clicks", HelpClicksTab.DrawTab, null, false),
            ("Keys & Sends", HelpKeysTab.DrawTab, null, false));
    }
}
