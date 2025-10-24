using Microsoft.AspNetCore.Mvc;
using SoNice.Application.DTOs;

namespace SoNice.Api.Controllers;

/// <summary>
/// Base controller with common functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Returns a standardized API response
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">API response DTO</param>
    /// <returns>HTTP result</returns>
    protected IActionResult HandleResponse<T>(ApiResponseDto<T> response)
    {
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Returns a standardized API response without data
    /// </summary>
    /// <param name="response">API response DTO</param>
    /// <returns>HTTP result</returns>
    protected IActionResult HandleResponse(ApiResponseDto response)
    {
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    /// <returns>User ID or null if not found</returns>
    protected string? GetCurrentUserId()
    {
        return User.FindFirst("userId")?.Value;
    }

    /// <summary>
    /// Gets the current user role from JWT claims
    /// </summary>
    /// <returns>User role or null if not found</returns>
    protected string? GetCurrentUserRole()
    {
        return User.FindFirst("role")?.Value;
    }

    /// <summary>
    /// Gets the current user email from JWT claims
    /// </summary>
    /// <returns>User email or null if not found</returns>
    protected string? GetCurrentUserEmail()
    {
        return User.FindFirst("email")?.Value;
    }
}
