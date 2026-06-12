using System.Reflection;
using EnumDictDemo.Services;

namespace EnumDictDemo.Infrastructure;

public class DictTranslationHelper
{
    private readonly IDictService _dictService;
    private readonly ILogger<DictTranslationHelper> _logger;

    public DictTranslationHelper(IDictService dictService, ILogger<DictTranslationHelper> logger)
    {
        _dictService = dictService;
        _logger = logger;
    }

    public async Task TranslateAsync(List<TranslationRequest> requests, CancellationToken ct = default)
    {
        if (requests.Count == 0) return;

        var keysByCode = requests
            .GroupBy(r => r.DictCode)
            .ToDictionary(g => g.Key, g => g.Select(r => r.SourceValue).Distinct().ToList());

        var translations = new Dictionary<string, Dictionary<string, string>>();

        foreach (var (dictCode, sourceValues) in keysByCode)
        {
            var dict = new Dictionary<string, string>();
            foreach (var srcVal in sourceValues)
            {
                if (srcVal != null)
                {
                    var label = await _dictService.GetDictLabelAsync(dictCode, srcVal, ct);
                    dict[srcVal] = label ?? srcVal;
                }
            }
            translations[dictCode] = dict;
        }

        foreach (var request in requests)
        {
            try
            {
                if (!translations.TryGetValue(request.DictCode, out var codeTranslations))
                    continue;

                var translated = request.SourceValue != null && codeTranslations.TryGetValue(request.SourceValue, out var label)
                    ? label
                    : request.DefaultValue ?? request.SourceValue;

                var targetProp = request.TargetProperty
                    ?? request.TargetObject.GetType().GetProperty(request.TargetPropertyName, BindingFlags.Public | BindingFlags.Instance);

                if (targetProp != null && targetProp.CanWrite)
                {
                    var convertedValue = ConvertValue(translated, targetProp.PropertyType);
                    targetProp.SetValue(request.TargetObject, convertedValue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Translation failed for {DictCode}:{SourceValue}", request.DictCode, request.SourceValue);

                var defaultValue = request.DefaultValue ?? request.SourceValue;
                if (defaultValue != null)
                {
                    TrySetDefault(request.TargetObject, request.TargetPropertyName, defaultValue);
                }
            }
        }
    }

    private static object? ConvertValue(string? value, Type targetType)
    {
        if (value == null) return null;
        if (targetType == typeof(string)) return value;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            return Convert.ChangeType(value, underlyingType);
        }
        catch
        {
            return value;
        }
    }

    private static void TrySetDefault(object target, string propertyName, string value)
    {
        try
        {
            var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(string))
            {
                prop.SetValue(target, value);
            }
        }
        catch
        {
        }
    }
}