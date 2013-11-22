using System.Text.RegularExpressions;

namespace kOS
{
    [AttributeCommand("STAGE")]
    class VesselStageCommand : Command
    {
        public VesselStageCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            Staging.ActivateNextStage();

            State = ExecutionState.DONE;
        }
    }
}