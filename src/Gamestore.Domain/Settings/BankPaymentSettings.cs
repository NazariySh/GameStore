namespace Gamestore.Domain.Settings;

public record BankPaymentSettings
{
    public const string SectionName = nameof(BankPaymentSettings);

    public int ValidityPeriodInDays { get; init; }
}