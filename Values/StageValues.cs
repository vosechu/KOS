using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Craft;

namespace kOS.Values
{
    public class StageValues : SpecialValue
    {
        readonly Vessel vessel;

        public StageValues(Vessel vessel)
        {
            this.vessel = vessel;
        }

        public override object GetSuffix(string suffixName)
        {
            var suffixSegment = suffixName.Split('#');
            if (suffixSegment.Count() == 2)
            {
		    if (suffixSegment.Last().ToUpper() == "SPENT")
		    {
			return GetSpentStatus(suffixSegment.First());
		    }
            }
            return GetResourceOfCurrentStage(suffixName);
        }

        private bool GetSpentStatus(string resource)
        {
	    //These parts have the named resource remaining
            var toReturn = vessel.parts.Where(part => Staging.CurrentStage - 1 == part.inverseStage && part.Resources[resource].amount > 0);

	    //exclude sepratrons
            toReturn = toReturn.Where(part => !part.IsSepratron());

            return toReturn.Any();
        }

        private object GetResourceOfCurrentStage(String resourceName)
        {
            var activeEngines = vessel.GetListOfActivatedEngines();
            return Utils.ProspectForResource(resourceName, activeEngines);
        }
    }
}
