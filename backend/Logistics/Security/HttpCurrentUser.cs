using System.Security.Claims;
using Logistics.Application.Ports;

namespace Logistics.Api.Security;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var id = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var g) ? g : null;
        }
    }

    public bool IsInRole(string role)
        => _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
}
