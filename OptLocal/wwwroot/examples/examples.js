'use strict';

const Examples = {
    simpleExample: {
        name: 'Simple example',
        code: `using System;

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
}`
    },
    simpleExampleWithMethod: {
        name: 'Simple example with method',
        code: `using System;

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
}`
    },
    simpleClassInstanceExample: {
        name:'Simple class instance example',
        code: `using System;
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
            Console.WriteLine("Hello World!");
        }
    }
}`
    },
    simpleClassWithEmptyConstructor: {
        name: 'Simple class with empty constructor',
        code: `using System;
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
            Console.WriteLine("Hello World!");
        }
    }
}`,
        classInstanceExample:{
            name: 'Class instance example',
            code: `using System;
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
            var s = new Student("Ante");
            Console.WriteLine("Hello World!");
        }
    }
}`
        }
    }
};

window.Examples = Examples;