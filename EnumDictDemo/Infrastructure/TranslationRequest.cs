using System.Reflection;

namespace EnumDictDemo.Infrastructure;

public class TranslationRequest
{
    public object TargetObject { get; set; } = null!;
    public string? SourceValue { get; set; }
    public string DictCode { get; set; } = string.Empty;
    public string TargetPropertyName { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 缓存的目标属性反射信息（在 ObjectVisitor 中填充），避免在 TranslateAsync 中二次反射查找。
    /// </summary>
    public PropertyInfo? TargetProperty { get; set; }
}