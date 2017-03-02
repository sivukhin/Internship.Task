namespace StatCore.Stats
{
    public class AverageValue
    {
        private int Count { get; set; }
        private double Sum { get; set; }

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
            return new AverageValue { Count = average.Count - 1, Sum = average.Sum - value };
        }
    }
}