using ClosedXML.Excel;

namespace SubsCheck.Constants
{
    public static class Styles
    {
        public static IXLStyle None(IXLStyle style)
            => style;

        public static IXLStyle Unpaid(IXLStyle style)
            => style
                .Fill.SetBackgroundColor(XLColor.LightPink)
                .Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        public static IXLStyle NonMember(IXLStyle style)
            => style
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Font.SetFontColor(XLColor.LightGray);

        public static IXLStyle Note(IXLStyle style)
            => style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                .Font.SetFontColor(XLColor.Blue)
                .Font.SetItalic();

        public static IXLStyle ReadOnly(IXLStyle style)
            => style.Fill.SetBackgroundColor(XLColor.Orange);

        public static IXLStyle Editable(IXLStyle style)
            => style.Fill.SetBackgroundColor(XLColor.GrannySmithApple);

        public static IXLStyle DifferentAccount(IXLStyle style)
            => style.Font.SetFontColor(XLColor.Blue);

        public static IXLStyle IncorrectAllocationMediumRisk(IXLStyle style)
            => style.Font.SetFontColor(XLColor.Orange);

        public static IXLStyle IncorrectAllocationHighRisk(IXLStyle style)
            => style.Font.SetFontColor(XLColor.Red);
    }
}
