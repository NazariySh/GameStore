namespace Gamestore.WebApi.Utilities;

public static class EnumParser
{
    public static T? ParseEnum<T>(this string? value)
        where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return Enum.TryParse<T>(value, true, out var result)
            ? result
            : throw new ArgumentException($"Invalid value '{value}' for enum type '{typeof(T).Name}'.");
    }
}