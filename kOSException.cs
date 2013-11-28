using System;

namespace kOS
{
    [Serializable]
    public class KOSException : Exception
    {
        public new String Message;
        public String Filename;
        public int LineNumber;
        public Command.Command CommandObj;
        public ExecutionContext Context;
        public ContextRunProgram Program;

        public KOSException(String message)
        {
            Message = message;
            //this.commandObj = commandObj;
        }

        public KOSException(String message, ExecutionContext context) : this (message)
        {
            LineNumber = context.Line;
            Context = context;
            Program = context.FindClosestParentOfType<ContextRunProgram>();
        }
    }

    public class KOSReadOnlyException : KOSException
    {
        public KOSReadOnlyException(String varName) : base (varName + " is read-only")
        {
        }
    }
}
