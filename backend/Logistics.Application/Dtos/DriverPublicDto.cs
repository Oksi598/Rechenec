namespace Logistics.Application.Dtos;

public sealed record DriverPublicDto(
    Guid Id,
    string FullName,
    MaskedContactDto Contact);

