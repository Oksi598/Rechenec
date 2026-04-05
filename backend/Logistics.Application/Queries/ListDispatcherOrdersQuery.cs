using Logistics.Application.Dtos;
using MediatR;

namespace Logistics.Application.Queries;

public sealed record ListDispatcherOrdersQuery : IRequest<IReadOnlyList<OrderDetailDto>>;
