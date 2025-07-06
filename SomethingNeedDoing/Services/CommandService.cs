using Dalamud.Interface.Windowing;
using ECommons;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Gui;

namespace SomethingNeedDoing.Services;

public class CommandService
{
    public string MainCommand => "/somethingneeddoing";
    public string[] Aliases => ["/snd", "/pcraft"];

    private readonly IMacroScheduler _macroScheduler;
    private readonly WindowSystem _windowSystem;

    private class SubCommand(string command, string description, Action<string> handler, bool showInHelp = true)
    {
        public string Command { get; } = command;
        public string Description { get; } = description;
        public Action<string> Handler { get; } = handler;
        public bool ShowInHelp { get; } = showInHelp;
        public List<SubCommand> SubCommands { get; } = [];
    }

    private readonly SubCommand _rootCommand;

    public CommandService(IMacroScheduler macroScheduler, WindowSystem windowSystem)
    {
        _macroScheduler = macroScheduler;
        _windowSystem = windowSystem;

        _rootCommand = new("", "Open the main window", _ => _windowSystem.Toggle<MainWindow>());
        var runCommand = new SubCommand("run", "Run a macro, the name must be unique.", HandleRunCommand);
        runCommand.SubCommands.Add(new("loop", "Run a macro and then loop N times, the name must be unique.", HandleRunLoopCommand));

        var pauseCommand = new SubCommand("pause", "Pause the given executing macro.", HandlePauseCommand);
        pauseCommand.SubCommands.Add(new("loop", "Pause the given executing macro at the next loop point.", HandlePauseLoopCommand));
        pauseCommand.SubCommands.Add(new("all", "Pause all running macros.", _ => HandlePauseAllCommand()));

        var resumeCommand = new SubCommand("resume", "Resume the given paused macro.", HandleResumeCommand);
        resumeCommand.SubCommands.Add(new("all", "Resume all paused macros.", _ => HandleResumeAllCommand()));

        var stopCommand = new SubCommand("stop", "Stop the given executing macro.", HandleStopCommand);
        stopCommand.SubCommands.Add(new("loop", "Stop the given executing macro at the next loop point.", HandleStopLoopCommand));
        stopCommand.SubCommands.Add(new("all", "Stop all running macros.", _ => _macroScheduler.StopAllMacros()));

        var helpCommand = new SubCommand("help", "Show the help window.", _ => ShowHelp(), false);
        //var cfgCommand = new SubCommand("cfg", "Change a configuration value.", HandleConfigCommand);
        var statusCommand = new SubCommand("status", "Toggle the running macros window.", _ => _windowSystem.Toggle<StatusWindow>());
        var changelogCommand = new SubCommand("changelog", "Toggle the changelog window.", _ => _windowSystem.Toggle<ChangelogWindow>());

        _rootCommand.SubCommands.AddRange([runCommand, pauseCommand, stopCommand, helpCommand, resumeCommand, statusCommand, changelogCommand]);

        RegisterCommands();
    }

    public List<(string Command, string Description)> GetCommandData()
    {
        var result = new List<(string, string)>();
        foreach (var cmd in _rootCommand.SubCommands)
        {
            result.Add((cmd.Command, cmd.Description));
            foreach (var subCmd in cmd.SubCommands)
                result.Add(($"{cmd.Command} {subCmd.Command}", subCmd.Description));
        }
        return result;
    }

    public void RegisterCommands()
    {
        EzCmd.Add(MainCommand, OnChatCommand, "Open a window to edit various settings.", displayOrder: int.MinValue);
        Aliases.ToList().ForEach(a => EzCmd.Add(a, OnChatCommand, $"{MainCommand} Alias"));
    }

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();

        if (arguments == string.Empty)
        {
            _rootCommand.Handler(arguments);
            return;
        }

        var parts = arguments.Split(' ', 2);
        var subCommand = parts[0].ToLowerInvariant();
        var subArgs = parts.Length > 1 ? parts[1] : string.Empty;

        var cmd = _rootCommand.SubCommands.FirstOrDefault(c => c.Command == subCommand);
        if (cmd == null)
        {
            ShowHelp();
            return;
        }

