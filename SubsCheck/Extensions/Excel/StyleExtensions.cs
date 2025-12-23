using ClosedXML.Excel;

namespace SubsCheck.Extensions.Excel
{
    public static class StyleExtensions
    {
        public static IXLStyle ApplyStyle(this IXLStyle style, Func<IXLStyle, IXLStyle> styleAction)
            => styleAction(style);
    }
}
