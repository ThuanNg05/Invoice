namespace Invoice.Core.Contracts.Services;

/// <summary>
/// Wrapper chứa data + metadata về thời gian cache.
/// TTL mặc định 5 phút — đủ để tránh stale data trong session làm việc bình thường.
/// </summary>
internal sealed class CacheEntry<T>
{
    public T Data
    {
        get;
    }
    public DateTime CachedAt
    {
        get;
    }
    public TimeSpan TimeToLive
    {
        get;
    }

    public bool IsExpired => DateTime.UtcNow - CachedAt > TimeToLive;

    public CacheEntry(T data, TimeSpan? ttl = null)
    {
        Data = data;
        CachedAt = DateTime.UtcNow;
        TimeToLive = ttl ?? TimeSpan.FromMinutes(5);
    }
}
