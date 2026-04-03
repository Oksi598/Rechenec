using Logistics.Application.Ports;
using MediatR;

namespace Logistics.Application.Queries.Handlers;

public sealed class GetExternalContactDtoQueryHandler
    : IRequestHandler<GetExternalContactDtoQuery, Logistics.Application.Dtos.ExternalContactDto>
{
    private readonly IExternalPublicReadModelFactory _factory;

    public GetExternalContactDtoQueryHandler(IExternalPublicReadModelFactory factory)
    {
        _factory = factory;
    }

    public Task<Logistics.Application.Dtos.ExternalContactDto> Handle(
        GetExternalContactDtoQuery request,
        CancellationToken cancellationToken)
        => _factory.CreateAsync(request.ExternalContactId, request.Scope, cancellationToken);
}

