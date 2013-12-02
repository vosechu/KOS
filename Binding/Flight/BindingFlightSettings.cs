using kOS.Context;
using kOS.Craft;
using kOS.Values;

namespace kOS.Binding.Flight
{
    [kOSBinding("ksp")]
    public class BindingFlightSettings : Binding
    {
        public override void AddTo(BindingManager manager)
        {
            manager.AddSetter("TARGET", delegate(CPU cpu, object val)
                {
                    var targetable = val as ITargetable;
                    if (targetable != null)
                    {
                        VesselUtils.SetTarget(targetable);
                    }
                    else
                    {
                        var vesselTarget = val as VesselTarget;
                        if (vesselTarget != null)
                        {
                            VesselUtils.SetTarget(vesselTarget.target);
                        }
                        else
                        {
                            var target = val as BodyTarget;
                            if (target != null)
                            {
                                VesselUtils.SetTarget(target.Target);
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
                                }
                            }
                        }
                    }
                });

            manager.AddGetter("TARGET", delegate(CPU cpu) 
                {
                    var currentTarget = FlightGlobals.fetch.VesselTarget;

                    var vessel = currentTarget as Vessel;
                    if (vessel != null)
                    {
                        return new VesselTarget(vessel, cpu);
                    }
                    var body = currentTarget as CelestialBody;
                    return body != null ? new BodyTarget(body, cpu) : null;
                });
        }
    }
}