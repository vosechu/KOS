using System.Collections.Generic;
using kOS.Debug;

namespace kOS.Values
{
    public class Node : SpecialValue
    {
        private ManeuverNode nodeRef;
        private Vessel vesselRef;
        private double ut;
        private double prograde;
        private double radialOut;
        private double normal;

        private static readonly Dictionary<ManeuverNode, Node> _nodeLookup = new Dictionary<ManeuverNode, Node>();

        public Node(double ut, double radialOut, double normal, double prograde)
        {
            this.ut = ut;
            this.prograde = prograde;
            this.radialOut = radialOut;
            this.normal = normal;
        }

        public Node(Vessel v, ManeuverNode existingNode)
        {
            nodeRef = existingNode;
            vesselRef = v;
            _nodeLookup.Add(existingNode, this);

            UpdateValues();
        }

        public static Node FromExisting(Vessel v, ManeuverNode existingNode)
        {
            return _nodeLookup.ContainsKey(existingNode) ? _nodeLookup[existingNode] : new Node(v, existingNode);
        }

        public void AddToVessel(Vessel v)
        {
            if (nodeRef != null) throw new kOSException("Node has already been added");

            vesselRef = v;
            nodeRef = v.patchedConicSolver.AddManeuverNode(ut);

            UpdateNodeDeltaV();

            v.patchedConicSolver.UpdateFlightPlan();

            _nodeLookup.Add(nodeRef, this);
        }

        private void UpdateAll()
        {
            UpdateNodeDeltaV();
            if (vesselRef != null) vesselRef.patchedConicSolver.UpdateFlightPlan();
        }

        private void UpdateNodeDeltaV()
        {
            if (nodeRef == null) return;
            var dv = new Vector3d(radialOut, normal, prograde);
            nodeRef.DeltaV = dv;
        }

        private void CheckNodeRef()
        {
            if (nodeRef == null)
            {
                throw new kOSException("Must attach node first");
            }
        }

        public Vector GetBurnVector()
        {
            CheckNodeRef();

            return new Vector(nodeRef.GetBurnVector(vesselRef.GetOrbit()));
        }

        private void UpdateValues()
        {
            // If this node is attached, and the values on the attached node have chaged, I need to reflect that
            if (nodeRef == null) return;

            ut = nodeRef.UT;

            radialOut = nodeRef.DeltaV.x;
            normal = nodeRef.DeltaV.y;
            prograde = nodeRef.DeltaV.z;
        }
                
        public override object GetSuffix(string suffixName)
        {
            UpdateValues();
            
            switch (suffixName)
            {
                case "BURNVECTOR":
                    return GetBurnVector();
                case "ETA":
                    return ut - Planetarium.GetUniversalTime();
                case "DELTAV":
                    return GetBurnVector();
                case "PROGRADE":
                    return prograde;
                case "RADIALOUT":
                    return radialOut;
                case "NORMAL":
                    return normal;
                case "APOAPSIS":
                    if (nodeRef == null) throw new kOSException("Node must be added to flight plan first");
                    return nodeRef.nextPatch.ApA;
                case "PERIAPSIS":
                    if (nodeRef == null) throw new kOSException("Node must be added to flight plan first");
                    return nodeRef.nextPatch.PeA;
                default:
                    return null;
            }
        }

        public override bool SetSuffix(string suffixName, object value)
        {
            switch (suffixName)
            {
                case "DELTAV":
                case "ETA":
                case "BURNVECTOR":
                    throw new kOSReadOnlyException(suffixName);
                case "PROGRADE":
                    prograde = (double)value;
                    UpdateAll();
                    return true;
                case "RADIALOUT":
                    radialOut = (double)value;
                    UpdateAll();
                    return true;
                case "NORMAL":
                    normal = (double)value;
                    UpdateAll();
                    return true;
            }

            return false;
        }

        public void Remove()
        {
            if (nodeRef == null) return;
            _nodeLookup.Remove(nodeRef);

            vesselRef.patchedConicSolver.RemoveManeuverNode(nodeRef);

            nodeRef = null;
            vesselRef = null;
        }

        public override string ToString()
        {
            return "NODE(" + ut + "," + radialOut + "," + normal + "," + prograde + ")";
        }
    }
}
