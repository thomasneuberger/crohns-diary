using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrohnsDiary.App.Database;
using CrohnsDiary.App.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

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
    public required ISnackbar Snackbar { get; set; }

    [Inject]
    public required EntryDatabase Database { get; set; }

    [Inject]
    public required IJSRuntime Js { get; set; }

    [Inject]
    public required IStringLocalizer<ExportImport> Loc { get; set; }

    private IBrowserFile? FileToImport { get; set; }

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

    private async Task OnImport()
    {
        if (FileToImport is null)
        {
            Snackbar.Add(Loc["SelectFileFirst", Severity.Warning]);
            return;
        }

        var json = await ReadImportDataFromArchive(FileToImport);

        if (json is null)
        {
            return;
        }

        Entry[]? entries;
        try
        {
            entries = JsonSerializer.Deserialize<Entry[]>(json, _serializerOptions);
        }
        catch (JsonException)
        {
            Snackbar.Add(Loc["ParsingError"], Severity.Error);
            return;
        }

        if (entries != null)
        {
            await ImportEntries(entries);
        }
    }

    private async Task ImportEntries(Entry[] entries)
    {
        var ids = entries.Select(e => e.Id).ToArray();
        await Database.Entries.BulkPut(entries);

        Snackbar.Add(Loc["Imported"], Severity.Success);
    }

    private async Task<string?> ReadImportDataFromArchive(IBrowserFile file)
    {
        await using var archiveStream = new MemoryStream();
        await using (var uploadedStream = file.OpenReadStream())
        {
            await uploadedStream.CopyToAsync(archiveStream);
        }
        archiveStream.Position = 0;

        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: false);
        var entry = archive.GetEntry(FileNameJson);
        if (entry is null)
        {
            Snackbar.Add(Loc["NoDataFound"], Severity.Error);
            return null;
        }

        await using var entryStream = entry.Open();
        using var reader = new StreamReader(entryStream);
        var json = await reader.ReadToEndAsync();
        return json;
    }

    private void ImportFileSelected(InputFileChangeEventArgs e)
    {
        FileToImport = e.File;
    }
}
