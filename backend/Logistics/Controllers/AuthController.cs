using System.Security.Claims;
using Logistics.Api.Security;
using Logistics.Domain;
using Logistics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Logistics.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenBuilder _jwt;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenBuilder jwt,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
        _jwtOptions = jwtOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterCustomerRequest request, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            EmailConfirmed = true,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.Phone?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description).ToList() });

        await _userManager.AddToRoleAsync(user, AppRoles.Customer);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwt.CreateAccessToken(user, roles, _jwtOptions);
        return Ok(new AuthResponse(token, user.Id, roles.ToList()));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });

        var check = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!check.Succeeded)
            return Unauthorized(new { message = "Invalid email or password." });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwt.CreateAccessToken(user, roles, _jwtOptions);
        return Ok(new AuthResponse(token, user.Id, roles.ToList()));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken ct)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(id, out var userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new MeResponse(user.Id, user.Email ?? string.Empty, user.FullName, roles.ToList()));
    }

    public sealed record RegisterCustomerRequest(
        string Email,
        string Password,
        string FullName,
        string? Phone);

    public sealed record LoginRequest(string Email, string Password);

    public sealed record AuthResponse(string AccessToken, Guid UserId, IReadOnlyList<string> Roles);

    public sealed record MeResponse(Guid UserId, string Email, string FullName, IReadOnlyList<string> Roles);
}
