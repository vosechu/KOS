using System.Collections.Generic;
using System.Linq;

namespace kOS.Utilities
{
    public static class Utils
    {
        public static double ProspectForResource(string resourceName, List<Part> engines)
        {
            var visited = new List<Part>();

            return engines.Sum(part => ProspectForResource(resourceName, part, ref visited));
        }

        public static double ProspectForResource(string resourceName, Part engine)
        {
            var visited = new List<Part>();

            return ProspectForResource(resourceName, engine, ref visited);
        }

        public static double ProspectForResource(string resourceName, Part part, ref List<Part> visited)
        {
            double ret = 0;

            if (visited.Contains(part))
            {
                return 0;
            }

            visited.Add(part);

            foreach (PartResource resource in part.Resources)
            {
                if (resource.resourceName.ToLower() == resourceName.ToLower())
                {
                    ret += resource.amount;
                }
            }

            foreach (var attachNode in part.attachNodes)
            {
                if (attachNode.attachedPart != null //if there is a part attached here            
                    && attachNode.nodeType == AttachNode.NodeType.Stack
                    //and the attached part is stacked (rather than surface mounted)
                    && (attachNode.attachedPart.fuelCrossFeed //and the attached part allows fuel flow
                       )
                    && !(part.NoCrossFeedNodeKey.Length > 0 //and this part does not forbid fuel flow
                         && attachNode.id.Contains(part.NoCrossFeedNodeKey))) //    through this particular node
                {


                    ret += ProspectForResource(resourceName, attachNode.attachedPart, ref visited);
                }
            }

            return ret;
        }

    }
} 

 
