using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Services.Interfaces;
using MapsterMapper;

namespace Gamestore.BLL.Interfaces;

public interface IServiceContext
{
    IUnitOfWork UnitOfWork { get; }

    IMapper Mapper { get; }

    IValidationService Validation { get; }

    IUserContext UserContext { get; }
}