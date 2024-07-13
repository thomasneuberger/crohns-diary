using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrohnsDiary.App.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace CrohnsDiary.App.Pages;

public partial class ExportImport
{
    private const string FileNameJson = "CrohnsDiaryData.json";
    private const string FileNameTemplateDownload = "CrohnsData_Backup_{0}.cdb";

    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerOptions.Default)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Inject]
    public required EntryDatabase Database { get; set; }

    [Inject]
    public required IJSRuntime Js { get; set; }

    [Inject]
    public required IStringLocalizer<ExportImport> Loc { get; set; }

    private async Task OnExport()
    {
        var entries = await Database.Entries
            .ToList();

        var exportData = JsonSerializer.Serialize(entries, _serializerOptions);

        using var archiveStream = await CreateArchive(exportData);

        archiveStream.Seek(0, SeekOrigin.Begin);

        var filename = string.Format(FileNameTemplateDownload, DateTime.Now.ToString("yyyyMMdd-HHmmss"));

        using var streamReference = new DotNetStreamReference(archiveStream, leaveOpen: false);

        await Js.InvokeVoidAsync("exportData", filename, streamReference);
    }

    private static async Task<MemoryStream> CreateArchive(string exportData)
    {
        var archiveStream = new MemoryStream();
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true);

        var entry = archive.CreateEntry(FileNameJson, CompressionLevel.SmallestSize);
        await using var entryStream = entry.Open();
        await using var entryWriter = new StreamWriter(entryStream, Encoding.UTF8);
        await entryWriter.WriteAsync(exportData);

        return archiveStream;
    }
}
