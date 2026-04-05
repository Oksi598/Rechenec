using Logistics.Application.Dtos;
using MediatR;

namespace Logistics.Application.Queries;

public sealed record ListMyCustomerOrdersQuery : IRequest<IReadOnlyList<OrderDetailDto>>;
