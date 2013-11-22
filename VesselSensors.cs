using System;

namespace kOS
{
    public class VesselSensors : SpecialValue
    {
        readonly Vector acceleration = new Vector(0, 0, 0);
        readonly float pressure;
        readonly float temperature;
        readonly Vector geeForce = new Vector(0, 0, 0);
        readonly Single kerbolExposure;

        public VesselSensors(Vessel target)
        {
            foreach (var part in target.Parts)
            {
                if (part.State != PartStates.ACTIVE && part.State != PartStates.IDLE) continue;

                foreach (PartModule module in part.Modules)
                {
                    if (module is ModuleEnviroSensor)
                    {
                        switch (module.Fields.GetValue("sensorType").ToString())
                        {
                            case "ACC":
                                acceleration = new Vector(FlightGlobals.getGeeForceAtPosition(part.transform.position) - target.acceleration);
                                break;
                            case "PRES":
                                pressure = (Single)FlightGlobals.getStaticPressure();
                                break;
                            case "TEMP":
                                temperature = part.temperature;
                                break;
                            case "GRAV":
                                geeForce = new Vector(FlightGlobals.getGeeForceAtPosition(part.transform.position));
                                break;
                        }
                    }
                    foreach (var c in part.FindModulesImplementing<ModuleDeployableSolarPanel>())
                    {
                        kerbolExposure += c.sunAOA;
                    }
                }
            }
        }
        public override object GetSuffix(string suffixName)
        {
            if (suffixName == "ACC") return acceleration;
            if (suffixName == "PRES") return pressure;
            if (suffixName == "TEMP") return temperature;
            if (suffixName == "GRAV") return geeForce;
            if (suffixName == "LIGHT") return kerbolExposure;



            return base.GetSuffix(suffixName);
        }
    }
}
