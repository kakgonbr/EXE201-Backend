namespace EXE201_Backend.Models.Requests
{
    public class HostWithdrawUpdateRequest
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
    }
}
