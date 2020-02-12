using System;
using System.Threading;
using System.Threading.Tasks;

namespace TracingCore.Interceptors
{
    public class ReadLineHandler
    {
        public ReadLineHandler()
        {
            
        }

        public void HandleReadLine()
        {
            var currentThread = Thread.CurrentThread;
            var x = Task.Factory.StartNew(() => RequireUserInput(currentThread));
            // currentThread.Suspend();
            var inputResult = x.Result;
        }

        public string RequireUserInput(Thread currentThread)
        {
            var userInput = Console.ReadLine();
            return userInput;
        }
    }
}