namespace kOS.Safe.Suffixed
{
    public interface IOperatable
    {
        object TryOperation(string op, object other, bool reverseOrder);
    }
}