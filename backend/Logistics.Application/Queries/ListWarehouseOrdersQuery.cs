using Logistics.Application.Dtos;
using MediatR;

namespace Logistics.Application.Queries;

public sealed record ListWarehouseOrdersQuery : IRequest<IReadOnlyList<OrderDetailDto>>;
