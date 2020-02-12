namespace TraceSourceExample
{
    public static class Codes
    {
        public static string GetSimpleExample()
        {
            return @"using System;

namespace TraceSourceExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var b = Console.ReadLine();
            var a = 5;
            Console.WriteLine(""Hello World!"");
        }
    }
}";
        }
    }
}