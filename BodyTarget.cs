using System;
using kOS.Craft;

namespace kOS
{
    public class BodyTarget : SpecialValue
    {
        private CelestialBody target;
        private readonly ExecutionContext context;

        public BodyTarget(String name, ExecutionContext context) : this(VesselUtils.GetBodyByName(name), context) { }

        public BodyTarget(CelestialBody target, ExecutionContext context)
        {
            this.context = context;
            this.target = target;
        }

        public CelestialBody Target
        {
            get { return target; }
            set { target = value; }
        }

        public double GetDistance()
        {
            return Vector3d.Distance(context.Vessel.GetWorldPos3D(), target.position) - target.Radius;
        }

        public override object GetSuffix(string suffixName)
        {
            if (target == null) throw new KOSException("BODY structure appears to be empty!");

            switch (suffixName)
            {
                case "NAME":
                    return target.name;
                case "DESCRIPTION":
                    return target.bodyDescription;
                case "MASS":
                    return target.Mass;
                case "POSITION":
                    return new Vector(target.position);
                case "ALTITUDE":
                    return target.orbit.altitude;
                case "APOAPSIS":
                    return target.orbit.ApA;
                case "PERIAPSIS":
                    return target.orbit.PeA;
                case "VELOCITY":
                    return new Vector(target.orbit.GetVel());
                case "DISTANCE":
                    return GetDistance();
                case "BODY":
                    return new BodyTarget(target.orbit.referenceBody, context);
            }

            return base.GetSuffix(suffixName);
        }

        public override string ToString()
        {
 	        if (target != null)
            {
                return "BODY(\"" + target.name + "\")";
            }

            return base.ToString();
        }
    }


}
