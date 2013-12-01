using kOS.Values;

namespace kOS.Binding.Test
{
    [kOSBinding("ksp", "testTerm")]
    public class BindingsTest : Binding
    {
        public override void AddTo(BindingManager manager)
        {
            manager.AddGetter("TEST:RADAR", cpu => new TimeSpan(cpu.SessionTime)); 
        }
    }
}

