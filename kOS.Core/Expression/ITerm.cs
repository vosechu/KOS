using System.Collections.Generic;

namespace kOS.Expression
{
    public interface ITerm
    {
        void CopyFrom(ref ITerm from);
        ITerm Merge(params ITerm[] terms);
        string Demo();
        string Demo(int tabIndent);
        TermType Type { get; set; }
        string Text { get; set; }
        List<ITerm> SubTerms { get; set; }
    }
}