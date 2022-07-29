using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FadingFlame.Lists;

public interface IListValidationService
{
    Task<List<string>> Validate(string list);
}

public class ListValidationService : IListValidationService
{
    private readonly HttpClient _httpClient;

    public ListValidationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<List<string>> Validate(string list)
    {
        try
        {
            await _httpClient.GetAsync("");
            return new List<string>();
        }
        catch (Exception)
        {
            return new List<string> {"Connection error, could not validate list with newrecruit, please try again later or contact the orga group"};
        }
    }
}