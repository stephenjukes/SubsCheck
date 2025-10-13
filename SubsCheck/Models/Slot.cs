namespace SubsCheck.Models
{
    public class Slot
    {
        public DateTime Date { get; set; }

        public Transaction Sub { get; set; }
    }
}
