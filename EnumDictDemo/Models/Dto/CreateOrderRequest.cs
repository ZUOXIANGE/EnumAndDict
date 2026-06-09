using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EnumDictDemo.Models.Enums;

namespace EnumDictDemo.Models.Dto;

public class CreateOrderRequest
{
    [Required]
    public string OrderNo { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [JsonConverter(typeof(EnumStringConverter<OrderStatus>))]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [JsonConverter(typeof(EnumStringConverter<PaymentMethod>))]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.WeChatPay;

    [Required]
    public string Source { get; set; } = string.Empty;
}

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