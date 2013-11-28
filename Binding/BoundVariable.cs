using kOS.Stats;

namespace kOS.Binding
{
    public class BoundVariable : Variable
    {
        public BindingManager.BindingSetDlg Set;
        public BindingManager.BindingGetDlg Get;
        public CPU Cpu;

        public override object Value
        {
            get
            {
                return Get(Cpu);
            }
            set
            {
                Set(Cpu, value);
            }
        }
    }

    public class SmoothVariable : BoundVariable
    {
        private readonly MovingAverage movingAverage;

        public SmoothVariable()
        {
            movingAverage = new MovingAverage();
        }

        public override object Value
        {
            get
            {
                return movingAverage.Value;
            }
        }

	public void Update()
	{
	    double newValue;
	    if (!double.TryParse(Get(Cpu).ToString(), out newValue)) return;
	    movingAverage.Value = newValue;
	}
    }
}