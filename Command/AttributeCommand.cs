using System;

namespace kOS
{
    public class AttributeCommand : Attribute
    {
        public string[] Values { get; set; }
        public AttributeCommand(params string[] values) { Values = values; }

        public override String ToString()
        {
            return String.Join(",", Values);
        }
    }
}