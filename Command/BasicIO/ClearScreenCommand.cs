using System.Text.RegularExpressions;

namespace kOS
{    
    [AttributeCommand("CLEARSCREEN")]
    public class ClearScreenCommand : Command
    {
        public ClearScreenCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            ParentContext.SendMessage(SystemMessage.CLEARSCREEN);
            State = ExecutionState.DONE;
        }
    }
}
