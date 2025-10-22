namespace SubsCheck.Models.IO.Input
{
    public class MemberInput
    {
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MotherLastName { get; set; }

        public string MotherFirstName { get; set; }

        public string FatherLastName { get; set; }

        public string FatherFirstName { get; set; }

        public DateOnly Start { get; set; }

        public DateOnly? End { get; set; }

        public bool? CheckSplitWordsOnly { get; set; }
    }
}
