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