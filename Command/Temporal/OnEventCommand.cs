using System;
using System.Text.RegularExpressions;

namespace kOS.Command.Temporal
{
    [AttributeCommand("ON % *")]
    public class OnEventCommand : Command
    {
        private Variable targetVariable;
        private Command targetCommand;
        private bool originalValue;

        public OnEventCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            targetVariable = ParentContext.FindOrCreateVariable(RegexMatch.Groups[1].Value);
            targetCommand = Command.Get(RegexMatch.Groups[2].Value, ParentContext);

            if (!objToBool(targetVariable.Value, out originalValue))
            {
                throw new Exception("Value type error");
            }

            ParentContext.Lock(this);

            State = ExecutionState.DONE;
        }

        public override void Update(float time)
        {
            bool newValue;
            if (!objToBool(targetVariable.Value, out newValue))
            {
                ParentContext.Unlock(this);

                throw new Exception("Value type error");
            }

            if (originalValue == newValue) return;
            ParentContext.Unlock(this);

            targetCommand.Evaluate();
            ParentContext.Push(targetCommand);
        }

        public bool objToBool(object obj, out bool result)
        {
            if (bool.TryParse(targetVariable.Value.ToString(), out result))
            {
                return true;
            }
            if (obj is float)
            {
                result = ((float)obj) > 0;
                return true;
            }

            return false;
        }
    }
}