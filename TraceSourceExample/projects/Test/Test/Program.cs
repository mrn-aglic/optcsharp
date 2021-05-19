using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new Student("Marin", "Antic", 1990);
            var godine = s.Godine();
            
            Console.WriteLine($"Student ima {godine} godina.");
        }
    }
}