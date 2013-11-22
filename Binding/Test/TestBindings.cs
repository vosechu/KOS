namespace kOS
{
    [kOSBinding]
    public class TestBindings : Binding
    {
        public override void AddTo(BindingManager manager)
        {
            //manager.AddGetter("TEST1", delegate(CPU cpu) { return 4; });
            //manager.AddSetter("TEST1", delegate(CPU cpu, object val) { cpu.PrintLine(val.ToString()); });
        }
    }
}