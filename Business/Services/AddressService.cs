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

    public async Task<AddressEntity> GetOrCreateAddressAsync(string street, string postalCode, string city)
    {
        street = street.Trim();
        postalCode = postalCode.Trim();
        city = city.Trim();

        var existing = await _addressRepository.GetOneAsync(a =>
            a.Street.ToLower() == street.ToLower() &&
            a.PostalCode.ToLower() == postalCode.ToLower() &&
            a.City.ToLower() == city.ToLower()
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
