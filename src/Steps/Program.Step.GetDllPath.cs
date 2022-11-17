using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Spectre.Console;
using UnityPdbDl.Extensions;

internal partial class Program
{
    private static string GetDllPath(string[] args)
    {
        if (args.FirstOrDefault() is string dllPath)
        {
            dllPath = Path.GetFullPath(dllPath);

            if (File.Exists(dllPath)
                && Path.GetFileName(dllPath).Equals("UnityPlayer.dll", StringComparison.OrdinalIgnoreCase))
            {
                return dllPath;
            }
            else
            {
                AnsiConsole.MarkupLine("[IndianRed_1]Provided path was not a valid UnityPlayer.dll path![/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[IndianRed_1]Path to game's UnityPlayer.dll file was not provided![/]");
        }

        AnsiConsole.MarkupLine("[LightGreen]Note:[/] you can simply drag the file onto the executable.");

        if (TryFindModuleFromRunningGame(out var gameDllPath))
        {
            if (AnsiConsole.Confirm("[Yellow]Is this correct?[/]", true))
            {
                dllPath = gameDllPath;
            }
            else
            {
                dllPath = GetDllPathFromPrompt();
            }

            AnsiConsole.WriteLine();
        }
        else
        {
            dllPath = GetDllPathFromPrompt();
        }

        return dllPath;
    }

    private static bool TryFindModuleFromRunningGame([NotNullWhen(true)] out string? dllPath)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[Yellow]Looking for open Unity games instead...[/]");

        var (found, dll, gameName) = AnsiConsole.Progress()
            .AutoClear(true)
            .HideCompleted(true)
            .Start(FindModule_Progress);

        if (found)
        {
            AnsiConsole.MarkupLine($"[Yellow]Found [LightGreen]{gameName!}[/].[/]");

            dllPath = dll!;
            return true;
        }
        else
        {
            AnsiConsole.MarkupLine("[IndianRed_1]None found![/]");
            AnsiConsole.WriteLine();

            dllPath = null;
            return false;
        }
    }

    private static (bool, string?, string?) FindModule_Progress(ProgressContext ctx)
    {
        bool found = false;
        string? dll = null, gameName = null;

        var processes = Process.GetProcesses();
        var processesTask = ctx.AddTask("[Yellow]Polling open processes...[/]", maxValue: processes.Length);

        for (int i = 0; i < processes.Length; i++)
        {
            var process = processes[i];

            if (found || process.MainWindowHandle == 0)
            {
                goto Next;
            }

            var modules = process.Modules;
            var modulesTask = ctx.AddTask("[Yellow]Polling process' modules...[/]", maxValue: modules.Count);

            for (int j = 0; j < modules.Count; j++)
            {
                var module = modules[j];

                if (!found && module.ModuleName.Equals("UnityPlayer.dll", StringComparison.OrdinalIgnoreCase))
                {
                    gameName = process.ProcessName;
                    dll = module.FileName;

                    found = true;
                }

                module.Dispose();
                modulesTask.Increment(1);
            }

        Next:
            process.Dispose();
            processesTask.Increment(1);
        }

        return (found, dll, gameName);
    }

    private static string GetDllPathFromPrompt()
    {
        var relativeDllPath = AnsiConsole.Prompt(
            new TextPrompt<string>("[CornflowerBlue]Enter the full path to the game's UnityPlayer.dll:[/]")
            .Validate(static path =>
            {
                path = Path.GetFullPath(path.Strip('"'));

                if (!File.Exists(path))
                {
                    return ValidationResult.Error("[IndianRed_1]File does not exist![/]");
                }

                if (!Path.GetFileName(path).Equals("UnityPlayer.dll", StringComparison.OrdinalIgnoreCase))
                {
                    return ValidationResult.Error("[IndianRed_1]File is not a UnityPlayer.dll![/]");
                }

                return ValidationResult.Success();
            }));

        return Path.GetFullPath(relativeDllPath.Strip('"'));
    }
}
