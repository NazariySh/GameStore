namespace Gamestore.BLL.DTOs.Games;

public class DownloadGameImageDto
{
    public string FileName { get; set; }

    public string ContentType { get; set; }

    public byte[] Content { get; set; }
}