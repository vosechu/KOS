using kOS.Craft;

namespace kOS.Binding.Flight
{
    [kOSBinding("ksp")]
    public class BindingFlightSettings : Binding
    {
        public override void AddTo(BindingManager manager)
        {
            manager.AddSetter("TARGET", delegate(CPU cpu, object val) 
                {
                    if (val is ITargetable)
                    {
                        VesselUtils.SetTarget((ITargetable)val);
                    }
                    else if (val is VesselTarget)
                    {
                        VesselUtils.SetTarget(((VesselTarget)val).target);
                    }
                    else if (val is BodyTarget)
                    {
                        VesselUtils.SetTarget(((BodyTarget)val).target);
                    }
                    else
                    {
                        var body = VesselUtils.GetBodyByName(val.ToString());
                        if (body != null)
                        {
                            VesselUtils.SetTarget(body);
                            return;
                        }

                        var vessel = cpu.Vessel.GetVesselByName(val.ToString());
                        if (vessel != null)
                        {
                            VesselUtils.SetTarget(vessel);
                            return;
                        }
                    }
                });

            manager.AddGetter("TARGET", delegate(CPU cpu) 
                {
                    var currentTarget = FlightGlobals.fetch.VesselTarget;

                    if (currentTarget is Vessel)
                    {
                        return new VesselTarget((Vessel)currentTarget, cpu);
                    }
                    else if (currentTarget is CelestialBody)
                    {
                        return new BodyTarget((CelestialBody)currentTarget, cpu);
                    }

                    return null;
                });
        }
    }
}