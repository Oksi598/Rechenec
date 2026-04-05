using Logistics.Domain;
using Logistics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Api.Controllers;

/// <summary>
/// Створення облікових записів персоналу диспетчером: водій або працівник складу.
/// <c>POST /api/staff/users</c> — лише роль Dispatcher; у тілі <c>role</c> = <c>Driver</c> або <c>Warehouse</c>.
/// Самореєстрації для цих ролей немає — тільки через цей endpoint.
/// </summary>
[ApiController]
[Route("api/staff/users")]
[Authorize(Roles = AppRoles.Dispatcher)]
public sealed class StaffUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffUsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<ActionResult<StaffUserCreatedResponse>> CreateStaffUser(
        [FromBody] CreateStaffUserRequest request,
        CancellationToken ct)
    {
        if (request.Role != AppRoles.Driver && request.Role != AppRoles.Warehouse)
            return BadRequest(new { message = "Role must be Driver or Warehouse." });

        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.Phone?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description).ToList() });

        await _userManager.AddToRoleAsync(user, request.Role);

        return Ok(new StaffUserCreatedResponse(user.Id, email, request.Role));
    }

    public sealed record CreateStaffUserRequest(
        string Email,
        string Password,
        string FullName,
        string? Phone,
        string Role);

    public sealed record StaffUserCreatedResponse(Guid UserId, string Email, string Role);
}
