namespace Logistics.Application.Dtos;

public sealed record ExternalContactDto(
    Guid Id,
    string FullName,
    MaskedContactDto Contact);

