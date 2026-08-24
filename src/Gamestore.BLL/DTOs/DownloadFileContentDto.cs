namespace Gamestore.BLL.DTOs;

public class DownloadFileContentDto
{
    public string FileName { get; set; }

    public string ContentType { get; set; }

    public byte[] Content { get; set; }
}