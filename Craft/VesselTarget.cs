using System;
using UnityEngine;
using kOS.Context;
using kOS.Values;

namespace kOS.Craft
{
    public class VesselTarget : SpecialValue
    {
        public ExecutionContext context;
        public Vessel target;
        public static String[] ShortCuttableShipSuffixes;

        static VesselTarget()
        {
            ShortCuttableShipSuffixes = new[] 
            {
                "HEADING", "PROGRADE", "RETROGRADE", "FACING", "MAXTHRUST", "VELOCITY", "GEOPOSITION", "LATITUDE", "LONGITUDE", 
                "UP", "NORTH", "BODY", "ANGULARMOMENTUM", "ANGULARVEL", "MASS", "VERTICALSPEED", "SURFACESPEED", "AIRSPEED", "VESSELNAME", 
                "ALTITUDE", "APOAPSIS", "PERIAPSIS", "SENSOR"
            };
        }

        public VesselTarget(Vessel target, ExecutionContext context)
        {
            this.context = context;
            this.target = target;
        }

        public bool IsInRange(double range)
        {
            return GetDistance() <= range;
        }

        public double GetDistance()
        {
            return Vector3d.Distance(context.Vessel.GetWorldPos3D(), target.GetWorldPos3D());
        }

        public override string ToString()
        {
            return "VESSEL(\"" + target.vesselName + "\")";
        }

        public Direction GetPrograde()
        {
            var up = (target.findLocalMOI(target.findWorldCenterOfMass()) - target.mainBody.position).normalized;

            var d = new Direction {Rotation = Quaternion.LookRotation(target.orbit.GetVel().normalized, up)};
            return d;
        }

        public Direction GetRetrograde()
        {
            var up = (target.findLocalMOI(target.findWorldCenterOfMass()) - target.mainBody.position).normalized;

            var d = new Direction {Rotation = Quaternion.LookRotation(target.orbit.GetVel().normalized*-1, up)};
            return d;
        }

        public Direction GetFacing()
        {
            var facing = target.transform.up;
            return new Direction(new Vector3d(facing.x, facing.y, facing.z).normalized, false);
        }

        public override object GetSuffix(string suffixName)
        {
            switch (suffixName)
            {
                case "DIRECTION":
                    {
                        var vector = (target.GetWorldPos3D() - context.Vessel.GetWorldPos3D());
                        return new Direction(vector, false);
                    }
                case "DISTANCE":
                    return (float)GetDistance();
                case "BEARING":
                    return context.Vessel.GetTargetBearing(target);
                case "HEADING":
                    return context.Vessel.GetTargetHeading(target);
                case "PROGRADE":
                    return GetPrograde();
                case "RETROGRADE":
                    return GetRetrograde();
                case "MAXTHRUST":
                    return target.GetMaxThrust();
                case "TWR":
                    return target.GetThrustToWeight();
                case "VELOCITY":
                    return new VesselVelocity(target);
                case "GEOPOSITION":
                    return new GeoCoordinates(target);
                case "LATITUDE":
                    return target.GetVesselLattitude();
                case "LONGITUDE":
                    return target.GetVesselLongitude();
                case "FACING":
                    return GetFacing();
                case "UP":
                    return new Direction(target.upAxis, false);
                case "NORTH":
                    return new Direction(target.GetNorthVector(), false);
                case "BODY":
                    return target.mainBody.bodyName;
                case "ANGULARMOMENTUM":
                    return  new Direction(target.angularMomentum, true);
                case "ANGULARVEL":
                    return new Direction(target.angularVelocity, true);
                case "MASS":
                    return  target.GetTotalMass();
                case "VERTICALSPEED":
                    return  target.verticalSpeed;
                case "SURFACESPEED":
                    return  target.horizontalSrfSpeed;
                case "AIRSPEED":
                    return (target.orbit.GetVel() - FlightGlobals.currentMainBody.getRFrmVel(target.GetWorldPos3D())).magnitude; //the velocity of the vessel relative to the air);
                case "VESSELNAME":
                    return  target.vesselName;
                case "ALTITUDE":
                    return target.altitude;
                case "APOAPSIS":
                    return  target.orbit.ApA;
                case "PERIAPSIS":
                    return  target.orbit.PeA;
                case "SENSOR":
                    return new VesselSensors(target);
                case "TERMVELOCITY":
                    return target.GetTerminalVelocity();
            }

            // Is this a resource?
            double dblValue;
            if (target.TryGetResource(suffixName, out dblValue)) return dblValue;
            
            return null;
        }
    }
}
