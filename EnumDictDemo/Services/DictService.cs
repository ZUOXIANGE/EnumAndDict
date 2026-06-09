using EnumDictDemo.Data;
using EnumDictDemo.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EnumDictDemo.Services;

public class DictService : IDictService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DictService> _logger;
    private readonly DictTranslateOptions _options;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    private const string AllDictsCacheKey = "__ALL_DICT_MAPPINGS__";

    public DictService(AppDbContext context, IMemoryCache cache, IOptions<DictTranslateOptions> options, ILogger<DictService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string?> GetDictLabelAsync(string dictCode, string dictValue, CancellationToken ct = default)
    {
        var allMappings = await GetAllMappingsAsync(ct);
        if (allMappings.TryGetValue(dictCode, out var codeMapping) && codeMapping.TryGetValue(dictValue, out var label))
            return label;

        return null;
    }

    public async Task<Dictionary<string, string>> GetDictMappingAsync(string dictCode, CancellationToken ct = default)
    {
        var allMappings = await GetAllMappingsAsync(ct);
        return allMappings.TryGetValue(dictCode, out var mapping) ? mapping : [];
    }

    public async Task<bool> ExistsAsync(string dictCode, string dictValue, CancellationToken ct = default)
    {
        var label = await GetDictLabelAsync(dictCode, dictValue, ct);
        return label != null;
    }

    public Task RefreshCacheAsync(CancellationToken ct = default)
    {
        _cache.Remove(AllDictsCacheKey);
        _logger.LogInformation("Dict cache cleared");
        return Task.CompletedTask;
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> GetAllMappingsAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue<Dictionary<string, Dictionary<string, string>>>(AllDictsCacheKey, out var cached))
            return cached!;

        await _semaphore.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue<Dictionary<string, Dictionary<string, string>>>(AllDictsCacheKey, out var doubleCheck))
                return doubleCheck!;

            _logger.LogInformation("Loading dict data from database");

            var dicts = await _context.SysDicts
                .AsNoTracking()
                .Where(d => d.IsEnabled)
                .OrderBy(d => d.SortOrder)
                .ToListAsync(ct);

            var mappings = dicts
                .GroupBy(d => d.DictCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(d => d.DictValue, d => d.DictLabel)
                );

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheDurationMinutes)
            };

            _cache.Set(AllDictsCacheKey, mappings, cacheOptions);
            _logger.LogInformation("Loaded {Count} dict types into cache", mappings.Count);

            return mappings;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}