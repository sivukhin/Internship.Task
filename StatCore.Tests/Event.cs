namespace StatCore.Tests
{
    public class Event<T>
    {
        public bool IsAddEvent { get; set; }
        public T Value;

        public static Event<T> Add(T value)
        {
            return new Event<T> {IsAddEvent = true, Value = value};
        }

        public static Event<T> Delete(T value)
        {
            return new Event<T> {IsAddEvent = false, Value = value};
        }
    }
}