namespace Logistics.Application.Ports;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsInRole(string role);
}
