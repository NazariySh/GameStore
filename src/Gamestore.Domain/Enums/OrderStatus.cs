namespace Gamestore.Domain.Enums;

public enum OrderStatus
{
    /// <summary>
    /// Products are in the cart.
    /// </summary>
    Open = 0,

    /// <summary>
    /// Payment is started.
    /// </summary>
    Checkout = 1,

    /// <summary>
    /// Payment is performed successfully.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// The order has been shipped.
    /// </summary>
    Shipped = 3,

    /// <summary>
    /// Payment is performed with errors.
    /// </summary>
    Cancelled = 4,
}