using System.Text.RegularExpressions;
using kOS.Context;

namespace kOS.Command.FlowControl
{
    [AttributeCommand("CALL *")]
    public class CallExternalCommand : Command
    {
        public CallExternalCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            // External functions are now handled within expressions,
            // so simply execute the expression and throw away the value
            var subEx = new Expression(RegexMatch.Groups[1].Value, this);
            subEx.GetValue();
            
            State = ExecutionState.DONE;
        }
    }
}
