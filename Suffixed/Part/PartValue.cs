﻿using System.Collections.Generic;

namespace kOS.Suffixed.Part
{
    public class PartValue : SpecialValue
    {
        public PartValue(global::Part part)
        {
            Part = part;
        }

        protected global::Part Part { get; private set; }

        public override object GetSuffix(string suffixName)
        {
            switch (suffixName)
            {
                case "NAME":
                    return Part.name;
                case "STAGE":
                    return Part.inverseStage;
                case "UID":
                    return Part.uid;
                case "RESOURCES":
                    var resources = new ListValue();
                    foreach (PartResource resource in Part.Resources)
                    {
                        resources.Add(new ResourceValue(resource));
                    }
                    return resources;
                case "MODULES":
                    var modules = new ListValue();
                    foreach (var module in Part.Modules)
                    {
                        modules.Add(module.GetType());
                    }
                    return modules;
            }
            return base.GetSuffix(suffixName);
        }

        public override string ToString()
        {
            return string.Format("PART({0},{1})", Part.name, Part.uid);
        }

        public static ListValue PartsToList(IEnumerable<global::Part> parts)
        {
            var toReturn = new ListValue();
            foreach (var part in parts)
            {
                toReturn.Add(new PartValue(part));
            }
            return toReturn;
        }
    }
}