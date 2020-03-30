'use strict';

const Categories = {
    Debug: 'Debug',
    Microsoft: 'Microsoft',
    Professor: 'Professor',
    OOP: 'OOP',
    P2: 'P2'
};

const Examples = {
    Vj02_1: {
        name: 'Primjer preopterećene metode',
        categories: [Categories.Professor, Categories.Debug, Categories.OOP],
        code: `using System;

namespace Vj02
{
    class Pozicija
    {
        public int x;
        public int y;

        public Pozicija(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    class Sprite
    {
        private int x;
        private int y;
        private string smjer;

        public Sprite(int posX, int posY)
        {
            x = posX;
            y = posY;
        }

        public void PomakNa(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void PomakNa(int x, int y, string smjer)
        {
            this.x = x;
            this.y = y;
            this.smjer = smjer;
        }

        public void PomakNa(Pozicija poz)
        {
            this.x = poz.x;
            this.y = poz.y;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Sprite sprite = new Sprite(0, 0);
            
            sprite.PomakNa(0, 0);
            sprite.PomakNa(1, 1, "Gore");
            
            Pozicija poz = new Pozicija(2, 2);
            sprite.PomakNa(poz);
        }
    }
}`
    },
    Vj02_2: {
        name: 'Primjer params',
        categories: [Categories.Professor, Categories.Debug, Categories.OOP],
        code: `using System;

namespace Vj02
{
    class Pozicija
    {
        public int x;
        public int y;

        public Pozicija(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    class Sprite
    {
        private int x;
        private int y;
        private string smjer;

        public Sprite(int posX, int posY)
        {
            x = posX;
            y = posY;
        }
        
        public void PomakNa(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void PomakNa(params Pozicija[] pozicije)
        {
            foreach (Pozicija pozicija in pozicije)
            {
                PomakNa(pozicija.x, pozicija.y);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Sprite sprite = new Sprite(0, 0);
            
            Pozicija poz1 = new Pozicija(1, 1);
            Pozicija poz2 = new Pozicija(2, 3);
            Pozicija poz3 = new Pozicija(3, 4);
            sprite.PomakNa(poz1, poz2, poz3);
        }
    }
}`
    },
    Vj03_01: {
        name: 'Primjer enkapsulacija',
        categories: [Categories.Debug, Categories.Professor, Categories.OOP],
        code: `using System;

namespace Vj03
{
    class Student
    {
        private int godine;

        public int Godine
        {
            get { return godine; }
            set
            {
                if (value <= 0)
                    godine = 1;
                else
                    godine = value;
            }
        }

        public Student(int godine)
        {
            this.Godine = godine;
        }
    }

    class Program
    {
        static void Main()
        {
            Student student1 = new Student(-1);
            Console.WriteLine(student1.Godine);
            Student student2 = new Student(15);
            Console.WriteLine(student2.Godine);
        }
    }
}`
    },
    Vj03_02: {
        name: 'Primjer static klasa',
        categories: [Categories.Debug, Categories.Professor, Categories.OOP],
        code: `using System;

namespace Vj03
{
    static class Postavke
    {
        public static int VelicinaSvijeta = 0;
        public static int SirinaSvijeta = 0;
        public static string Naziv = "";
    }

    class Program
    {
        static void Main()
        {
            Postavke.Naziv = "Primjer static klase";
            Postavke.VelicinaSvijeta = 150;
            Postavke.SirinaSvijeta = 420;
        }
    }
}`
    },
    Vj04_01:{
        name: 'Pikado (svojstva)',
        categories: [Categories.Debug, Categories.Professor, Categories.OOP],
        code: `using System;

namespace Vj04
{
    class Pikado
    {
        private int bodovi;

        public int Bodovi
        {
            get { return bodovi; }
            set
            {
                if (value >= 0)
                {
                    bodovi = value;
                }
            }
        }

        public Pikado(int bodovi)
        {
            Bodovi = bodovi;
        }


        public void UmanjiZa(int bod)
        {
            Bodovi = Bodovi - bod;
        }

        public void UmanjiZa(params int[] bodovi)
        {
            foreach (int bod in bodovi)
            {
                Bodovi = Bodovi - bod;
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Pikado pikado = new Pikado(180);
            // Cilj igre je spustiti broj bodova na pikadu od 180 do 0. S time da, ukoliko igrač
            // pogodi više bodova od potrebnog broja da se spusti točno na 0, pikado resetira vrijednost
            // na onu prije bacanja strelice.
            pikado.UmanjiZa(50, 50, 60, 60);
        }
    }
}`
    },
    Vj04_02:{
        name: 'Sat (svojstva)',
        categories: [Categories.Debug, Categories.Professor, Categories.OOP],
        code: `using System;

namespace Vj04
{
    class Sat
    {
        private int sati;

        public int Sati
        {
            get { return sati; }
            set
            {
                if (value > 24)
                {
                    int s = value - 24;
                    Sati = s;
                }
                else
                {
                    sati = value;
                }
            }
        }

        private int minute;

        public int Minute
        {
            get { return minute; }
            set
            {
                if (value > 60)
                {
                    int s = value / 60;
                    PomakniSate(s);
                    Minute = value - 60 * s;
                }
                else
                {
                    minute = value;
                }
            }
        }

        public void PomakniSate(int sati)
        {
            Sati = Sati + sati;
        }

        public void PomakniMinute(int minute)
        {
            Minute = Minute + minute;
        }
    }

    class Program
    {
        static void Main()
        {
            Sat s = new Sat();
            s.PomakniSate(27);
            s.PomakniMinute(125);
            Console.WriteLine(s.Sati + " sata i " + s.Minute + " minuta");
        }
    }
}`
    },
    P2Vj_021: {
        name: 'Primjer slijed',
        categories: [Categories.Debug, Categories.Professor, Categories.P2],
        code: `using System;

namespace Vj02
{
    class Program
    {
        static void Main(string[] args)
        {
            string ime;
            
            Console.WriteLine("Kako se zoveš?");
            ime = Console.ReadLine();
            Console.WriteLine(ime);
        }
    }
}`
    },
    P2Vj_022: {
        name: 'Primjer parsiranje (int)',
        categories: [Categories.Debug, Categories.Professor, Categories.P2],
        code: `using System;

namespace Vj02
{
    class Program
    {
        static void Main(string[] args)
        {
            string s;
            int broj;
            
            Console.WriteLine("Unesi broj?");
            s = Console.ReadLine();
            broj = int.Parse(s);
            Console.WriteLine(broj);
        }
    }
}`
    },
    P2Vj_031: {
        name: 'Primjer for (parni brojevi)',
        categories: [Categories.Debug, Categories.Professor, Categories.P2],
        code: `using System;

namespace Vj03
{
    class Program
    {
        static void Main()
        {
            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                {
                    Console.WriteLine(i);
                }
            }
        }
    }
}`
    },
    P2Vj_032: {
        name: 'Primer while (suma znamenaka)',
        categories: [Categories.Debug, Categories.Professor, Categories.P2],
        code: `using System;

namespace Vj03
{
    class Program
    {
        static void Main()
        {
            int br = 123;
            int suma = 0;
            while (br > 0)
            {
                int znam = br % 10;
                suma = suma + znam;
                br = br / 10;
            }
            Console.WriteLine(suma);
        }
    }
}`
    },
    P2Vj_033: {
      
        name: 'Do-while primjer (unesi dok)',
        categories: [Categories.Debug, Categories.Professor, Categories.P2],
        code: `using System;

namespace Vj03
{
    class Program
    {
        static void Main()
        {
            int broj;
            do
            {
                Console.WriteLine("Unesi pozitivan broj: ");
                broj = int.Parse(Console.ReadLine());
            } while (broj <= 0);
            Console.WriteLine("Unesen broj: {0}", broj);
        }
    }
}`
    },
    ifElse: {
        name: 'If-else primjer',
        categories: [Categories.Debug],
        code: `using System;

class Program
{
    static void Main()
    {
        int m = 15;
        int n = 12;
        if (m > 10)
        {
            if (n > 20)
                Console.WriteLine("Result1");
        }
        else if(m > 5)
        {
        }
        else
        {
            Console.WriteLine("Result2");
        }
    }
}`
    },
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
    instanceFirst: {
        name: 'Primjer instanciranja',
        categories: [Categories.Debug, Categories.Microsoft],
        code:
            `using System;

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
}`
    },
    instanceSecond: {
        name: 'Primjer instanciranja 2',
        categories: [Categories.Debug, Categories.Microsoft],
        code: `using System;
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
    },
    inheritance: {
        name: 'Primjer nasljeđivanje',
        categories: [Categories.Debug, Categories.Microsoft],
        code: `using System;

public class A 
{
   private int value = 10;

   public class B : A
   {
       public int GetValue()
       {
           return this.value;
       }     
   }
}

public class C : A
{
//    public int GetValue()
//    {
//        return this.value;
//    }
}

public class Example
{
    public static void Main(string[] args)
    {
        var b = new A.B();
        Console.WriteLine(b.GetValue());
    }
}
// The example displays the following output:
//       10`
    }
};

window.Examples = Examples;
window.Categories = Categories;