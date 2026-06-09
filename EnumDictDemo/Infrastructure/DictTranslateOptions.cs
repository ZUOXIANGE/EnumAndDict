namespace EnumDictDemo.Infrastructure;

public class DictTranslateOptions
{
    public const string SectionName = "DictTranslate";

    public int MaxRecursionDepth { get; set; } = 10;
    public bool EnableBatchTranslation { get; set; } = true;
    public int CacheDurationMinutes { get; set; } = 60;
    public bool ThrowOnMissingTargetProperty { get; set; } = false;
    public bool EnableCycleDetection { get; set; } = true;
}