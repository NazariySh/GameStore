using FluentValidation;
using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games.Publishers;

public class PublisherCreateDtoValidator : AbstractValidator<PublisherCreateDto>
{
    private readonly IRepository<Publisher> _publisherRepository;

    public PublisherCreateDtoValidator(
        IRepository<Publisher> publisherRepository,
        IValidator<PublisherCreateUpdateDto> baseValidator)
    {
        _publisherRepository = publisherRepository;

        Include(baseValidator);

        RuleFor(x => x.CompanyName)
            .MustAsync(BeUniqueCompanyNameAsync)
            .WithMessage("Publisher with this company name already exists.");
    }

    private Task<bool> BeUniqueCompanyNameAsync(string companyName, CancellationToken cancellationToken)
    {
        return _publisherRepository.NotExistsAsync(
            p => p.CompanyName == companyName,
            cancellationToken);
    }
}