using System.Text.RegularExpressions;

namespace kOS.Command.Temporal
{
    [AttributeCommand("WHEN ~ THEN *")]
    public class WhenCommand : Command
    {
        private Command targetCommand;
        private Expression expression;
        private bool triggered = false;

        public WhenCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            expression = new Expression(RegexMatch.Groups[1].Value, ParentContext);
            targetCommand = Command.Get(RegexMatch.Groups[2].Value, ParentContext);

            ParentContext.Lock(this);

            State = ExecutionState.DONE;
        }

        public override void Update(float time)
        {
            if (triggered)
            {
                targetCommand.Update(time);
                if (targetCommand.State == ExecutionState.DONE) ParentContext.Unlock(this);
            }
            else if (expression.IsTrue())
            {
                triggered = true;
                targetCommand.Evaluate();
            }
        }
    }
}