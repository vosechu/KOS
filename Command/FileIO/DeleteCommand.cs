using System;
using System.Text.RegularExpressions;
using kOS.Context;
using kOS.Debug;

namespace kOS.Command.FileIO
{
    [AttributeCommand("DELETE &[FROM,FROM VOLUME]?[:^]?")]
    public class DeleteCommand : Command
    {
        public DeleteCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            String targetFile = RegexMatch.Groups[1].Value.Trim();
            String volumeName = RegexMatch.Groups[3].Value.Trim();

            File file = null;

            if (volumeName.Trim() != "")
            {
                Volume targetVolume = GetVolume(volumeName);
                file = targetVolume.GetByName(targetFile);
                if (file == null) throw new kOSException("File '" + targetFile + "' not found", this);
                targetVolume.DeleteByName(targetFile);
            }
            else
            {
                file = SelectedVolume.GetByName(targetFile);
                if (file == null) throw new kOSException("File '" + targetFile + "' not found", this);
                SelectedVolume.DeleteByName(targetFile);
            }

            State = ExecutionState.DONE;
        }
    }
}