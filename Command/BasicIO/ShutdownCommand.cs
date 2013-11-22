using System.Text.RegularExpressions;

namespace kOS
{
    [AttributeCommand("SHUTDOWN")]
    public class ShutdownCommand : Command
    {
        public ShutdownCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            ParentContext.SendMessage(SystemMessage.SHUTDOWN);
            State = ExecutionState.DONE;
        }
    }
}