using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using MediatR;

namespace Logistics.Application.Queries;

public sealed record GetExternalContactDtoQuery(Guid ExternalContactId, MaskingScope Scope) : IRequest<ExternalContactDto>;

