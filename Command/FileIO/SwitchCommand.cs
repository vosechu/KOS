using System.Text.RegularExpressions;

namespace kOS.Command.FileIO
{
    [AttributeCommand("SWITCH TO ^")]
    public class SwitchCommand : Command
    {
        public SwitchCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            var targetVolume = RegexMatch.Groups[1].Value.Trim();
            int volID;

            if (int.TryParse(targetVolume, out volID))
            {
                if (!ParentContext.SwitchToVolume(volID))
                {
                    throw new KOSException("Volume " + volID + " not found", this);
                }
            }
            else
            {
                if (!ParentContext.SwitchToVolume(targetVolume))
                {
                    throw new KOSException("Volume '" + targetVolume + "' not found", this);
                }
            }

            State = ExecutionState.DONE;
        }
    }
}