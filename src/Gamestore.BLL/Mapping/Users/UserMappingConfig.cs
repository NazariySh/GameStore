using Gamestore.BLL.DTOs.Users;
using Gamestore.Domain.Entities.Users;
using Mapster;

namespace Gamestore.BLL.Mapping.Users;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, User>()
            .Ignore(dest => dest.PasswordHash);

        config.NewConfig<UserRole, UserRole>()
            .Ignore(dest => dest.User)
            .Ignore(dest => dest.Role);

        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Name, src => src.UserName);

        config.NewConfig<UserCreateDto, User>()
            .Map(dest => dest.UserName, src => src.Name);

        config.NewConfig<UserUpdateDto, User>()
            .Map(dest => dest.UserName, src => src.Name);
    }
}