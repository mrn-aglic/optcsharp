using System;

namespace TracingCore.Exceptions
{
    public class ExitExecutionException : Exception
    {
        public override string Message => "Exit execution to prompt user for ReadLine input";
    }
}