namespace kOS.Safe.Expression
{
    public interface IExpression
    {
        object GetValue();
        object GetValueOfTerm(ITerm term);
        bool IsNull();
        bool IsTrue();
        double Double();
        float Float();
        string ToString();
    }
}