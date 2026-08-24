using FluentValidation.TestHelper;
using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Payments;
using Gamestore.BLL.Validators.Payments;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Payments;

namespace Gamestore.BLL.Tests.Validators.Payments;

public class PaymentRequestValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<PaymentMethod> _paymentMethodRepository;
    private readonly PaymentRequestValidator _validator;

    public PaymentRequestValidatorTests()
    {
        _unitOfWork = UnitOfWorkFactory.Create();
        _paymentMethodRepository = _unitOfWork.Repositories.GetGeneric<PaymentMethod>();
        _validator = new PaymentRequestValidator(_paymentMethodRepository);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task Should_HaveError_When_MethodIsEmpty(string invalidMethod)
    {
        var request = new PaymentRequest
        {
            Method = invalidMethod,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Method)
            .WithErrorMessage("Payment method is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_MethodDoesNotExist()
    {
        var request = new PaymentRequest
        {
            Method = "NonExistentMethod",
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Method)
            .WithErrorMessage($"Payment method '{request.Method}' does not exist.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_MethodIsValid()
    {
        var paymentMethod = await SeedPaymentMethodAsync();

        var request = new PaymentRequest
        {
            Method = paymentMethod.Title,
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Method);
    }

    private async Task<PaymentMethod> SeedPaymentMethodAsync()
    {
        var paymentMethod = PaymentMethodTestData.GetPaymentMethod();
        await _paymentMethodRepository.AddAsync(paymentMethod);
        await _unitOfWork.SaveChangesAsync();
        return paymentMethod;
    }
}