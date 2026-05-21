namespace EXE201_Backend.Models.Requests
{
    public sealed class TimeShiftRequest
    {
        public string? Duration { get; set; }
        public double? Seconds { get; set; }
    }
}
