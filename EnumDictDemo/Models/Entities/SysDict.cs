using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnumDictDemo.Models.Entities;

[Table("SysDict")]
public class SysDict
{
    [Key]
    public long Id { get; set; }

    [MaxLength(32)]
    public string DictCode { get; set; } = string.Empty;

    [MaxLength(32)]
    public string DictValue { get; set; } = string.Empty;

    [MaxLength(64)]
    public string DictLabel { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; } = true;
}