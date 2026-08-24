using Gamestore.Domain.Entities.Payments;

namespace Gamestore.BLL.Tests.TestData.Payments;

public static class PaymentMethodTestData
{
    public const string Bank = "Bank";
    public const string IBox = "IBox terminal";
    public const string Visa = "Visa";

    public static IReadOnlyList<PaymentMethod> GetPaymentMethods()
    {
        return
        [
            new PaymentMethod
            {
                Id = Guid.Parse("5f8d04c7-4d7a-4f98-a0f0-abbc39f67f21"),
                Title = "Bank",
                Description = "Pay using your bank account.",
                ImageUrl = "https://static.vecteezy.com/system/resources/previews/000/593/729/non_2x/vector-bank-building-icon-isolated-on-white-background.jpg",
            },
            new PaymentMethod
            {
                Id = Guid.Parse("9e6c42d7-3e3a-4c89-9d20-0c7a47c69b81"),
                Title = "IBox terminal",
                Description = "Pay using IBox terminal.",
                ImageUrl = "https://images.seeklogo.com/logo-png/39/1/ibox-logo-png_seeklogo-397796.png",
            },
            new PaymentMethod
            {
                Id = Guid.Parse("a3bf6719-7f55-4dbb-84d6-cb2f64bda8a1"),
                Title = "Visa",
                Description = "Pay using your Visa card.",
                ImageUrl = "https://images.seeklogo.com/logo-png/14/1/visa-logo-png_seeklogo-149698.png",
            },
        ];
    }

    public static PaymentMethod GetPaymentMethod()
    {
        return GetPaymentMethods()[0];
    }
}