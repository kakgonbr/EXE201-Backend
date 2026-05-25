namespace EXE201_Backend.Models.Requests
{
    public class HostWithdrawCreateRequest
    {
        public decimal Amount { get; set; }
        public string BankAccount { get; set; } = default!;
        public string BankName { get; set; } = default!;
    }
}
