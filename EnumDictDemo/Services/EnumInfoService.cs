using System.Collections.Concurrent;
using EnumDictDemo.Models.Dto;
using EnumDictDemo.Models.Enums;
using FastEnumCore = FastEnumUtility.FastEnum;

namespace EnumDictDemo.Services;

public class EnumInfoService : IEnumInfoService
{
    private readonly Dictionary<string, List<EnumOptionResponse>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public EnumInfoService()
    {
        _cache[nameof(OrderStatus)] = BuildOptions<OrderStatus>(e => e.GetDisplayName());
        _cache[nameof(PaymentMethod)] = BuildOptions<PaymentMethod>(e => e.GetDisplayName());
    }

    public List<EnumOptionResponse> GetEnumOptions(string enumName)
    {
        return _cache.TryGetValue(enumName, out var cached) ? cached : [];
    }

    public Dictionary<string, List<EnumOptionResponse>> GetAllEnums()
    {
        return _cache;
    }

    private static List<EnumOptionResponse> BuildOptions<TEnum>(Func<TEnum, string> getDisplayName)
        where TEnum : struct, Enum
    {
        return FastEnumCore.GetValues<TEnum>()
            .Select(e => new EnumOptionResponse
            {
                Value = Convert.ToInt32(e),
                Name = e.ToString(),
                Label = getDisplayName(e)
            })
            .ToList();
    }
}