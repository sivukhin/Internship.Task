namespace StatCore.Stats
{
    public class AverageValue
    {
        public int Count { get; private set; }
        public double Sum { get; private set; }

        public double Value
        {
            get
            {
                if (Count == 0)
                    return 0;
                return Sum / Count;
            }
        }

        public static AverageValue operator +(AverageValue average, double value)
        {
            return new AverageValue { Count = average.Count + 1, Sum = average.Sum + value };
        }

        public static AverageValue operator -(AverageValue average, double value)
        {
            if (average.Count == 0)
                return average;
            return new AverageValue { Count = average.Count - 1, Sum = average.Sum - value };
        }
    }
}