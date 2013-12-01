using System.Text.RegularExpressions;
using kOS.Context;
using kOS.Debug;

namespace kOS.Command.BasicIO
{
    [AttributeCommand("DECLARE PARAMETERS? *")]
    public class DeclareParameterCommand : Command
    {
        public DeclareParameterCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            if (!(ParentContext is ContextRunProgram)) throw new kOSException("DECLARE PARAMETERS can only be used within a program.", this);

            foreach (var varName in RegexMatch.Groups[1].Value.Split(','))
            {
                var v = FindOrCreateVariable(varName);
                if (v == null) throw new kOSException("Can't create variable '" + varName + "'", this);

                var program = (ContextRunProgram)ParentContext;
                v.Value = program.PopParameter();
            }

            State = ExecutionState.DONE;
        }
    }
}