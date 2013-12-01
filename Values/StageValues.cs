using System;
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
            return GetResourceOfCurrentStage(suffixName);
        }

        private object GetResourceOfCurrentStage(String resourceName)
        {
            var activeEngines = vessel.GetListOfActivatedEngines();
            return Utils.ProspectForResource(resourceName, activeEngines);
        }
    }
}
