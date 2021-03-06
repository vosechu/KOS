using kOS.Context;
using kOS.Suffixed;
using kOS.Utilities;

namespace kOS.Binding
{
    [KOSBinding("ksp")]
    public class MissionSettings : IBinding
    {
        public void BindTo(IBindingManager manager)
        {
            manager.AddSetter("TARGET", delegate(ICPU cpu, object val)
                {
                    var targetable = val as IKOSTargetable;
                    if (targetable != null)
                    {
                        VesselUtils.SetTarget(targetable);
                    }
                    else
                    {
                        var body = VesselUtils.GetBodyByName(val.ToString());
                        if (body != null)
                        {
                            VesselUtils.SetTarget(body);
                            return;
                        }

                        var vessel = VesselUtils.GetVesselByName(val.ToString(), cpu.Vessel);
                        if (vessel != null)
                        {
                            VesselUtils.SetTarget(vessel);
                        }
                    }
                });

            manager.AddGetter("TARGET", delegate(ICPU cpu)
                {
                    var currentTarget = FlightGlobals.fetch.VesselTarget;

                    var vessel = currentTarget as Vessel;
                    if (vessel != null)
                    {
                        return new VesselTarget(vessel, cpu);
                    }
                    var body = currentTarget as CelestialBody;
                    if (body != null)
                    {
                        return new BodyTarget(body, cpu.Vessel);
                    }

                    return null;
                });
        }

        public void Update(float time)
        {
        }
    }
}