using Gamestore.Domain.Enums;

namespace Gamestore.BLL.DTOs.Logging;

public class ChangeLogDto<T>
    where T : class
{
    public ChangeLogDto(LogAction action, T? oldVersion = null, T? newVersion = null)
        : this(action.ToString(), oldVersion, newVersion)
    {
    }

    public ChangeLogDto(string action, T? oldVersion = null, T? newVersion = null)
    {
        Action = action;
        OldVersion = oldVersion;
        NewVersion = newVersion;
    }

    public string Action { get; set; }

    public T? OldVersion { get; set; }

    public T? NewVersion { get; set; }
}