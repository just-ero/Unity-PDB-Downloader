using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Console;

internal partial class Program
{
    private static async Task<bool> TryDownloadCabFile(string pdbDownloadUrl, string cabOutputPath)
    {
        return await AnsiConsole.Progress()
            .StartAsync(ctx => DownloadFile_Progress(ctx, pdbDownloadUrl, cabOutputPath));
    }

    private static async Task<bool> DownloadFile_Progress(ProgressContext ctx, string pdbDownloadUrl, string cabOutputPath)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(pdbDownloadUrl, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode
            || response.Content.Headers.ContentLength is not long contentLength)
        {
            AnsiConsole.MarkupLine($"[IndianRed_1]Web request failed: {(int)response.StatusCode} {response.StatusCode}.[/]");
            return false;
        }

        var downloadTask = ctx.AddTask("[Yellow]Downloading archive...[/]", maxValue: contentLength);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            using var source = await response.Content.ReadAsStreamAsync();
            using var destination = File.OpenWrite(cabOutputPath);

            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer)) != 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead));
                downloadTask.Increment(bytesRead);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return false;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return true;
    }
}
