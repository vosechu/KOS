using System;

namespace kOS.Values
{
    public abstract class SpecialValue : ISpecialValue
    {
        public virtual bool SetSuffix(String suffixName, object value)
        {
            return false;
        }

        public abstract object GetSuffix(String suffixName);

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
