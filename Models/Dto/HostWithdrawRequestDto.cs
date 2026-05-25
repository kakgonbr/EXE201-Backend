namespace EXE201_Backend.Models.Dto
{
    public class HostWithdrawRequestDto
    {
        public int Id { get; set; }
        public int HostId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = default!;
        public string HostName { get; set; } = default!;
        public string BankName { get; set; } = default!;
        public string BankAccount { get; set; } = default!;
        public string? Note { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
