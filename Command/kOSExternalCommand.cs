using System;

namespace kOS.Command
{
    public struct KOSExternalFunction
    {
        public String Name;
        public object Parent;
        public String MethodName;
        public int ParameterCount;
        public String Regex;

        public KOSExternalFunction(String name, object parent, String methodName, int parameterCount)
        {
            Name = name;
            Parent = parent;
            ParameterCount = parameterCount;
            MethodName = methodName;

            Regex = Utils.BuildRegex(name + "_(" + parameterCount + ")");
        }
    }
}
