using System.Collections;
using System.Reflection;
using EnumDictDemo.Attributes;
using Microsoft.Extensions.Options;

namespace EnumDictDemo.Infrastructure;

public class ObjectVisitor
{
    private readonly DictTranslateOptions _options;
    private readonly ILogger<ObjectVisitor> _logger;

    private static readonly HashSet<Type> SimpleTypes =
    [
        typeof(string), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
        typeof(Guid), typeof(decimal), typeof(bool),
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
        typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(char)
    ];

    public ObjectVisitor(IOptions<DictTranslateOptions> options, ILogger<ObjectVisitor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public List<TranslationRequest> Visit(object? root)
    {
        var requests = new List<TranslationRequest>();
        if (root == null) return requests;

        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        Walk(root, requests, visited, 0);
        return requests;
    }

    private void Walk(object? obj, List<TranslationRequest> requests, HashSet<object> visited, int depth)
    {
        if (obj == null) return;

        if (depth > _options.MaxRecursionDepth)
        {
            _logger.LogError("Max recursion depth {Depth} exceeded at type {Type}", _options.MaxRecursionDepth, obj.GetType().Name);
            return;
        }

        var type = obj.GetType();

        if (IsSimpleType(type))
            return;

        if (_options.EnableCycleDetection)
        {
            if (!visited.Add(obj))
            {
                _logger.LogWarning("Cycle detected at type {Type}, skipping", type.Name);
                return;
            }
        }

        if (obj is IDictionary dict)
        {
            foreach (var value in dict.Values)
                Walk(value, requests, visited, depth + 1);
            return;
        }

        if (obj is IEnumerable enumerable and not string)
        {
            foreach (var item in enumerable)
                Walk(item, requests, visited, depth + 1);
            return;
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (!prop.CanRead) continue;

            var translateAttrs = prop.GetCustomAttributes<DictTranslateAttribute>();
            foreach (var attr in translateAttrs)
            {
                if (string.IsNullOrEmpty(attr.DictCode) || string.IsNullOrEmpty(attr.TargetProperty))
                {
                    _logger.LogWarning("Invalid DictTranslate on {Type}.{Property}", type.Name, prop.Name);
                    continue;
                }

                var sourceValue = prop.GetValue(obj)?.ToString();

                var targetProp = type.GetProperty(attr.TargetProperty, BindingFlags.Public | BindingFlags.Instance);
                if (targetProp == null || !targetProp.CanWrite)
                {
                    _logger.LogWarning("Target property {Target} not found or readonly on {Type}", attr.TargetProperty, type.Name);
                    continue;
                }

                requests.Add(new TranslationRequest
                {
                    TargetObject = obj,
                    SourceValue = sourceValue,
                    DictCode = attr.DictCode,
                    TargetPropertyName = attr.TargetProperty,
                    DefaultValue = attr.DefaultValue,
                    TargetProperty = targetProp
                });
            }

            var propValue = prop.GetValue(obj);
            if (propValue != null && !IsSimpleType(prop.PropertyType) && prop.PropertyType != type)
            {
                Walk(propValue, requests, visited, depth + 1);
            }
        }
    }

    private static bool IsSimpleType(Type type) =>
        type.IsPrimitive || type.IsEnum || SimpleTypes.Contains(type) ||
        Nullable.GetUnderlyingType(type) != null && IsSimpleType(Nullable.GetUnderlyingType(type)!);
}