using System;
using System.Text.RegularExpressions;

namespace kOS.Command.BasicIO
{
    [AttributeCommand("DECLARE %")]
    public class DeclareVariableCommand : Command
    {
        public DeclareVariableCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            String varName = RegexMatch.Groups[1].Value;
            Variable v = FindOrCreateVariable(varName);

            if (v == null) throw new KOSException("Can't create variable '" + varName + "'", this);
            
            State = ExecutionState.DONE;
        }
    }
}