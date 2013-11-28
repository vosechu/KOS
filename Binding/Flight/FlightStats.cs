using System;
using System.Linq;
using UnityEngine;
using kOS.Craft;

namespace kOS.Binding.Flight
{

    [kOSBinding("ksp")]
    public class FlightStats : Binding
    {
        public override void AddTo(BindingManager manager)
        {
            manager.AddSmooth("ALT:RADAR",
                              cpu => cpu.Vessel.heightFromTerrain > 0
                                         ? Math.Min(cpu.Vessel.heightFromTerrain, cpu.Vessel.altitude)
                                         : cpu.Vessel.altitude);
            manager.AddSmooth("ALT:APOAPSIS", cpu => cpu.Vessel.orbit.ApA);
            manager.AddSmooth("ALT:PERIAPSIS", cpu => cpu.Vessel.orbit.PeA);
            manager.AddSmooth("ETA:APOAPSIS", cpu => cpu.Vessel.orbit.timeToAp);
            manager.AddSmooth("ETA:PERIAPSIS", cpu => cpu.Vessel.orbit.timeToPe);
            manager.AddSmooth("OBT:PERIOD", cpu => cpu.Vessel.orbit.period);
            manager.AddSmooth("OBT:INCLINATION", cpu => cpu.Vessel.orbit.inclination);
            manager.AddSmooth("OBT:ECCENTRICITY", cpu => cpu.Vessel.orbit.eccentricity);
            manager.AddSmooth("OBT:SEMIMAJORAXIS", cpu => cpu.Vessel.orbit.semiMajorAxis);
            manager.AddSmooth("OBT:SEMIMINORAXIS", cpu => cpu.Vessel.orbit.semiMinorAxis);
            manager.AddSmooth("OBT:SEMIMINORAXIS", cpu => cpu.Vessel.orbit.semiMinorAxis);

            manager.AddGetter("MISSIONTIME", cpu => cpu.Vessel.missionTime);
            manager.AddGetter("TIME", cpu => new TimeSpan(Planetarium.GetUniversalTime()));

            manager.AddGetter("STATUS", cpu => cpu.Vessel.situation.ToString().Replace("_", " "));
            manager.AddGetter("COMMRANGE", cpu => cpu.Vessel.GetCommRange());
            manager.AddGetter("INCOMMRANGE", cpu => CheckCommRange(cpu.Vessel));

            manager.AddGetter("AV",
                              cpu =>
                              cpu.Vessel.transform.InverseTransformDirection(cpu.Vessel.rigidbody.angularVelocity));
            manager.AddGetter("STAGE", cpu => new StageValues(cpu.Vessel));

            manager.AddGetter("ENCOUNTER", cpu => cpu.Vessel.TryGetEncounter());

            manager.AddGetter("NEXTNODE",       delegate(CPU cpu)
            {
                var vessel = cpu.Vessel;
                if (!vessel.patchedConicSolver.maneuverNodes.Any()) { throw new KOSException("No maneuver nodes present!"); }

                return Node.FromExisting(vessel, vessel.patchedConicSolver.maneuverNodes[0]);
            });

            // Things like altitude, mass, maxthrust are now handled the same for other ships as the current ship
            manager.AddGetter("SHIP", cpu => new VesselTarget(cpu.Vessel, cpu));

            // These are now considered shortcuts to SHIP:suffix
            foreach (var scName in VesselTarget.ShortCuttableShipSuffixes)
            {
                var name = scName;
                manager.AddGetter(scName, cpu => new VesselTarget(cpu.Vessel, cpu).GetSuffix(name));
            }

            manager.AddSetter("VESSELNAME", delegate(CPU cpu, object value) { cpu.Vessel.vesselName = value.ToString(); });
            }

        private static bool CheckCommRange(Vessel vessel)
            {
                return (vessel.GetDistanceToKerbinSurface() < vessel.GetCommRange());
            }
  }
}
