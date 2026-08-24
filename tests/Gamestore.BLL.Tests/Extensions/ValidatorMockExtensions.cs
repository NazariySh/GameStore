using FluentValidation;
using Moq;

namespace Gamestore.BLL.Tests.Extensions;

public static class ValidatorMockExtensions
{
    public static void VerifyValidateCalled<TValidationType, TInstance>(
        this Mock<IValidator<TValidationType>> mock,
        TInstance expectedInstance,
        Func<Times> times)
    {
        mock.Verify(
            v => v.ValidateAsync(
                It.Is<ValidationContext<TInstance>>(
                    ctx => ctx.InstanceToValidate.Equals(expectedInstance)),
                It.IsAny<CancellationToken>()),
            times);
    }

    public static void VerifyValidateCalledOnce<TValidationType, TInstance>(
        this Mock<IValidator<TValidationType>> mock,
        TInstance expectedInstance)
    {
        mock.VerifyValidateCalled(expectedInstance, Times.Once);
    }
}