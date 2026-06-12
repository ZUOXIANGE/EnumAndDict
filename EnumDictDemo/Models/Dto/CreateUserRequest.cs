using System.ComponentModel.DataAnnotations;

namespace EnumDictDemo.Models.Dto;

public class CreateUserRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Sex { get; set; } = string.Empty;

    [Required]
    public string Nation { get; set; } = string.Empty;

    [Range(1, 150)]
    public int Age { get; set; }

    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
