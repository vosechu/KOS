using System.Text.RegularExpressions;

namespace kOS.Command.FlowControl
{
    [AttributeCommand("UNTIL ~_{}")]
    public class UntilLoopCommand : Command
    {
        Expression waitExpression;
        // commandString;
        Command targetCommand;

        public UntilLoopCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            waitExpression = new Expression(RegexMatch.Groups[1].Value, ParentContext);

            var numLinesChild = Utils.NewLineCount(Input.Substring(0, RegexMatch.Groups[2].Index));
            targetCommand = Get(RegexMatch.Groups[2].Value, this, Line + numLinesChild);

            State = ExecutionState.WAIT;
        }

        public override bool Break()
        {
            State = ExecutionState.DONE;

            return true;
        }

        public override bool SpecialKey(kOSKeys key)
        {
            if (key == kOSKeys.BREAK)
            {
                StdOut("Break.");
                Break();
            }

            return base.SpecialKey(key);
        }

        public override void Update(float time)
        {
            base.Update(time);

            if (ChildContext == null)
            {
                if (waitExpression.IsTrue())
                {
                    State = ExecutionState.DONE;
                }
                else
                {
                    ChildContext = targetCommand;
                    ((Command)ChildContext).Evaluate();
                }
            }
            else
            {
                if (ChildContext.State == ExecutionState.DONE)
                {
                    ChildContext = null;
                }
            }
        }
    }
}