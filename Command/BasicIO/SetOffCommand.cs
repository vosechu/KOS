using System.Text.RegularExpressions;

namespace kOS.Command.BasicIO
{
    [AttributeCommand("% OFF")]
    public class SetOffCommand : Command
    {
        public SetOffCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            var varName = RegexMatch.Groups[1].Value;
            var v = FindOrCreateVariable(varName);

            if (v == null)
            {
                throw new kOSException("Can't find or create variable '" + varName + "'", this);
            }
            if (!(v.Value is bool) && !(v.Value is float))
            {
                throw new kOSException("That variable can't be set to 'OFF'.", this);
            }
            v.Value = false;
            State = ExecutionState.DONE;
        }
    }
}