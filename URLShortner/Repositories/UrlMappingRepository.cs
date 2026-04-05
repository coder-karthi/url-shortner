using Microsoft.EntityFrameworkCore;
using URLShortner.Data;
using URLShortner.Contracts;

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

    public async Task<PagedResult<UrlMappingListItemResponse>> GetUrlMappings(UrlMappingListQuery query)
    {
        var urlMappingsQuery = context.UrlMappings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = $"%{query.Search.Trim()}%";
            urlMappingsQuery = urlMappingsQuery.Where(x =>
                EF.Functions.ILike(x.LongUrl, searchTerm) ||
                EF.Functions.ILike(x.ShortCode, searchTerm));
        }

        urlMappingsQuery = ApplySorting(urlMappingsQuery, query.SortBy, query.SortDirection);

        var totalCount = await urlMappingsQuery.CountAsync();
        var items = await urlMappingsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new UrlMappingListItemResponse
            {
                Id = x.Id,
                LongUrl = x.LongUrl,
                ShortCode = x.ShortCode,
                CreatedAt = x.CreatedAt,
                ClickCount = x.ClickCount
            })
            .ToListAsync();

        return new PagedResult<UrlMappingListItemResponse>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
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

    private static IQueryable<UrlMapping> ApplySorting(IQueryable<UrlMapping> query, string? sortBy, string? sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "longurl" => isDescending ? query.OrderByDescending(x => x.LongUrl) : query.OrderBy(x => x.LongUrl),
            "shortcode" => isDescending ? query.OrderByDescending(x => x.ShortCode) : query.OrderBy(x => x.ShortCode),
            "clickcount" => isDescending ? query.OrderByDescending(x => x.ClickCount) : query.OrderBy(x => x.ClickCount),
            "createdat" or _ => isDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };
    }
}
