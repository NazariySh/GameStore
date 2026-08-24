using FluentValidation;
using Gamestore.BLL.Interfaces;
using Gamestore.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services;

public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(
        IServiceProvider serviceProvider,
        ILogger<ValidationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ValidateAndThrowAsync<T>(T instance, CancellationToken cancellationToken = default)
        where T : class
    {
        Guard.AgainstNull(instance);

        var validator = GetValidator<T>();

        if (validator is null)
        {
            _logger.LogWarning("No validator found for type {TypeName}. Skipping validation", typeof(T).Name);
            return;
        }

        var result = await validator.ValidateAsync(instance, cancellationToken);

        if (!result.IsValid)
        {
            _logger.LogError("Validation failed for type {TypeName}: {Errors}", typeof(T).Name, result.Errors);
            throw new ValidationException(result.Errors);
        }
    }

    private IValidator<T>? GetValidator<T>()
    {
        return _serviceProvider.GetService<IValidator<T>>();
    }
}