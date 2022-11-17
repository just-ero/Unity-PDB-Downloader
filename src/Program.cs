using System;
using Spectre.Console;

var dllPath = GetDllPath(args);

if (!TryGetPdbInfo(dllPath, out var guid, out var pdbFileName))
{
    goto End;
}

var outputDirectory = GetOutputDirectory(dllPath);

var pdbDownloadUrl = $"http://symbolserver.unity3d.com/{pdbFileName}.pdb/{guid}/{pdbFileName}.pd_";
var cabFilePath = $"{outputDirectory}/{pdbFileName}.cab";

if (!await TryDownloadCabFile(pdbDownloadUrl, cabFilePath))
{
    goto End;
}

if (!TryExtractCabinetFile(cabFilePath, outputDirectory))
{
    goto End;
}

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[LightGreen]Done![/]");

End:
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("Press any key to exit...");
Console.ReadKey(true);
