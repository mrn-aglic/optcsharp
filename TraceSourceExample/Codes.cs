using System;
using System.IO;

namespace TraceSourceExample
{
    public static class Codes
    {
        private static string GetFileContents(string filename)
        {
            var directory =
                AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal));
            var filePath = $"{directory}CodeExamples/{filename}";
            return File.ReadAllText(filePath);
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
        
        public static string GetSimpleExampleWithMethod()
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
            b = Test() + b;
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
            return @"using System;
            namespace TraceSourceExample
            {
                class Student 
                {
                    public Student()
                    {
                    } 
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
    }
}