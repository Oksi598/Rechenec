using MediatR;

namespace Logistics.Application.Commands;

public sealed record OptimizeLoadCommand(Guid RouteId) : IRequest<Unit>;

