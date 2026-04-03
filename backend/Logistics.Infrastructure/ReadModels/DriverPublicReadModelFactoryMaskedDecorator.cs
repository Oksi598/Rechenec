using Logistics.Application.Dtos;
using Logistics.Application.Ports;

namespace Logistics.Infrastructure.ReadModels;

public sealed class DriverPublicReadModelFactoryMaskedDecorator : IDriverPublicReadModelFactory
{
    private readonly IDriverPublicReadModelFactoryRaw _raw;
    private readonly IDataMaskingService _masking;

    public DriverPublicReadModelFactoryMaskedDecorator(
        IDriverPublicReadModelFactoryRaw raw,
        IDataMaskingService masking)
    {
        _raw = raw;
        _masking = masking;
    }

    public async Task<DriverPublicDto> CreateAsync(Guid driverId, MaskingScope scope, CancellationToken ct)
    {
        var raw = await _raw.CreateAsync(driverId, ct);

        var maskedEmail = _masking.MaskEmail(raw.Contact.MaskedEmail, scope);
        var maskedPhone = _masking.MaskPhone(raw.Contact.MaskedPhone, scope);

        return new DriverPublicDto(
            raw.Id,
            raw.FullName,
            new MaskedContactDto(maskedEmail, maskedPhone));
    }
}

