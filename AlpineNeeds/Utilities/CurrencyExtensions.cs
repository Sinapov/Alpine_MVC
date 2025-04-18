using System.Globalization;

namespace AlpineNeeds.Utilities;

public static class CurrencyExtensions
{
    public static string ToBgCurrency(this decimal amount)
    {
        return amount.ToString("C", new CultureInfo("bg-BG"));
    }
}