using System;

namespace kOS
{
    public struct kOSExternalFunction
    {
        public String Name;
        public object Parent;
        public String MethodName;
        public int ParameterCount;
        public String regex;

        public kOSExternalFunction(String name, object parent, String methodName, int parameterCount)
        {
            Name = name;
            Parent = parent;
            ParameterCount = parameterCount;
            MethodName = methodName;

            regex = Utils.BuildRegex(name + "_(" + parameterCount + ")");
        }
    }
}
