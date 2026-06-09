namespace EnumDictDemo.Services;

public interface IDictService
{
    Task<string?> GetDictLabelAsync(string dictCode, string dictValue, CancellationToken ct = default);
    Task<Dictionary<string, string>> GetDictMappingAsync(string dictCode, CancellationToken ct = default);
    Task<bool> ExistsAsync(string dictCode, string dictValue, CancellationToken ct = default);
    Task RefreshCacheAsync(CancellationToken ct = default);
}