namespace Gamestore.BLL.Interfaces;

public interface IValidationService
{
    Task ValidateAndThrowAsync<T>(T instance, CancellationToken cancellationToken = default)
        where T : class;
}