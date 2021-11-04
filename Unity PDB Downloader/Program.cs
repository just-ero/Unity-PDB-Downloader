using Microsoft.Deployment.Compression.Cab;
using PeNet;
using System.Net;
using System.Text.RegularExpressions;

if (args.Length == 0) return;

string dllPath = args[0].ToString();
var cvInfoPbd70 = new PeFile(dllPath).ImageDebugDirectory[0].CvInfoPdb70;
string sig = cvInfoPbd70.Signature.ToString().Replace("-", "").ToUpper() + "1";

Console.WriteLine($"PE Header GUID: {sig}");
Console.WriteLine();

string outputDir = null;
do
{
    if (outputDir != null)
        Console.WriteLine("The directory specified was not found!");

    Console.Write("Specify an output directory (leave blank to place PDB in DLL's directory): ");
    Console.WriteLine();

    outputDir = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(outputDir))
        outputDir = dllPath[..dllPath.LastIndexOf('\\')];
} while (!Directory.Exists(outputDir));

var rgx = new Regex(@"[^\\]*(?=[.][\w]+$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
var matches = rgx.Matches(cvInfoPbd70.PdbFileName);

if (matches.Count < 1)
{
    Console.WriteLine("No PDB found in DLL's PE header.");
    Console.ReadKey();
    return;
}

string dlUrl = @"http://symbolserver.unity3d.com/" + matches[0] + ".pdb/" + sig + "/" + matches[0] + ".pd_";
string cabPath = outputDir + matches[0] + ".cab";

if (!File.Exists(cabPath))
{
    Console.WriteLine($"Downloading from {dlUrl}");
    try
    {
        using var webClient = new WebClient();
        webClient.DownloadFile(dlUrl, cabPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine("CAB download failed:");
        Console.WriteLine();
        Console.WriteLine(ex);
        Console.ReadKey();
        return;
    }
}

var cab = new CabInfo(cabPath);
cab.Unpack(outputDir);
File.Delete(cabPath);