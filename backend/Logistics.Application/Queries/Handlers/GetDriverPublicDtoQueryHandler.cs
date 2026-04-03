using Logistics.Application.Ports;
using MediatR;

namespace Logistics.Application.Queries.Handlers;

public sealed class GetDriverPublicDtoQueryHandler : IRequestHandler<GetDriverPublicDtoQuery, Logistics.Application.Dtos.DriverPublicDto>
{
    private readonly IDriverPublicReadModelFactory _factory;

    public GetDriverPublicDtoQueryHandler(IDriverPublicReadModelFactory factory)
    {
        _factory = factory;
    }

    public Task<Logistics.Application.Dtos.DriverPublicDto> Handle(GetDriverPublicDtoQuery request, CancellationToken cancellationToken)
        => _factory.CreateAsync(request.DriverId, request.Scope, cancellationToken);
}

