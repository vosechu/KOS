using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace kOS.Command.FlowControl
{
    [AttributeCommand("^{([\\S\\s]*)}$")]
    public class BlockCommand : Command
    {
        readonly List<Command> commands = new List<Command>();
        String commandBuffer = "";

        public BlockCommand(Match regexMatch, ExecutionContext context) : base(regexMatch, context) { }

        public BlockCommand(string directInput, ExecutionContext context) : base(Regex.Match(directInput, "^([\\S\\s]*)$"), context) {}

        public override void Evaluate()
        {
            var innerText = RegexMatch.Groups[1].Value;
            string cmd;
            commandBuffer = innerText;
            var lineCount = Line;
            var commandLineStart = 0;

            while (ParseNext(ref innerText, out cmd, ref lineCount, out commandLineStart))
            {
                commands.Add(Get(cmd, this, commandLineStart));
            }

            State = (commands.Count == 0) ? ExecutionState.DONE : ExecutionState.WAIT;
        }

        public override void Refresh()
        {
            base.Refresh();

            foreach (Command c in commands)
            {
                c.Refresh();
            }
        }

        public new void Break()
        {
            commands.Clear();
            State = ExecutionState.DONE;
        }

        public override void Update(float time)
        {
            foreach (Command command in commands)
            {
                switch (command.State)
                {
                    case ExecutionState.NEW:
                        command.Evaluate();
                        ChildContext = command;
                        return;

                    case ExecutionState.WAIT:
                        command.Update(time);
                        return;
                }
            }

            State = ExecutionState.DONE;
        }
    }
}
