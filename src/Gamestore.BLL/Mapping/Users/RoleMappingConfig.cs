using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.Domain.Entities.Users;
using Mapster;

namespace Gamestore.BLL.Mapping.Users;

public class RoleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Role, Role>()
            .Ignore(dest => dest.UserRoles);

        config.NewConfig<RoleClaim, RoleClaim>()
            .Ignore(dest => dest.Role);

        config.NewConfig<Role, RoleDto>();

        config.NewConfig<RoleCreateDto, Role>();
        config.NewConfig<RoleUpdateDto, Role>();
    }
}