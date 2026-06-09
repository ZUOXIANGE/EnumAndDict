namespace EnumDictDemo.Attributes;

/// <summary>
/// 字典翻译特性
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class DictTranslateAttribute : Attribute
{
    /// <summary>
    /// 字典翻译特性
    /// </summary>
    /// <param name="dictCode">字典编码</param>
    /// <param name="targetProperty">目标属性名</param>
    /// <param name="defaultValue">默认值</param>
    public DictTranslateAttribute(string dictCode, string targetProperty, string? defaultValue = null)
    {
        DictCode = dictCode;
        TargetProperty = targetProperty;
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// 字典编码
    /// </summary>
    public string DictCode { get; set; } = string.Empty;

    /// <summary>
    /// 目标属性名
    /// </summary>
    public string TargetProperty { get; set; } = string.Empty;

    /// <summary>
    /// 默认值
    /// </summary>
    public string? DefaultValue { get; set; }
}