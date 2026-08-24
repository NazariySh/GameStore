using Gamestore.Domain.Shared;
using Microsoft.Extensions.Configuration;

namespace Gamestore.BLL.Extensions;

public static class ConfigurationExtensions
{
    public static T GetSection<T>(this IConfiguration configuration, string sectionName)
        where T : class, new()
    {
        var section = configuration.GetSection(sectionName).Get<T>();
        Guard.AgainstNull(section);
        return section;
    }
}