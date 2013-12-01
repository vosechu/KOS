using kOS.Context;

namespace kOS.Values
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
            if (suffixName == "A")
            {
                return cpu.SessionTime;
            }
            return null;
        }
    }
}
