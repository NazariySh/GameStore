using FluentValidation;
using Gamestore.BLL.DTOs.Payments;

namespace Gamestore.BLL.Validators.Payments;

public class CardDetailsDtoValidator : AbstractValidator<CardDetailsDto>
{
    public CardDetailsDtoValidator()
    {
        RuleFor(x => x.Holder)
            .NotEmpty().WithMessage("Card holder name is required.")
            .MinimumLength(CreditCardValidationRules.MinHolderLength)
            .WithMessage($"Card holder name must be at least {CreditCardValidationRules.MinHolderLength} characters long.")
            .MaximumLength(CreditCardValidationRules.MaxHolderLength)
            .WithMessage($"Card holder name must not exceed {CreditCardValidationRules.MaxHolderLength} characters.")
            .Matches(CreditCardValidationRules.HolderPattern())
            .WithMessage(CreditCardValidationRules.HolderPatternMessage);

        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .CreditCard().WithMessage("Invalid card number format.");

        RuleFor(x => x.MonthExpire)
            .InclusiveBetween(CreditCardValidationRules.MinMonth, CreditCardValidationRules.MaxMonth)
            .WithMessage($"Expiration month must be between {CreditCardValidationRules.MinMonth} and {CreditCardValidationRules.MaxMonth}.");

        RuleFor(x => x.YearExpire)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
            .WithMessage("Expiration year must be in the current year or later.")
            .LessThanOrEqualTo(DateTime.UtcNow.Year + CreditCardValidationRules.MaxYearExpireOffset)
            .WithMessage($"Expiration year must be within {CreditCardValidationRules.MaxYearExpireOffset} years from now.");

        RuleFor(x => x)
            .Must(BeInTheFuture).WithMessage("The card is expired.");

        RuleFor(x => x.Cvv2)
            .InclusiveBetween(CreditCardValidationRules.MinCvv, CreditCardValidationRules.MaxCvv)
            .WithMessage($"CVV must be between {CreditCardValidationRules.MinCvv} and {CreditCardValidationRules.MaxCvv}.");
    }

    private static bool BeInTheFuture(CardDetailsDto card)
    {
        try
        {
            var expiration = new DateTime(card.YearExpire, card.MonthExpire, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(1)
                .AddDays(-1);
            return expiration >= DateTime.UtcNow.Date;
        }
        catch
        {
            return false;
        }
    }
}