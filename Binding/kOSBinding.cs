using System;

namespace kOS.Binding
{
    public class kOSBinding : Attribute
    {
        public string[] Contexts;
        public kOSBinding(params string[] contexts) { Contexts = contexts; }
    }
}