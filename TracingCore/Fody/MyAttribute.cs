using System;

namespace TracingCore.Fody
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class MyAttribute : Attribute
    {
        private int line;

        public MyAttribute(int line)
        {
            Console.WriteLine(line);
            this.line = line;
        }

        public virtual int Line => line;
    }
}