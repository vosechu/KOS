using System.Text.RegularExpressions;
using kOS.Context;

namespace kOS.Command.BasicIO
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
