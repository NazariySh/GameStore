using System.Text.Json;
using Gamestore.BLL.DTOs;
using Gamestore.Domain.Shared;

namespace Gamestore.BLL.Utilities;

public static class FileContentExporter
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private const string TextContentType = "text/plain";
    private const string TextFileExtension = ".txt";

    public static DownloadFileContentDto ExportToTextFile<T>(T instance, string fileName)
    {
        Guard.AgainstNull(instance);
        Guard.AgainstNullOrWhiteSpace(fileName);

        return new DownloadFileContentDto
        {
            FileName = GetFileName(fileName, TextFileExtension),
            ContentType = TextContentType,
            Content = JsonSerializer.SerializeToUtf8Bytes(instance, JsonOptions),
        };
    }

    private static string GetFileName(string name, string extension)
    {
        return $"{name}{extension}";
    }
}