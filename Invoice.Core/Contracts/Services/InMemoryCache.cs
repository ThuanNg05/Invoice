namespace Invoice.Core.Contracts.Services;

/// <summary>
/// Thread-safe in-memory cache dùng cho session hiện tại.
/// Không persist qua app restart — phù hợp với local desktop app.
/// </summary>
public sealed class InMemoryCache
{
    public const string CUSTOMERS = "customers";
    public const string MATERIALS = "materials";
    public const string FRAMES = "frames";
    public const string PLANKS = "planks";
    public const string PRICES = "prices";
    public const string PRODUCTS = "products";
    public const string INVOICES = "invoices";
    public const string TRANSACTIONS = "transactions";

    private readonly Dictionary<string, object> _store = new();
    private readonly object _lock = new();

    /// <summary>
    /// Lưu data vào cache với TTL tuỳ chỉnh.
    /// </summary>
    public void Set<T>(string key, T data, TimeSpan? ttl = null)
    {
        lock (_lock)
        {
            _store[key] = new CacheEntry<T>(data, ttl);
        }
    }

    /// <summary>
    /// Lấy data từ cache. Trả về (true, data) nếu hit và chưa expired.
    /// </summary>
    public bool TryGet<T>(string key, out T value)
    {
        lock (_lock)
        {
            if (_store.TryGetValue(key, out var raw) && raw is CacheEntry<T> entry && !entry.IsExpired)
            {
                value = entry.Data;
                return true;  // cache HIT
            }

            value = default;
            return false;     // cache MISS hoặc EXPIRED
        }
    }

    /// <summary>
    /// Xoá một key cụ thể — gọi sau khi mutate data (Add/Update/Delete).
    /// </summary>
    public void Invalidate(string key)
    {
        lock (_lock)
        {
            _store.Remove(key);
        }
    }

    /// <summary>
    /// Kiểm tra key có tồn tại và còn hạn không.
    /// </summary>
    public bool IsValid(string key)
    {
        lock (_lock)
        {
            return _store.TryGetValue(key, out var raw)
                && raw is CacheEntry<object> entry
                && !entry.IsExpired;
        }
    }

    /// <summary>
    /// Reset toàn bộ cache — dùng khi logout hoặc switch user.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _store.Clear();
        }
    }
}
