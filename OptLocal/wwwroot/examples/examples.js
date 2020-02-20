'use strict';

const Categories = {
    Debug: 'Debug',
    Microsoft: 'Microsoft',
    Professor: 'Professor'
};

const Examples = {
    methodsExample: {
        name: 'Methods example',
        categories: [Categories.Debug],
        code: `using System;

namespace TraceSourceExample
{
    class Program
    {
        static void Test()
        {
        }

        static void Test2()
        {
            var x = 5;
        }

        static int Test3(int x)
        {
            return x + 3;
        }

        static int Test4(int x)
        {
            x = x + 2;
            return x + 1;
        }

        static void Main(string[] args)
        {
            Test();
            Test2();
            Test3(3);
            Test4(4);
            Console.WriteLine("Hello World!");
        }
    }
}`
    },
    simpleExample: {
        name: 'Simple example',
        categories: [Categories.Debug],
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
            Console.WriteLine("Hello World!");
        }
    }
}`
    },
    simpleExampleWithMethod: {
        name: 'Simple example with method',
        categories: [Categories.Debug],
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
            Console.WriteLine("Hello World!");
        }
    }
}`
    },
    simpleClassInstanceExample: {
        name: 'Simple class instance example',
        categories: [Categories.Debug],
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
        categories: [Categories.Debug],
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
        classInstanceExample: {
            name: 'Class instance example',
            categories: [Categories.Debug],
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
    },
    polymorphism: {
        name: 'Polimorfizam',
        categories: [Categories.Debug],
        code: `using System;

namespace Polimorfizam
{
    public class Zivotinja
    {
        public void Glas()
        {
            Console.WriteLine("Zivotinja proizvodi glas.");
        }
    }
    
    public class Macka : Zivotinja
    {
        public void Glas()
        {
            Console.WriteLine("Mijau mijau");
        }
    }
    
    public class Pas : Zivotinja
    {
        public void Glas()
        {
            Console.WriteLine("Vau vau");
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Zivotinja mojaZivotinja = new Zivotinja();
            Zivotinja mojaMacka = new Macka();
            Zivotinja mojPas = new Pas();
            
            mojaZivotinja.Glas();
            mojaMacka.Glas();
            mojPas.Glas();
        }
    }
}`
    },
    property: {
        name: 'Primjer svojstva',
        categories: [Categories.Debug, Categories.Microsoft],
        code: `using System;

class TimePeriod
{
   private double _seconds;

   public double Hours
   {
       get { return _seconds / 3600; }
       set { 
          if (value < 0 || value > 24)
             throw new ArgumentOutOfRangeException(
                   $"{nameof(value)} must be between 0 and 24.");

          _seconds = value * 3600; 
       }
   }
}

class Program
{
   static void Main()
   {
       TimePeriod t = new TimePeriod();
       // The property assignment causes the 'set' accessor to be called.
       t.Hours = 24;

       // Retrieving the property causes the 'get' accessor to be called.
       Console.WriteLine($"Time in hours: {t.Hours}");
   }
}`
    }
};

window.Examples = Examples;
window.ExampleCategories = Categories;