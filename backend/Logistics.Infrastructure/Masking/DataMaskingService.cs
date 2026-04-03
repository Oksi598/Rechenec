using Logistics.Application.Ports;

namespace Logistics.Infrastructure.Masking;

public sealed class DataMaskingService : IDataMaskingService
{
    public string MaskEmail(string email, MaskingScope scope)
    {
        if (string.IsNullOrWhiteSpace(email))
            return email;

        if (scope == MaskingScope.Internal)
            return email;

        var at = email.IndexOf('@');
        if (at <= 0)
            return "***";

        var local = email[..at];
        var domain = email[(at + 1)..];

        var keep = Math.Min(2, local.Length);
        var maskedCount = Math.Max(1, local.Length - keep);

        return local[..keep] + new string('*', maskedCount) + "@" + domain;
    }

    public string MaskPhone(string phone, MaskingScope scope)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return phone;

        if (scope == MaskingScope.Internal)
            return phone;

        // Heuristic:
        // - keep the country-prefix digits (everything except the last 3 and the masked middle),
        // - always keep last 3 digits,
        // - mask the remaining middle digits.
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length < 6)
            return "***";

        var countryDigits = digits.Length - 6; // heuristic: keep enough digits for country prefix
        if (countryDigits < 1)
            countryDigits = Math.Max(1, digits.Length - 6);

        var prefix = digits[..countryDigits];
        var suffix = digits[^3..];
        var maskLen = digits.Length - countryDigits - 3;
        maskLen = Math.Max(1, maskLen);

        return "+" + prefix + new string('*', maskLen) + suffix;
    }
}

