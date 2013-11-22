using System;

namespace kOS
{
    public class kOSBinding : Attribute
    {
        public string[] Contexts;
        public kOSBinding(params string[] contexts) { Contexts = contexts; }
    }
}