using FluentValidation;
using Gamestore.BLL.Interfaces;
using Moq;

namespace Gamestore.BLL.Tests.Extensions;

public static class ValidationServiceMockExtensions
{
    public static void SetupValidationThrows<T>(
        this Mock<IValidationService> mock,
        T instance)
        where T : class
    {
        mock.Setup(x => x.ValidateAndThrowAsync(
                instance,
                It.IsAny<CancellationToken>()))
            .Throws(new ValidationException("Validation failed."));
    }
}