using System.Text.RegularExpressions;
using kOS.Context;

namespace kOS.Command.Temporal
{
    [AttributeCommand("LOCK % TO *")]
    public class LockCommand : Command
    {
        public LockCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            var varname = RegexMatch.Groups[1].Value;
            var expression = new Expression(RegexMatch.Groups[2].Value, ParentContext);

            ParentContext.Unlock(varname);
            ParentContext.Lock(varname, expression);

            State = ExecutionState.DONE;
        }
    }
}