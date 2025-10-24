using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;
using SoNice.Application.Interfaces;
using SoNice.Domain.Enums;

namespace SoNice.Api.Controllers;

/// <summary>
/// PayOS controller - matches Node.js PayOsController exactly
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PayOsController : ControllerBase
{
    private readonly IPayOsService _payOsService;
    private readonly ILogger<PayOsController> _logger;

    public PayOsController(IPayOsService payOsService, ILogger<PayOsController> logger)
    {
        _payOsService = payOsService;
        _logger = logger;
    }

    /// <summary>
    /// Health check for PayOS webhook verification - matches Node.js GET /callback exactly
    /// </summary>
    [HttpGet("callback")]
    public IActionResult HealthCheck()
    {
        return Ok("OK");
    }

    /// <summary>
    /// Health check (HEAD) for PayOS webhook verification - matches Node.js HEAD /callback exactly
    /// </summary>
    [HttpHead("callback")]
    public IActionResult HealthCheckHead()
    {
        return Ok();
    }

    /// <summary>
    /// PayOS webhook callback - matches Node.js payOsCallBack exactly
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> PayOsCallback([FromBody] PayOsCallbackDto dto)
    {
        try
        {
            // If missing payload (e.g., PayOS verify ping), respond 200 to avoid webhook verification failure
            if (string.IsNullOrEmpty(dto.Data?.OrderCode))
            {
                return Ok("OK");
            }

            var result = await _payOsService.HandleWebhookCallbackAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PayOsCallback");
            // Always acknowledge to prevent PayOS from flagging webhook as down
            return Ok("OK");
        }
    }
}
