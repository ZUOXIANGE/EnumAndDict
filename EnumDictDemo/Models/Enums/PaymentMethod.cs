using FastEnumUtility;

namespace EnumDictDemo.Models.Enums;

public enum PaymentMethod
{
    [Label("微信支付")]
    WeChatPay = 1,
    [Label("支付宝")]
    Alipay = 2,
    [Label("银行卡")]
    BankCard = 3,
    [Label("货到付款")]
    CashOnDelivery = 4
}