using System.Text.RegularExpressions;
using kOS.Context;
using kOS.Debug;

namespace kOS.Command.FileIO
{
    [AttributeCommand("EDIT &")]
    public class EditFileCommand : Command
    {
        public EditFileCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            var fileName = RegexMatch.Groups[1].Value;

            if (ParentContext is ImmediateMode)
            {
                ParentContext.Push(new InterpreterEdit(fileName, ParentContext));
            }
            else
            {
                throw new kOSException("Edit can only be used when in immediate mode.", this);
            }
        }
    }
}
