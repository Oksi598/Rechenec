using Logistics.Application.Dtos;
using Logistics.Application.Ports;

namespace Logistics.Infrastructure.ReadModels;

public sealed class ExternalPublicReadModelFactoryMaskedDecorator : IExternalPublicReadModelFactory
{
    private readonly IExternalPublicReadModelFactoryRaw _raw;
    private readonly IDataMaskingService _masking;

    public ExternalPublicReadModelFactoryMaskedDecorator(
        IExternalPublicReadModelFactoryRaw raw,
        IDataMaskingService masking)
    {
        _raw = raw;
        _masking = masking;
    }

    public async Task<ExternalContactDto> CreateAsync(Guid externalContactId, MaskingScope scope, CancellationToken ct)
    {
        var raw = await _raw.CreateAsync(externalContactId, ct);

        var maskedEmail = _masking.MaskEmail(raw.Contact.MaskedEmail, scope);
        var maskedPhone = _masking.MaskPhone(raw.Contact.MaskedPhone, scope);

        return new ExternalContactDto(
            raw.Id,
            raw.FullName,
            new MaskedContactDto(maskedEmail, maskedPhone));
    }
}

