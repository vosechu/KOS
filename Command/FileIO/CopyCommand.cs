using System;
using System.Text.RegularExpressions;

namespace kOS
{
    [AttributeCommand("COPY &[TO,FROM][VOLUME]? ^")]
    public class CopyCommand : Command
    {
        public CopyCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public override void Evaluate()
        {
            String targetFile = RegexMatch.Groups[1].Value.Trim();
            String volumeName = RegexMatch.Groups[4].Value.Trim();
            String operation = RegexMatch.Groups[2].Value.Trim().ToUpper();

            Volume targetVolume = GetVolume(volumeName); // Will throw if not found

            File file = null;

            switch (operation)
            {
                case "FROM":
                    file = targetVolume.GetByName(targetFile);
                    if (file == null) throw new kOSException("File '" + targetFile + "' not found", this);
                    if (!SelectedVolume.SaveFile(new File(file))) throw new kOSException("File copy failed", this);
                    break;

                case "TO":
                    file = SelectedVolume.GetByName(targetFile);
                    if (file == null) throw new kOSException("File '" + targetFile + "' not found", this);
                    if (!targetVolume.SaveFile(new File(file))) throw new kOSException("File copy failed", this);
                    break;
            }

            State = ExecutionState.DONE;
        }
    }
}