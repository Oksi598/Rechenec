using Microsoft.AspNetCore.Identity;

namespace Logistics.Infrastructure.Data;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
