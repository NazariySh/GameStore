using FluentValidation;
using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games.Publishers;

public class PublisherUpdateDtoValidator : AbstractValidator<PublisherUpdateDto>
{
    private readonly IRepository<Publisher> _publisherRepository;

    public PublisherUpdateDtoValidator(
        IRepository<Publisher> publisherRepository,
        IValidator<PublisherCreateUpdateDto> baseValidator)
    {
        _publisherRepository = publisherRepository;

        Include(baseValidator);

        RuleFor(x => x)
            .MustAsync(BeUniqueCompanyNameAsync)
            .WithMessage("Publisher with this company name already exists.")
            .OverridePropertyName(x => x.CompanyName);
    }

    private async Task<bool> BeUniqueCompanyNameAsync(PublisherUpdateDto publisher, CancellationToken cancellationToken)
    {
        bool isUniqueCompanyName;

        if (publisher.Id.IsPrimary)
        {
            isUniqueCompanyName = await _publisherRepository.NotExistsAsync(
                p => p.CompanyName == publisher.CompanyName && p.Id != publisher.Id.PrimaryId,
                cancellationToken);
        }
        else
        {
            isUniqueCompanyName = await _publisherRepository.NotExistsAsync(
                p => p.CompanyName == publisher.CompanyName && p.SupplierId != publisher.Id.SecondaryId,
                cancellationToken);
        }

        return isUniqueCompanyName;
    }
}