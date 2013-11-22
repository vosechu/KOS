using System;
using System.Text.RegularExpressions;

namespace kOS.Command.FileIO
{
    [AttributeCommand("LIST[VOLUMES,FILES]?")]
    public class ListCommand : Command
    {
        public ListCommand(Match regexMatch, ExecutionContext context) : base(regexMatch,  context) { }

        public override void Evaluate()
        {
            String listType = RegexMatch.Groups[1].Value.Trim().ToUpper();

            if (listType == "FILES" || String.IsNullOrEmpty(listType))
            {
                StdOut("");

                StdOut("Volume " + GetVolumeBestIdentifier(SelectedVolume));
                StdOut("-------------------------------------");                

                foreach (FileInfo fileInfo in SelectedVolume.GetFileList())
                {
                    StdOut(fileInfo.Name.PadRight(30, ' ') + fileInfo.Size.ToString());
                }
                
                int freeSpace = SelectedVolume.GetFreeSpace();
                StdOut("Free space remaining: " + (freeSpace > -1 ? freeSpace.ToString() : " infinite"));

                StdOut("");

                State = ExecutionState.DONE;
                return;
            }
            if (listType == "VOLUMES")
            {
                StdOut("");
                StdOut("ID    Name                    Size");
                StdOut("-------------------------------------");

                int i = 0;

                foreach (Volume volume in Volumes)
                {
                    String id = i.ToString();
                    if (volume == SelectedVolume) id = "*" + id;

                    String line = id.PadLeft(2).PadRight(6, ' ');
                    line += volume.Name.PadRight(24, ' ');

                    String size = volume.CheckRange() ? (volume.Capacity > -1 ? volume.Capacity.ToString() : "Inf") : "Disc";
                    line += size;

                    StdOut(line);

                    i++;
                }

                StdOut("");

                State = ExecutionState.DONE;
                return;
            }

            throw new kOSException("List type '" + listType + "' not recognized.", this);
        }
    }
}