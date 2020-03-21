using System;
using System.IO;

namespace TraceSourceExample
{
    public static class Codes
    {
        private static string GetFileContents(string filename)
        {
            var directory =
                AppContext.BaseDirectory.Substring(0,
                    AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal));
            var filePath = $"{directory}CodeExamples/{filename}";
            return File.ReadAllText(filePath);
        }

        public static string GetIfElseExample()
        {
            return GetFileContents("IfElse.txt");
        }

        public static string GetForExample(int id)
        {
            return GetFileContents($"For_{id}.txt");
        }

        public static string GetWhileExample(int id)
        {
            return GetFileContents($"While_{id}.txt");
        }

        public static string GetOOPExample(string name)
        {
            return GetFileContents($"OOP_{name}.txt");
        }

        public static string GetSimpleExample()
        {
            return @"using System;

namespace TraceSourceExample
{
    class Program
    {
        static int Test(){
            var x = 5;
            return x + 1;
        }

        static void Main(string[] args)
        {
            int c;
            var a = 5;
            var b = a + 2;
            Console.WriteLine(""Hello World!"");
        }
    }
}";
        }

        public static string GetMethodsExample()
        {
            return GetFileContents("MethodsExample.txt");
        }

        public static string GetSimpleExampleWithMethod()
        {
            return @"using System;

namespace TraceSourceExample
{
    class Program
    {
        static void Test()
        {
        }

        static void Main(string[] args)
        {
            int c;
            var a = 5;
            var b = a + 2;
            Test();
            Console.WriteLine(""Hello World!"");
        }
    }
}";
        }

        public static string GetSimpleClassInstanceExample()
        {
            return @"using System;
            namespace Primjer
            {
                class Student 
                {
                }

                class Program
                {
                    static void Main(string[] args)
                    {
                        int c;
                        var s = new Student();
                        Console.WriteLine(""Hello World!"");
                    }
                }
            }";
        }

        public static string GetClassEmptyConstructorInstanceExample()
        {
            return GetFileContents("ClassEmptyConstructor.txt");
        }

        public static string GetClassInstanceExample()
        {
            return @"using System;
            namespace TraceSourceExample
            {
                class Student 
                {
                    public string FirstName {get;}
                    public Student(string firstName)
                    {
                        FirstName = firstName;
                    } 
                }

                class Program
                {
                    static void Main(string[] args)
                    {
                        int c;
                        var s = new Student(""Ante"");
                        Console.WriteLine(""Hello World!"");
                    }
                }
            }";
        }

        public static string GetPolymorphismExample()
        {
            return GetFileContents("Polymorphism.txt");
        }

        public static string GetPolymorphismOverrideExample()
        {
            return GetFileContents("PolymorphismOverride.txt");
        }

        public static string GetHelloWorldExample()
        {
            return GetFileContents("HelloWorld.txt");
        }

        public static string GetMultipleNamespacesExample()
        {
            return GetFileContents("MultipleNamespacesExample.txt");
        }

        public static string GetPropertiesExample()
        {
            return GetFileContents("Properties.txt");
        }

        public static string GetSimpleWithMethod()
        {
            return GetFileContents("SimpleWithMethod.txt");
        }

        public static string VoidMethodExample()
        {
            return GetFileContents("VoidMethodExample.txt");
        }
    }
}