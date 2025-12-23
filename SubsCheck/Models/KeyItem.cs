using ClosedXML.Excel;

namespace SubsCheck.Models
{
    public class KeyItem
    {
        public KeyItem(string value, string description, Func<IXLStyle, IXLStyle> style)
        {
            Value = value;
            Description = description;
            Style = style;
        }

        public string Value { get; set; }

        public string Description { get; set; }

        public Func<IXLStyle, IXLStyle> Style { get; set; }
    }
}
