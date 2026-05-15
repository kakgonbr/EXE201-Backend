
namespace EXE201_Backend.Services
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
    }
}