namespace EXE201_Backend.Models.Requests
{
    public class HostRegistrationUpdateRequest
    {
        public int HostId { get; set; }
        public bool Approved { get; set; }
        public string? Note { get; set; }
    }
}
