using Logistics.Application.Commands;
using Logistics.Application.Services;
using MediatR;

namespace Logistics.Application.Commands.Handlers;

public sealed class OptimizeLoadCommandHandler : IRequestHandler<OptimizeLoadCommand, Unit>
{
    private readonly LoadOptimizationService _optimizationService;

    public OptimizeLoadCommandHandler(LoadOptimizationService optimizationService)
    {
        _optimizationService = optimizationService;
    }

    public async Task<Unit> Handle(OptimizeLoadCommand request, CancellationToken cancellationToken)
    {
        await _optimizationService.OptimizeAsync(request.RouteId, cancellationToken);
        return Unit.Value;
    }
}

