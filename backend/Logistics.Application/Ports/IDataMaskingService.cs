namespace Logistics.Application.Ports;

public interface IDataMaskingService
{
    string MaskEmail(string email, MaskingScope scope);
    string MaskPhone(string phone, MaskingScope scope);
}

