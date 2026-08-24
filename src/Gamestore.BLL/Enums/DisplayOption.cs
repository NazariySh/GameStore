using Ardalis.SmartEnum;

namespace Gamestore.BLL.Enums;

public abstract class DisplayOption<TEnum> : SmartEnum<TEnum>
    where TEnum : DisplayOption<TEnum>
{
    protected DisplayOption(string name, int value, string displayName)
        : base(name, value)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }
}