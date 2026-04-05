using Logistics.Application.Dtos;
using MediatR;

namespace Logistics.Application.Queries;

public sealed record GetOrderForCurrentUserQuery(Guid OrderId) : IRequest<OrderDetailDto?>;
