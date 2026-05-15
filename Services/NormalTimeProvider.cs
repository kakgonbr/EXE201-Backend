namespace EXE201_Backend.Services
{
    public class NormalTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
