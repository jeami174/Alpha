using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;

namespace Business.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<AddressEntity?> GetOrCreateAddressAsync(string? street, string? postalCode, string? city)
    {
        street = string.IsNullOrWhiteSpace(street) ? string.Empty : street.Trim();
        postalCode = string.IsNullOrWhiteSpace(postalCode) ? string.Empty : postalCode.Trim();
        city = string.IsNullOrWhiteSpace(city) ? string.Empty : city.Trim();

        if (string.IsNullOrEmpty(street) && string.IsNullOrEmpty(postalCode) && string.IsNullOrEmpty(city))
            return null;

        var existing = await _addressRepository.GetOneAsync(a =>
            (a.Street ?? "").ToLower() == street.ToLower() &&
            (a.PostalCode ?? "").ToLower() == postalCode.ToLower() &&
            (a.City ?? "").ToLower() == city.ToLower()
        );

        if (existing != null)
            return existing;

        var newAddress = new AddressEntity
        {
            Street = street,
            PostalCode = postalCode,
            City = city
        };

        await _addressRepository.CreateAsync(newAddress);
        await _addressRepository.SaveToDatabaseAsync();

        return newAddress;
    }

}