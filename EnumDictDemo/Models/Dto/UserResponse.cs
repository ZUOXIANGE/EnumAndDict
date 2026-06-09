using EnumDictDemo.Attributes;

namespace EnumDictDemo.Models.Dto;

public class UserResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [DictTranslate("sex", nameof(SexDesc))]
    public string Sex { get; set; } = string.Empty;

    public string SexDesc { get; set; } = string.Empty;

    [DictTranslate("nation", nameof(NationDesc))]
    public string Nation { get; set; } = string.Empty;

    public string NationDesc { get; set; } = string.Empty;

    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
}