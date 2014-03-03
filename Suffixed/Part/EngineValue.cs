﻿using System.Collections.Generic;

namespace kOS.Suffixed.Part
{
    public class EngineValue : PartValue
    {
        private readonly ModuleEnginesFX enginefFx;
        private readonly ModuleEngines engine;

        public EngineValue(global::Part part, ModuleEngines engine) : base(part)
        {
            this.engine = engine;
        }

        public EngineValue(global::Part part, ModuleEnginesFX enginefFx) : base(part)
        {
            this.enginefFx = enginefFx;
        }

        public override bool SetSuffix(string suffixName, object value)
        {
            if (engine == null)
            {
                return SetEngineSuffix(suffixName, value, engine);
            }
            if (enginefFx == null)
            {
                return SetEngineFxSuffix(suffixName, value, enginefFx);
            }
            return base.SetSuffix(suffixName, value);
        }

        private bool SetEngineFxSuffix(string suffixName, object value, ModuleEnginesFX moduleEnginesFx)
        {
            switch (suffixName)
            {
                case "ACTIVE":
                    var activate = (bool) value;
                    if (activate)
                    {
                        moduleEnginesFx.Activate();
                    }
                    else
                    {
                        moduleEnginesFx.Shutdown();
                    }
                    return true;
                case "THRUSTLIMIT":
                    var throttlePercent = (float) value;
                    moduleEnginesFx.thrustPercentage = throttlePercent;
                    return false;
            }
            return base.SetSuffix(suffixName, value);
        }

        private bool SetEngineSuffix(string suffixName, object value, ModuleEngines moduleEngines)
        {
            switch (suffixName)
            {
                case "ACTIVE":
                    var activate = (bool) value;
                    if (activate)
                    {
                        moduleEngines.Activate();
                    }
                    else
                    {
                        moduleEngines.Shutdown();
                    }
                    return true;
                case "THRUSTLIMIT":
                    var throttlePercent = (float) value;
                    moduleEngines.thrustPercentage = throttlePercent;
                    return false;
            }
            return base.SetSuffix(suffixName, value);
        }


        public override object GetSuffix(string suffixName)
        {
            if (engine == null)
            {
                return GetEngineSuffix(suffixName, engine);
            }
            if (enginefFx == null)
            {
                return GetEngineFxSuffix(suffixName, enginefFx);
            }
            return base.GetSuffix(suffixName);
        }

        private object GetEngineSuffix(string suffixName, ModuleEngines moduleEngines)
        {
            switch (suffixName)
            {
                case "MAXTHRUST":
                    return (double)moduleEngines.maxThrust;
                case "THRUST":
                    return (double)moduleEngines.finalThrust;
                case "FUELFLOW":
                    return (double)moduleEngines.fuelFlowGui;
                case "ISP":
                    return (double)moduleEngines.realIsp;
                case "FLAMEOUT":
                    return moduleEngines.getFlameoutState;
                case "IGNITION":
                    return moduleEngines.getIgnitionState;
                case "ALLOWRESTART":
                    return moduleEngines.allowRestart;
                case "ALLOWSHUTDOWN":
                    return moduleEngines.allowShutdown;
                case "THROTTLELOCK":
                    return moduleEngines.throttleLocked;
                case "THRUSTLIMIT":
                    return (double)moduleEngines.thrustPercentage;
            }
            return base.GetSuffix(suffixName);
        }

        private object GetEngineFxSuffix(string suffixName, ModuleEnginesFX moduleEngines)
        {
            switch (suffixName)
            {
                case "MAXTHRUST":
                    return (double)moduleEngines.maxThrust;
                case "THRUST":
                    return (double)moduleEngines.finalThrust;
                case "FUELFLOW":
                    return (double)moduleEngines.fuelFlowGui;
                case "ISP":
                    return (double)moduleEngines.realIsp;
                case "FLAMEOUT":
                    return moduleEngines.getFlameoutState;
                case "IGNITION":
                    return moduleEngines.getIgnitionState;
                case "ALLOWRESTART":
                    return moduleEngines.allowRestart;
                case "ALLOWSHUTDOWN":
                    return moduleEngines.allowShutdown;
                case "THROTTLELOCK":
                    return moduleEngines.throttleLocked;
                case "THRUSTLIMIT":
                    return (double)moduleEngines.thrustPercentage;
            }
            return base.GetSuffix(suffixName);
        }

        public new static ListValue PartsToList(IEnumerable<global::Part> parts)
        {
            var toReturn = new ListValue();
            foreach (var part in parts)
            {
                foreach (PartModule module in part.Modules)
                {
                    var engineModule = module as ModuleEngines;
                    if (engineModule != null)
                    {
                        toReturn.Add(new EngineValue(part, engineModule));
                    }
                    var engineModuleFx = module as ModuleEnginesFX;
                    if (engineModuleFx != null)
                    {
                        toReturn.Add(new EngineValue(part, engineModuleFx));
                    }
                }
            }
            return toReturn;
        }
    }
}