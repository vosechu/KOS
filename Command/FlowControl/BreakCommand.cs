using System.Text.RegularExpressions;

namespace kOS
{
    [AttributeCommand("BREAK")]
    public class BreakCommand : Command
    {
        public BreakCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            Break();
            State = ExecutionState.DONE;
        }
    }
}