using Data.Entities;

namespace Business.Interfaces;
public interface IAddressService
{
    Task<AddressEntity?> GetOrCreateAddressAsync(string? street, string? postalCode, string? city);
}