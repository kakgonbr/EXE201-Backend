using System.Text.Json.Serialization;

namespace EXE201_Backend.Models.Dto
{
    public class PaymentInfoDto
    {
        [JsonPropertyName("order_amount")]
        public string OrderAmount { get; set; } = default!;
        public string Merchant { get; set; } = default!;
        public string Currency { get; set; } = default!;
        public string Operation { get; set; } = default!;
        [JsonPropertyName("order_description")]
        public string OrderDescription { get; set; } = default!;
        [JsonPropertyName("order_invoice_number")]
        public string OrderInvoiceNumber { get; set; } = default!;
        [JsonPropertyName("success_url")]
        public string SuccessUrl { get; set; } = default!;
        [JsonPropertyName("error_url")]
        public string ErrorUrl { get; set; } = default!;
        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; } = default!;
        public string Signature { get; set; } = default!;
    }
}
