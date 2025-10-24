namespace SoNice.Application.DTOs;

/// <summary>
/// PayOS callback DTO
/// </summary>
public class PayOsCallbackDto
{
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public string? Message { get; set; }
    public PayOsCallbackDataDto? Data { get; set; }
}

/// <summary>
/// PayOS callback data DTO
/// </summary>
public class PayOsCallbackDataDto
{
    public string? OrderCode { get; set; }
    public string? Status { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public string? Signature { get; set; }
}

/// <summary>
/// PayOS webhook DTO
/// </summary>
public class PayOsWebhookDto
{
    public string? OrderCode { get; set; }
    public string? Status { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public string? Signature { get; set; }
}