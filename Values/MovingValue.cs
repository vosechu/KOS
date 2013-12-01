using kOS.Binding;
using kOS.Context;
using kOS.Stats;

namespace kOS.Values
{
    public class MovingValue : SpecialValue
    {
        private readonly CPU cpu;
        private readonly BindingManager.BindingGetDlg getter;
        private readonly MovingAverage movingAverage;

        public MovingValue(CPU cpu, BindingManager.BindingGetDlg getter)
        {
            this.cpu = cpu;
            this.getter = getter;
            movingAverage = new MovingAverage();
        }

        public override object GetSuffix(string suffixName)
        {
            return suffixName == "SMOOTH" ? movingAverage.Value : getter(cpu);
        }

        public void Update()
	{
	    movingAverage.Value = (double) getter(cpu);
	}
    }
}
