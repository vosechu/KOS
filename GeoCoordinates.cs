using UnityEngine;
using kOS.Craft;

namespace kOS
{
    public class GeoCoordinates : SpecialValue
    {
        public double Lat;
        public double Lng;
        public Vessel Vessel;
        public CelestialBody Body;

        public GeoCoordinates(Vessel vessel)
        {
            Lat = vessel.GetVesselLattitude();
            Lng = vessel.GetVesselLongitude();
            Vessel = vessel;

            Body = vessel.mainBody;
        }

        public GeoCoordinates(Vessel vessel, float lat, float lng)
        {
            Lat = lat;
            Lng = lng;
            Vessel = vessel;

            Body = vessel.mainBody;
        }

        public GeoCoordinates(Vessel vessel, double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
            Vessel = vessel;

            Body = vessel.mainBody;
        }

        public float GetBearing(Vessel vessel)
        {
            return VesselUtils.AngleDelta(vessel.GetHeading(), GetHeadingFromVessel(vessel));
        }
    
        public float GetHeadingFromVessel(Vessel vessel)
        {
            var up = vessel.upAxis;
            var north = vessel.GetNorthVector();

            var targetWorldCoords = vessel.mainBody.GetWorldSurfacePosition(Lat, Lng, vessel.altitude);
            
            var vector = Vector3d.Exclude(vessel.upAxis, targetWorldCoords - vessel.GetWorldPos3D()).normalized;
            var headingQ = Quaternion.Inverse(Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(Quaternion.LookRotation(vector, up)) * Quaternion.LookRotation(north, up));

            return headingQ.eulerAngles.y;
        }

        public double DistanceFrom(Vessel vessel)
        {
            return Vector3d.Distance(vessel.GetWorldPos3D(), Body.GetWorldSurfacePosition(Lat, Lng, vessel.altitude));
        }

        public override object GetSuffix(string suffixName)
        {
            if (suffixName == "LAT") return Lat;
            if (suffixName == "LNG") return Lng;
            if (suffixName == "DISTANCE") return DistanceFrom(Vessel);
            if (suffixName == "HEADING") return (double)GetHeadingFromVessel(Vessel);
            if (suffixName == "BEARING") return (double)GetBearing(Vessel);
            
            return base.GetSuffix(suffixName);
        }
        
        public override string ToString()
        {
            return "LATLNG(" + Lat + ", " + Lng + ")";
        }
    }
}
