using System;
using System.IO;
using Microsoft.Deployment.Compression.Cab;
using Spectre.Console;

internal partial class Program
{
    private static bool TryExtractCabinetFile(string cabFilePath, string outputDirectory)
    {
        AnsiConsole.MarkupLine("[Yellow]Unpacking PDB...[/]");

        try
        {
            new CabInfo(cabFilePath).Unpack(outputDirectory);
        }
        catch (NotSupportedException) { }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }

        return TryDeleteCabinetFile(cabFilePath);
    }

    private static bool TryDeleteCabinetFile(string cabFilePath)
    {
        AnsiConsole.MarkupLine("[Yellow]Deleting archive...[/]");

        try
        {
            File.Delete(cabFilePath);
            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return false;
        }
    }
}
