using kOS.Context;
using kOS.Values;

namespace kOS.Binding.Environment
{
    [kOSBinding("ksp")]
    public class BindingTimeWarp : Binding
    {
        public override void AddTo(BindingManager manager)
        {
            manager.AddGetter("WARP", cpu => TimeWarp.fetch.current_rate_index);
            manager.AddSetter("WARP", delegate(CPU cpu, object val)
            {
                int newRate;
                if (int.TryParse(val.ToString(), out newRate))
                {
                    TimeWarp.SetRate(newRate, false);
                }
            });

            foreach (var body in FlightGlobals.fetch.bodies)
            {
                var celestialBody = body;
                manager.AddGetter(body.name, cpu => new BodyTarget(celestialBody, cpu));
            }
        }
    }
}
