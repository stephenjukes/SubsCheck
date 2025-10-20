namespace SubsCheck.Models
{
    public class Person
    {
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public override string ToString()
            => $"{LastName} {FirstName}";
    }
}
