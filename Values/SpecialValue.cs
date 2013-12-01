using System;

namespace kOS.Values
{
    public class SpecialValue : ISpecialValue
    {
        public virtual bool SetSuffix(String suffixName, object value)
        {
            return false;
        }

        public virtual object GetSuffix(String suffixName)
        {
            return null;
        }

        public virtual object TryOperation(string op, object other, bool reverseOrder)
        {
            return null;
        }
    }

    public interface ISpecialValue
    {
        bool SetSuffix(String suffixName, object value);
        object GetSuffix(String suffixName);
        object TryOperation(string op, object other, bool reverseOrder);
    }
}
