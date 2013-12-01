using System.Text.RegularExpressions;
using kOS.Context;
using kOS.Debug;
using kOS.Values;

namespace kOS.Command.Vessel
{
    [AttributeCommand("ADD *")]
    public class AddObjectToVesselCommand : Command
    {
        public AddObjectToVesselCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            var ex = new Expression(RegexMatch.Groups[1].Value, this);
            var obj = ex.GetValue();

            if (obj is Node)
            {
                ((Node)obj).AddToVessel(Vessel);
            }
            else
            {
                throw new kOSException("Supplied object ineligible for adding", this);
            }

            State = ExecutionState.DONE;
        }
    }
}