namespace kOS
{
    public class SpecialValueTester : SpecialValue
    {
        readonly CPU cpu;

        public SpecialValueTester(CPU cpu)
        {
            this.cpu = cpu;
        }

        public override string ToString()
        {
            return "3";
        }

        public override object GetSuffix(string suffixName)
        {
            return suffixName == "A" ? cpu.SessionTime : base.GetSuffix(suffixName);
        }
    }
}
