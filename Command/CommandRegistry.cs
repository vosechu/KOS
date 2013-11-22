using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace kOS.Command
{
    public static class CommandRegistry
    {
        public static Dictionary<string, Type> Bindings = new Dictionary<string, Type>();

        static CommandRegistry()
        {
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attr = (AttributeCommand)t.GetCustomAttributes(typeof(AttributeCommand), true).FirstOrDefault();
                if (attr != null)
                {
                    foreach (var s in attr.Values)
                    {
                        Bindings.Add(Utils.BuildRegex(s), t);
                    }
                }
            }
        }
    }
}