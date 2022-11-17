using System.IO;
using Spectre.Console;

internal partial class Program
{
    private static string GetOutputDirectory(string dllPath)
    {
        var defaultDirectory = Path.GetDirectoryName(dllPath)!;

        return AnsiConsole.Prompt(
            new TextPrompt<string>("[CornflowerBlue]Specify an output directory (leave blank to place PDB in DLL's directory):[/]")
            .DefaultValue(defaultDirectory)
            .HideDefaultValue()
            .Validate(Directory.Exists)
            .AllowEmpty());
    }
}
