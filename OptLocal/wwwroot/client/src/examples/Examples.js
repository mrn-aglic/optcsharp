'use strict';

const Categories = {
    Debug: 'Debug',
    Microsoft: 'Microsoft',
    Professor: 'Professor'
};

class Example {
    
    constructor(name, categories, code) {
        this.name = name;
        this.categories = categories;
        this.code = code;
    }
}

const examples = [
  new Example('Methods example', [Categories.Debug], `using System;

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
}`),
    new Example('Simple example', [Categories.Debug], `using System;

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
}`),
    new Example('Simple example with method', [Categories.Debug], `using System;

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
}`),
    new Example('Simple class instance example', [Categories.Debug], `using System;
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
}`),
    new Example('Simple class with empty constructor', [Categories.Debug], `using System;
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
}`),
    new Example('Polimorfizam (bez overridea)', [Categories.Debug], `using System;

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
}`),
    new Example('Primjer insanciranja 1', [Categories.Debug, Categories.Microsoft], `using System;

class Coords
{
    public int x, y;

    // Default constructor.
    public Coords()
    {
        x = 0;
        y = 0;
    }

    // A constructor with two arguments.
    public Coords(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    // Override the ToString method.
    public override string ToString()
    {
        return $"({x},{y})";
    }
}

class MainClass
{
    static void Main()
    {
        var p1 = new Coords();
        var p2 = new Coords(5, 3);

        // Display the results using the overriden ToString method.
        Console.WriteLine($"Coords #1 at {p1}");
        Console.WriteLine($"Coords #2 at {p2}");
        Console.ReadLine();
    }
}`),
    new Example('Primjer instanciranja 2', [Categories.Debug, Categories.Microsoft], `using System;
public class Person
{
    public int age;
    public string name;
}

class TestPerson
{
    static void Main()
    {
        var person = new Person();

        Console.WriteLine("Name: {person.name}, Age: {person.age}");
        // Keep the console window open in debug mode.
        Console.WriteLine("Press any key to exit.");
        Console.ReadLine();
    }
}`),
    new Example('Primjer svojstva', [Categories.Debug, Categories.Microsoft],`using System;

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
}`)
];

export default examples;