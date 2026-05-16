namespace EXE201_Backend.Services
{
    /// <summary>
    /// Time provider implementation that allows manipulation of the current time for testing purposes.
    /// </summary>
    public class TimeMachine : ITimeProvider
    {
        public DateTime Now => DateTime.Now.Add(_offset);
        private TimeSpan _offset = TimeSpan.Zero;

        public void Forward(TimeSpan timeSpan)
        {
            _offset += timeSpan;
        }

        public void Backward(TimeSpan timeSpan)
        {
            _offset -= timeSpan;
        }

        public void Reset()
        {
            _offset = TimeSpan.Zero;
        }

        public void SetOffset(TimeSpan timeSpan)
        {
            _offset = timeSpan;
        }
    }
}
