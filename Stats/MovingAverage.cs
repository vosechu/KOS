using System.Globalization;
using System.Linq;

namespace kOS.Stats
{
    /// <summary>
    /// Smoothing Algorithm borrowed From MechJeb2
    /// </summary>
    public class MovingAverage
    {
        private readonly double[] store;
        private readonly int storeSize;
        private int nextIndex;

        public double Value
        {
            get
            {
                var tmp = store.Sum();
                return tmp / storeSize;
            }
            set
            {
                store[nextIndex] = value;
                nextIndex = (nextIndex + 1) % storeSize;
            }
        }

        public MovingAverage(int size = 10, double startingValue = 0)
        {
            storeSize = size;
            store = new double[size];
            Force(startingValue);
        }

        public void Force(double newValue)
        {
            for (var i = 0; i < storeSize; i++)
            {
                store[i] = newValue;
            }
        }

        public static implicit operator double(MovingAverage v)
        {
            return v.Value;
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(string format)
        {
            return Value.ToString(format);
        }
    }
}