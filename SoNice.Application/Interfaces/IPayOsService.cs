using SoNice.Application.DTOs;

namespace SoNice.Application.Interfaces;

/// <summary>
/// PayOS service interface - matches Node.js PayOsController logic
/// </summary>
public interface IPayOsService
{
    Task<object> HandleWebhookCallbackAsync(PayOsCallbackDto dto);
}
