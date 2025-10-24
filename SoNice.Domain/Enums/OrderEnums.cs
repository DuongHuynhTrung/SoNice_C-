namespace SoNice.Domain.Enums;

/// <summary>
/// Order status enumeration - matches Node.js OrderStatusEnum exactly
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipping = 3,
    Delivered = 4,
    Cancelled = 5,
    PaymentFailed = 6
}

/// <summary>
/// Order payment method enumeration - matches Node.js OrderPaymentMethodEnum exactly
/// </summary>
public enum OrderPaymentMethod
{
    Bank = 0,
    Cod = 1
}
