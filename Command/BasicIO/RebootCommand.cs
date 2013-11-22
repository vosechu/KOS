using System.Text.RegularExpressions;

namespace kOS
{
    [AttributeCommand("REBOOT")]
    public class RebootCommand : Command
    {
        public RebootCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            ParentContext.SendMessage(SystemMessage.RESTART);
            State = ExecutionState.DONE;
        }
    }
}