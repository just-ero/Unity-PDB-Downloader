using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using PeNet;
using Spectre.Console;
using UnityPdbDl.Extensions;

internal partial class Program
{
    private static bool TryGetPdbInfo(string dllPath, [NotNullWhen(true)] out string? guid, [NotNullWhen(true)] out string? pdbFileName)
    {
        guid = null;
        pdbFileName = null;

        var peFile = new PeFile(dllPath);
        var debugDirectory = peFile.ImageDebugDirectory;

        if (debugDirectory is null)
        {
            AnsiConsole.MarkupLine("[IndianRed_1]Could not retrieve debug directory information from PE header![/]");
            return false;
        }

        var cvInfoPdb70 = debugDirectory.FirstOrDefault()?.CvInfoPdb70;

        if (cvInfoPdb70 is null)
        {
            AnsiConsole.MarkupLine("[IndianRed_1]Could not retrieve PDB information from debug directory![/]");
            return false;
        }

        var matches = GetPdbFileNameRegex().Matches(cvInfoPdb70.PdbFileName);
        var match = matches.FirstOrDefault();

        if (match is null)
        {
            AnsiConsole.MarkupLine("[IndianRed_1]Could not find PDB file name in PDB information.[/]");
            return false;
        }

        guid = cvInfoPdb70.Signature.ToString().Strip('-').ToUpper() + '1';
        pdbFileName = match.Value;

        return true;
    }

    [GeneratedRegex(@"[^\\]*(?=[.][\w]+$)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex GetPdbFileNameRegex();
}
