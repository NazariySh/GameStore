using Gamestore.BLL.Mapping.Games;
using Mapster;
using MapsterMapper;

namespace Gamestore.BLL.Tests.Factories;

public static class MapperFactory
{
    private static readonly TypeAdapterConfig DefaultConfig = CreateConfig();

    public static IMapper Create()
    {
        var config = DefaultConfig.Clone();
        return new Mapper(config);
    }

    private static TypeAdapterConfig CreateConfig()
    {
        var globalConfig = TypeAdapterConfig.GlobalSettings;
        var registers = globalConfig.Scan(typeof(GameMappingConfig).Assembly);

        var config = new TypeAdapterConfig();
        foreach (var register in registers)
        {
            register.Register(config);
        }

        return config;
    }
}
