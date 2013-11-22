using System.Text.RegularExpressions;

namespace kOS.Command.BasicIO
{
    [AttributeCommand("TEST *")]
    public class TestKegexCommand : Command
    {
        public TestKegexCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            var e = new Expression(RegexMatch.Groups[1].Value, ParentContext);

            StdOut(e.GetValue().ToString());

            State = ExecutionState.DONE;
        }
    }
}