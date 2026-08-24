using FluentValidation;
using Gamestore.BLL.DTOs.Payments;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Payments;

namespace Gamestore.BLL.Validators.Payments;

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    private readonly IRepository<PaymentMethod> _paymentMethodRepository;

    public PaymentRequestValidator(IRepository<PaymentMethod> paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;

        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Payment method is required.")
            .MustAsync(BeValidPaymentMethodAsync)
            .WithMessage(x => $"Payment method '{x.Method}' does not exist.");
    }

    private Task<bool> BeValidPaymentMethodAsync(string method, CancellationToken cancellationToken)
    {
        return _paymentMethodRepository.ExistsAsync(
            pm => pm.Title == method,
            cancellationToken);
    }
}