namespace Test
{
    public class Student
    {
        private readonly string _ime;
        private readonly string _prezime;
        private readonly int _godiste;

        public Student(string ime, string prezime, int godiste)
        {
            _ime = ime;
            _prezime = prezime;
            _godiste = godiste;
        }

        public int Godine()
        {
            return 2021 - _godiste;
        }
    }
}