        if (subArgs == string.Empty)
        {
            cmd.Handler(subArgs);
            return;
        }

        var subParts = subArgs.Split(' ', 2);
        var subSubCommand = subParts[0].ToLowerInvariant();
        var subSubArgs = subParts.Length > 1 ? subParts[1] : string.Empty;

        if (cmd.SubCommands.FirstOrDefault(c => c.Command == subSubCommand) is { } subCmd)
            subCmd.Handler(subSubArgs);
        else
            cmd.Handler(subArgs);
    }

    private void ShowHelp()
    {
        Svc.Chat.PrintMessage("Commands:");
        foreach (var cmd in _rootCommand.SubCommands.Where(c => c.ShowInHelp))
        {
            Svc.Chat.PrintMessage($"{MainCommand} {cmd.Command} - {cmd.Description}");
            foreach (var subCmd in cmd.SubCommands)
                Svc.Chat.PrintMessage($"{MainCommand} {cmd.Command} {subCmd.Command} - {subCmd.Description}");
        }
        Svc.Chat.PrintMessage($"{MainCommand} id <macro name> - Get the macro ID for a macro name.");
    }

    private void HandleRunCommand(string arguments)
    {
        var macroName = arguments.Trim('"');
        if (C.GetMacroByName(macroName) is { } macro)
            _ = _macroScheduler.StartMacro(macro);
    }

    private void HandleRunLoopCommand(string arguments)
    {
        var parts = arguments.Split(' ', 2);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var loopCount))
        {
            Svc.Chat.PrintError("Invalid loop command format. Usage: /snd run loop <count> \"<macro name>\"");
            return;
        }

        var macroName = parts[1].Trim('"');
        if (C.GetMacroByName(macroName) is { } macro)
            _ = _macroScheduler.StartMacro(macro, loopCount);
    }

    private void HandlePauseCommand(string arguments)
    {
        var macroName = arguments.Trim('"');
        if (C.GetMacroByName(macroName) is { } macro)
            _macroScheduler.PauseMacro(macro.Id);
        else
            Svc.Chat.PrintError($"Macro '{macroName}' not found.");
    }

    private void HandlePauseLoopCommand(string arguments)
    {
        var macroName = arguments.Trim('"');
        if (C.GetMacroByName(macroName) is { } macro)
            _macroScheduler.PauseAtNextLoop(macro.Id);
        else
            Svc.Chat.PrintError($"Macro '{macroName}' not found.");
    }

    private void HandlePauseAllCommand()
    {
        foreach (var macro in _macroScheduler.GetMacros())
            _macroScheduler.PauseMacro(macro.Id);
        Svc.Chat.PrintMessage("All running macros paused.");
    }

    private void HandleResumeCommand(string arguments)
    {
        var macroName = arguments.Trim('"');
        if (C.GetMacroByName(macroName) is { } macro)
            _macroScheduler.ResumeMacro(macro.Id);
        else
            Svc.Chat.PrintError($"Macro '{macroName}' not found.");
    }

    private void HandleResumeAllCommand()
    {
        foreach (var macro in _macroScheduler.GetMacros())
            _macroScheduler.ResumeMacro(macro.Id);
        Svc.Chat.PrintMessage("All paused macros resumed.");
    }

    private void HandleStopCommand(string arguments)
    {
        var macroName = arguments.Trim('"');
        if (C.GetMacroByName(macroName) is { } macro)
            _macroScheduler.StopMacro(macro.Id);
        else
            Svc.Chat.PrintError($"Macro '{macroName}' not found.");
    }

    private void HandleStopLoopCommand(string arguments)
    {
        var macroName = arguments.Trim('"');
        if (C.GetMacroByName(macroName) is { } macro)
            _macroScheduler.StopAtNextLoop(macro.Id);
        else
            Svc.Chat.PrintError($"Macro '{macroName}' not found.");
    }

    //private void HandleConfigCommand(string arguments)
    //{
    //    var args = arguments.Split(" ");
    //    if (args.Length != 2)
    //    {
    //        Svc.Chat.PrintError("Invalid config command format. Usage: /snd cfg <setting> <value>");
    //        return;
    //    }
    //    C.SetProperty(args[0], args[1]);
    //}
}
