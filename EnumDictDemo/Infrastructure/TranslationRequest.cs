namespace EnumDictDemo.Infrastructure;

public class TranslationRequest
{
    public object TargetObject { get; set; } = null!;
    public string? SourceValue { get; set; }
    public string DictCode { get; set; } = string.Empty;
    public string TargetPropertyName { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
}