using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json.Serialization;
using System.Transactions;

namespace EXE201_Backend.Models.Dto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NotificationType
    {
        ORDER_PAID,
        TRANSACTION_VOID
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        CAPTURED,
        CANCELLED,
        AUTHENTICATION_NOT_NEEDED
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CurrencyCode
    {
        VND
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionType
    {
        PAYMENT,
        REFUND
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionStatus
    {
        APPROVED,
        DECLINED
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AuthenticationStatus
    {
        AUTHENTICATION_SUCCESSFUL
    }
    public class PaymentWebhookDto
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("notification_type")]
        public NotificationType NotificationType { get; set; }

        [JsonPropertyName("order")]
        public OrderDto Order { get; set; } = default!;

        [JsonPropertyName("transaction")]
        public TransactionDto Transaction { get; set; } = default!;

        [JsonPropertyName("customer")]
        public object? Customer { get; set; }

        [JsonPropertyName("agreement")]
        public object? Agreement { get; set; }
    }

    public class OrderDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("order_id")]
        public string OrderId { get; set; } = default!;

        [JsonPropertyName("order_status")]
        public OrderStatus OrderStatus { get; set; }

        [JsonPropertyName("order_currency")]
        public CurrencyCode OrderCurrency { get; set; }

        [JsonPropertyName("order_amount")]
        public decimal OrderAmount { get; set; }

        [JsonPropertyName("order_invoice_number")]
        public string OrderInvoiceNumber { get; set; } = default!;

        [JsonPropertyName("custom_data")]
        public List<object> CustomData { get; set; } = [];

        [JsonPropertyName("user_agent")]
        public string UserAgent { get; set; } = default!;

        [JsonPropertyName("ip_address")]
        public string IpAddress { get; set; } = default!;

        [JsonPropertyName("order_description")]
        public string OrderDescription { get; set; } = default!;
    }

    public class TransactionDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; } = default!;

        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; } = default!;

        [JsonPropertyName("transaction_type")]
        public TransactionType TransactionType { get; set; }

        [JsonPropertyName("transaction_date")]
        public string TransactionDate { get; set; } = default!;

        [JsonPropertyName("transaction_status")]
        public TransactionStatus TransactionStatus { get; set; }

        [JsonPropertyName("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonPropertyName("transaction_currency")]
        public CurrencyCode TransactionCurrency { get; set; }

        [JsonPropertyName("authentication_status")]
        public AuthenticationStatus AuthenticationStatus { get; set; }

        [JsonPropertyName("card_number")]
        public string? CardNumber { get; set; }

        [JsonPropertyName("card_holder_name")]
        public string? CardHolderName { get; set; }

        [JsonPropertyName("card_expiry")]
        public string? CardExpiry { get; set; }

        [JsonPropertyName("card_funding_method")]
        public string? CardFundingMethod { get; set; }

        [JsonPropertyName("card_brand")]
        public string? CardBrand { get; set; }
    }
}
