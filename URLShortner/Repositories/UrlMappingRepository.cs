using Microsoft.EntityFrameworkCore;
using URLShortner.Data;

namespace URLShortner.Repositories;

public class UrlMappingRepository(AppDbContext context)
{
    public async Task<UrlMapping?> GetByShortCode(string shortCode) => await context.UrlMappings
            .FirstOrDefaultAsync(x => x.ShortCode == shortCode);

    public async Task<UrlMapping?> GetByLongUrl(string longUrl) => await context.UrlMappings.FirstOrDefaultAsync(x => x.LongUrl == longUrl);

    public async Task Add(UrlMapping url)
    {
        context.UrlMappings.Add(url);
        await context.SaveChangesAsync();
    }

    public async Task<bool> IsShortCodePresent(string shortCode) => await context.UrlMappings.AnyAsync(x => x.ShortCode == shortCode);

    public async Task IncrementClick(string shortCode)
    {
        var url = await GetByShortCode(shortCode);
        if (url != null)
        {
            url.ClickCount += 1;
            await context.SaveChangesAsync();
        }

    }
}