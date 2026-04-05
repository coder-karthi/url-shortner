namespace URLShortner.Contracts;

public class UrlMappingListItemResponse
{
    public required Guid Id { get; init; }
    public required string LongUrl { get; init; }
    public required string ShortCode { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required long ClickCount { get; init; }
}
