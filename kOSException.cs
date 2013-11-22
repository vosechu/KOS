using System;

namespace kOS
{
    public class kOSException : Exception
    {
        public new String Message;
        public String Filename;
        public int LineNumber;
        public Command commandObj;
        public ExecutionContext Context;
        public ContextRunProgram Program;

        public kOSException(String message)
        {
            Message = message;
            //this.commandObj = commandObj;
        }

        public kOSException(String message, ExecutionContext context) : this (message)
        {
            LineNumber = context.Line;
            Context = context;
            Program = context.FindClosestParentOfType<ContextRunProgram>();
        }
    }

    public class kOSReadOnlyException : kOSException
    {
        public kOSReadOnlyException(String varName) : base (varName + " is read-only")
        {
        }
    }
}
