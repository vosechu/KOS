using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace kOS
{
    public class VesselUtils
    {
        public static List<Part> GetListOfActivatedEngines(Vessel vessel)
        {
            return (from part in vessel.Parts
                    from PartModule module in part.Modules
                    where module is ModuleEngines
                    let engineMod = (ModuleEngines)module
                    where engineMod.getIgnitionState
                    select part).ToList();
        }

        public static bool TryGetResource(Vessel vessel, string resourceName, out double total)
        {
            var resourceIsFound = false;
            total = 0;
            var resourceDefinition = PartResourceLibrary.Instance.resourceDefinitions.FirstOrDefault(rd => rd.name.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
            // Ensure the built-in resource types never produce an error, even if the particular vessel is incapable of carrying them
            if (resourceDefinition == null)
                return false;
            resourceName = resourceName.ToUpper();
            foreach (var resource in
                from part in vessel.parts
                from PartResource resource in part.Resources
                where resource.resourceName.ToUpper() == resourceName
                select resource)
            {
                resourceIsFound = true;
                total += resource.amount;
            }

            return resourceIsFound;
        }

        public static double GetResource(Vessel vessel, string resourceName)
        {
            resourceName = resourceName.ToUpper();

            return (from part in vessel.parts
                    from PartResource resource in part.Resources
                    where resource.resourceName.ToUpper() == resourceName
                    select resource.amount).Sum();
        }

        public static double GetMaxThrust(Vessel vessel)
        {
            return (from p in vessel.parts
                    from PartModule pm in p.Modules
                    where pm.isEnabled
                    select pm).OfType<ModuleEngines>()
                    .Where(e => e.EngineIgnited)
                    .Aggregate(0.0, (current, e) => current + e.maxThrust);
        }

        public static Vessel TryGetVesselByName(String name, Vessel origin)
        {
            return FlightGlobals.Vessels.FirstOrDefault(v => v != origin && v.vesselName.ToUpper() == name.ToUpper());
        }

        public static CelestialBody GetBodyByName(String name)
        {
            return FlightGlobals.fetch.bodies.FirstOrDefault(body => name.ToUpper() == body.name.ToUpper());
        }

        public static Vessel GetVesselByName(String name, Vessel origin)
        {
            var vessel = TryGetVesselByName(name, origin);

            if (vessel == null)
            {
                throw new kOSException("Vessel '" + name + "' not found");
            }
            return vessel;
        }

        public static void SetTarget(ITargetable val)
        {
            //if (val is Vessel)
            //{
            FlightGlobals.fetch.SetVesselTarget(val);
            //}
            //else if (val is CelestialBody)
            //{/

            // }
        }

        public static double GetCommRange(Vessel vessel)
        {
            var range = vessel.parts
                .Where(part => part.partInfo.name == "longAntenna")
                .Select(part => ((ModuleAnimateGeneric) part.Modules["ModuleAnimateGeneric"]).status)
                .Where(status => status == "Fixed" || status == "Locked")
                .Aggregate<string, double>(100000, (current, status) => current + 1000000);

            range = vessel.parts
                .Where(part => part.partInfo.name == "mediumDishAntenna")
                .Select(part => ((ModuleAnimateGeneric) part.Modules["ModuleAnimateGeneric"]).status)
                .Where(status => status == "Fixed" || status == "Locked")
                .Aggregate(range, (current, status) => current*100);

            return vessel.parts
                .Where(part => part.partInfo.name == "commDish")
                .Select(part => ((ModuleAnimateGeneric) part.Modules["ModuleAnimateGeneric"]).status)
                .Where(status => status == "Fixed" || status == "Locked")
                .Aggregate(range, (current, status) => current*200);
        }

        public static double GetDistanceToKerbinSurface(Vessel vessel)
        {
            foreach (var body in FlightGlobals.fetch.bodies.Where(body => body.name.ToUpper() == "KERBIN"))
            {
                return Vector3d.Distance(body.position, vessel.GetWorldPos3D()) - 600000; // Kerbin radius = 600,000
            }

            throw new kOSException("Planet Kerbin not found!");
        }

        public static float AngleDelta(float a, float b)
        {
            var delta = b - a;

            while (delta > 180) delta -= 360;
            while (delta < -180) delta += 360;

            return delta;
        }

        public static float GetHeading(Vessel vessel)
        {
            var up = vessel.upAxis;
            var north = GetNorthVector(vessel);
            var headingQ = Quaternion.Inverse(Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(vessel.GetTransform().rotation) * Quaternion.LookRotation(north, up));

            return headingQ.eulerAngles.y;
        }

        public static float GetVelocityHeading(Vessel vessel)
        {
            var up = vessel.upAxis;
            var north = GetNorthVector(vessel);
            var headingQ = Quaternion.Inverse(Quaternion.Inverse(Quaternion.LookRotation(vessel.srf_velocity, up)) * Quaternion.LookRotation(north, up));

            return headingQ.eulerAngles.y;
        }

        public static float GetTargetBearing(Vessel vessel, Vessel target)
        {
            return AngleDelta(GetHeading(vessel), GetTargetHeading(vessel, target));
        }

        public static float GetTargetHeading(Vessel vessel, Vessel target)
        {
            var up = vessel.upAxis;
            var north = GetNorthVector(vessel);
            var vector = Vector3d.Exclude(vessel.upAxis, target.GetWorldPos3D() - vessel.GetWorldPos3D()).normalized;
            var headingQ = Quaternion.Inverse(Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(Quaternion.LookRotation(vector, up)) * Quaternion.LookRotation(north, up));

            return headingQ.eulerAngles.y;
        }

        public static Vector3d GetNorthVector(Vessel vessel)
        {
            return Vector3d.Exclude(vessel.upAxis, vessel.mainBody.transform.up);
        }

        public static object TryGetEncounter(Vessel vessel)
        {
            foreach (var patch in vessel.patchedConicSolver.flightPlan.Where(patch => patch.patchStartTransition == Orbit.PatchTransitionType.ENCOUNTER))
            {
                return new OrbitInfo(patch);
            }

            return "None";
        }

        public static void LandingLegsCtrl(Vessel vessel, bool state)
        {
            // This appears to work on all legs in 0.22
            vessel.rootPart.SendEvent(state ? "LowerLeg" : "RaiseLeg");
        }

        internal static object GetLandingLegStatus(Vessel vessel)
        {
            var atLeastOneLeg = false; // No legs at all? Always return false

            foreach (var p in vessel.parts.Where(p => p.Modules.OfType<ModuleLandingLeg>().Any()))
            {
                atLeastOneLeg = true;

                if (p.FindModulesImplementing<ModuleLandingLeg>().Any(l => l.savedLegState != (int)(ModuleLandingLeg.LegStates.DEPLOYED)))
                {
                    return false;
                }
            }

            return atLeastOneLeg;
        }

        public static object GetChuteStatus(Vessel vessel)
        {
            var atLeastOneChute = false; // No chutes at all? Always return false

            foreach (Part p in vessel.parts)
            {
                foreach (ModuleParachute c in p.FindModulesImplementing<ModuleParachute>())
                {
                    atLeastOneChute = true;

                    if (c.deploymentState == ModuleParachute.deploymentStates.STOWED)
                    {
                        // If just one chute is not deployed return false
                        return false;
                    }
                }
            }

            return atLeastOneChute;
        }

        public static void DeployParachutes(Vessel vessel, bool state)
        {
            if (!vessel.mainBody.atmosphere || !state) return;

            foreach (var c in from p in vessel.parts 
                              where p.Modules.OfType<ModuleParachute>().Any() 
                              from c in p.FindModulesImplementing<ModuleParachute>()
                              .Where(c => c.deploymentState == ModuleParachute.deploymentStates.STOWED) 
                              select c)
            {
                c.DeployAction(null);
            }
        }

        public static object GetSolarPanelStatus(Vessel vessel)
        {
            var atLeastOneSolarPanel = false; // No panels at all? Always return false

            foreach (var c in vessel.parts.SelectMany(p => p.FindModulesImplementing<ModuleDeployableSolarPanel>()))
            {
                atLeastOneSolarPanel = true;

                if (c.panelState == ModuleDeployableSolarPanel.panelStates.RETRACTED)
                {
                    // If just one panel is not deployed return false
                    return false;
                }
            }

            return atLeastOneSolarPanel;
        }

        public static void SolarPanelCtrl(Vessel vessel, bool state)
        {
            vessel.rootPart.SendEvent(state ? "Extend" : "Retract");
        }


        public static double GetMassDrag(Vessel vessel)
        {
            return vessel.parts.Aggregate<Part, double>(0, (current, p) => current + (p.mass + p.GetResourceMass())*p.maximum_drag);
        }

        public static double RealMaxAtmosphereAltitude(CelestialBody body)
        {
            // This comes from MechJeb CelestialBodyExtensions.cs
            if (!body.atmosphere) return 0;
            //Atmosphere actually cuts out when exp(-altitude / scale height) = 1e-6
            return -body.atmosphereScaleHeight * 1000 * Math.Log(1e-6);
        }

        public static double GetTerminalVelocity(Vessel vessel)
        {
            if (vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass()) > RealMaxAtmosphereAltitude(vessel.mainBody)) return double.PositiveInfinity;
            var densityOfAir = FlightGlobals.getAtmDensity(FlightGlobals.getStaticPressure(vessel.findWorldCenterOfMass(), vessel.mainBody));
            return Math.Sqrt(2 * FlightGlobals.getGeeForceAtPosition(vessel.findWorldCenterOfMass()).magnitude * vessel.GetTotalMass() / (GetMassDrag(vessel) * FlightGlobals.DragMultiplier * densityOfAir));
        }
        public static float GetVesselLattitude(Vessel vessel)
        {
            var retVal = (float)vessel.latitude;

            if (retVal > 90) return 90;
            if (retVal < -90) return -90;

            return retVal;
        }

        public static float GetVesselLongitude(Vessel vessel)
        {
            var retVal = (float)vessel.longitude;

            while (retVal > 180) retVal -= 360;
            while (retVal < -180) retVal += 360;

            return retVal;
        }
    }
}