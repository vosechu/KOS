using System.Text.RegularExpressions;

namespace kOS
{
    [AttributeCommand("TOGGLE %")]
    public class ToggleCommand : Command
    {
        public ToggleCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            var varName = RegexMatch.Groups[1].Value;
            var v = FindOrCreateVariable(varName);

            if (v == null)
            {
                throw new kOSException("Can't find or create variable '" + varName + "'", this);
            }
            if (v.Value is bool)
            {
                v.Value = !((bool) v.Value);
                State = ExecutionState.DONE;
            }
            else if (v.Value is float)
            {
                var val = ((float) v.Value > 0);
                v.Value = !val;
                State = ExecutionState.DONE;
            }
            else
            {
                throw new kOSException("That variable can't be toggled.", this);
            }
        }
    }
}