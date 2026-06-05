namespace oedibud.Models;
public static class Helper
{
    public static string FormatPercent(decimal value, bool showPercentSign = false)
    {
        return showPercentSign ? $"{value:0.##} %" : $"{value:0.##}";
    }
}
