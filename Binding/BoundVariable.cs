namespace kOS
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
}