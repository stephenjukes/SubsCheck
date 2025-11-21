namespace SubsCheck.Models.Excel
{
    public class ConditionalFormatParameters<T>
    {
        public ConditionalFormatParameters(string cause, T effect)
        {
            Cause = cause;
            Effect = effect;
        }

        public string Cause { get; set; }

        public T Effect { get; set; }
    }
}
