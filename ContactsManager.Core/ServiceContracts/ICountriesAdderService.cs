using ServiceContracts.DTO;
using Microsoft.AspNetCore.Http;
namespace ServiceContracts;

/// <summary>
/// Represents business logic (insert) for manipulating Country entity
/// </summary>
public interface ICountriesAdderService
{
    /// <summary>
    /// Return a country object to the list of coutries
    /// </summary>
    /// <param name="countryAddRequest">Country object to add</param>
    /// <returns>Returns the country object after adding it (including newly generated country id)</returns>
    Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);

    
}