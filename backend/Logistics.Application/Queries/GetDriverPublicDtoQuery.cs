using Logistics.Application.Dtos;
using Logistics.Application.Ports;
using MediatR;

namespace Logistics.Application.Queries;

public sealed record GetDriverPublicDtoQuery(Guid DriverId, MaskingScope Scope) : IRequest<DriverPublicDto>;

