using System.Text.RegularExpressions;
using kOS.Context;
using kOS.Debug;
using kOS.Values;

namespace kOS.Command.BasicIO
{
    [AttributeCommand("SET ~ TO *")]
    public class SetCommand : Command
    { 
        public SetCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            var targetTerm = new Term(RegexMatch.Groups[1].Value);
            var e = new Expression(RegexMatch.Groups[2].Value, ParentContext);

            if (targetTerm.Type == Term.TermTypes.STRUCTURE)
            {
                var baseObj = new Expression(targetTerm.SubTerms[0], ParentContext).GetValue();

                var value = baseObj as ISpecialValue;
                if (value != null)
                {
                    if (value.SetSuffix(targetTerm.SubTerms[1].Text.ToUpper(), e.GetValue()))
                    {
                        State = ExecutionState.DONE;
                        return;
                    }
                    throw new kOSException("Suffix '" + targetTerm.SubTerms[1].Text + "' doesn't exist or is read only", this);
                }
                throw new kOSException("Can't set subvalues on a " + Expression.GetFriendlyNameOfItem(baseObj), this);
            }
            var v = FindOrCreateVariable(targetTerm.Text);

            if (v == null) return;
            v.Value = e.GetValue();
            State = ExecutionState.DONE;
        }
    }
}