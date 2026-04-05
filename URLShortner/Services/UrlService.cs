using URLShortner.Repositories;
using URLShortner.Contracts;

namespace URLShortner.Services;

public class UrlService(Base62Service base62Service, UrlMappingRepository urlMappingRepository)
{
    public async Task<string> CreateShortUrl(string longUrl)
    {
        var existingShortCode = await urlMappingRepository.GetByLongUrl(longUrl);

        if (existingShortCode is not null)
        {
            return existingShortCode.ShortCode;
        }

        string shortCode;

        do
        {
            shortCode = base62Service.GenerateShortCode();
        }
        while (await urlMappingRepository.IsShortCodePresent(shortCode));

        var url = new UrlMapping
        {
            LongUrl = longUrl,
            ShortCode = shortCode
        };

        await urlMappingRepository.Add(url);
        return shortCode;
    }

    public async Task<UrlMapping?> GetByShortCode(string shortCode) => await urlMappingRepository.GetByShortCode(shortCode);

    public async Task<PagedResult<UrlMappingListItemResponse>> GetUrlMappings(UrlMappingListQuery query) =>
        await urlMappingRepository.GetUrlMappings(query);

    public async Task IncrementClick(string shortCode)
    {
        await urlMappingRepository.IncrementClick(shortCode);
    }
}
