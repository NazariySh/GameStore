using Gamestore.BLL.Interfaces;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Services.Interfaces;
using MapsterMapper;

namespace Gamestore.BLL.Services;

public class ServiceContext : IServiceContext
{
    public ServiceContext(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidationService validationService,
        IUserContext userContext)
    {
        UnitOfWork = unitOfWork;
        Mapper = mapper;
        Validation = validationService;
        UserContext = userContext;
    }

    public IUnitOfWork UnitOfWork { get; }

    public IMapper Mapper { get; }

    public IValidationService Validation { get; }

    public IUserContext UserContext { get; }
}