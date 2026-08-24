using Gamestore.BLL.DTOs;

namespace Gamestore.BLL.Interfaces;

public interface IFileContentExporter
{
    DownloadFileContentDto ExportToTextFile<T>(T instance, string fileName);
}