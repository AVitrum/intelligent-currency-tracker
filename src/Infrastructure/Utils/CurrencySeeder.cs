using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Utils;

public static class CurrencySeeder
{
    public static async Task SeedCurrenciesAsync(IApplicationBuilder applicationBuilder)
    {
        using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        ICollection<Currency> currencies =
        [
            new() { R030 = 36, Name = "Australian Dollar", Code = "AUD" },
            new() { R030 = 124, Name = "Canadian Dollar", Code = "CAD" },
            new() { R030 = 156, Name = "Yuan Renminbi", Code = "CNY" },
            new() { R030 = 203, Name = "Czech Koruna", Code = "CZK" },
            new() { R030 = 208, Name = "Danish Krone", Code = "DKK" },
            new() { R030 = 344, Name = "Hong Kong Dollar", Code = "HKD" },
            new() { R030 = 348, Name = "Forint", Code = "HUF" },
            new() { R030 = 356, Name = "Indian Rupee", Code = "INR" },
            new() { R030 = 360, Name = "Rupiah", Code = "IDR" },
            new() { R030 = 376, Name = "New Israeli Shekel", Code = "ILS" },
            new() { R030 = 392, Name = "Yen", Code = "JPY" },
            new() { R030 = 398, Name = "Tenge", Code = "KZT" },
            new() { R030 = 410, Name = "Won", Code = "KRW" },
            new() { R030 = 484, Name = "Mexican Peso", Code = "MXN" },
            new() { R030 = 498, Name = "Moldovan Leu", Code = "MDL" },
            new() { R030 = 554, Name = "New Zealand Dollar", Code = "NZD" },
            new() { R030 = 578, Name = "Norwegian Krone", Code = "NOK" },
            new() { R030 = 643, Name = "Russian Ruble", Code = "RUB" },
            new() { R030 = 702, Name = "Singapore Dollar", Code = "SGD" },
            new() { R030 = 710, Name = "Rand", Code = "ZAR" },
            new() { R030 = 752, Name = "Swedish Krona", Code = "SEK" },
            new() { R030 = 756, Name = "Swiss Franc", Code = "CHF" },
            new() { R030 = 818, Name = "Egyptian Pound", Code = "EGP" },
            new() { R030 = 826, Name = "Pound Sterling", Code = "GBP" },
            new() { R030 = 840, Name = "US Dollar", Code = "USD" },
            new() { R030 = 933, Name = "Belarusian Ruble", Code = "BYN" },
            new() { R030 = 944, Name = "Azerbaijani Manat", Code = "AZN" },
            new() { R030 = 946, Name = "Romanian Leu", Code = "RON" },
            new() { R030 = 949, Name = "Turkish Lira", Code = "TRY" },
            new() { R030 = 960, Name = "SDR (Special Drawing Rights)", Code = "XDR" },
            new() { R030 = 975, Name = "Bulgarian Lev", Code = "BGN" },
            new() { R030 = 978, Name = "Euro", Code = "EUR" },
            new() { R030 = 985, Name = "Zloty", Code = "PLN" },
            new() { R030 = 12, Name = "Algerian Dinar", Code = "DZD" },
            new() { R030 = 50, Name = "Taka", Code = "BDT" },
            new() { R030 = 51, Name = "Armenian Dram", Code = "AMD" },
            new() { R030 = 214, Name = "Dominican Peso", Code = "DOP" },
            new() { R030 = 364, Name = "Iranian Rial", Code = "IRR" },
            new() { R030 = 368, Name = "Iraqi Dinar", Code = "IQD" },
            new() { R030 = 417, Name = "Som", Code = "KGS" },
            new() { R030 = 422, Name = "Lebanese Pound", Code = "LBP" },
            new() { R030 = 434, Name = "Libyan Dinar", Code = "LYD" },
            new() { R030 = 458, Name = "Malaysian Ringgit", Code = "MYR" },
            new() { R030 = 504, Name = "Moroccan Dirham", Code = "MAD" },
            new() { R030 = 586, Name = "Pakistani Rupee", Code = "PKR" },
            new() { R030 = 682, Name = "Saudi Riyal", Code = "SAR" },
            new() { R030 = 704, Name = "Dong", Code = "VND" },
            new() { R030 = 764, Name = "Baht", Code = "THB" },
            new() { R030 = 784, Name = "UAE Dirham", Code = "AED" },
            new() { R030 = 788, Name = "Tunisian Dinar", Code = "TND" },
            new() { R030 = 860, Name = "Uzbekistan Sum", Code = "UZS" },
            new() { R030 = 901, Name = "New Taiwan Dollar", Code = "TWD" },
            new() { R030 = 934, Name = "Turkmenistan New Manat", Code = "TMT" },
            new() { R030 = 941, Name = "Serbian Dinar", Code = "RSD" },
            new() { R030 = 972, Name = "Somoni", Code = "TJS" },
            new() { R030 = 981, Name = "Lari", Code = "GEL" },
            new() { R030 = 986, Name = "Brazilian Real", Code = "BRL" },
            new() { R030 = 959, Name = "Gold", Code = "XAU" },
            new() { R030 = 961, Name = "Silver", Code = "XAG" },
            new() { R030 = 962, Name = "Platinum", Code = "XPT" },
            new() { R030 = 964, Name = "Palladium", Code = "XPD" },

            new() { R030 = 31, Name = "Azerbaijani Manat (Old)", Code = "AZM" }, // Старий код для маната
            new() { R030 = 352, Name = "Iceland Krona", Code = "ISK" },
            new() { R030 = 428, Name = "Latvian Lats", Code = "LVL" },
            new() { R030 = 440, Name = "Lithuanian Litas", Code = "LTL" },
            new() { R030 = 792, Name = "Turkish Lira (Old)", Code = "TRL" }, // Старий код для ліри
            new() { R030 = 795, Name = "Turkmenistan Manat (Old)", Code = "TMM" }, // Старий код для маната
            new() { R030 = 974, Name = "Belarusian Ruble (Old)", Code = "BYR" }, // Старий код для білоруського рубля
            new() { R030 = 100, Name = "Bulgarian Lev (Old)", Code = "BGL" }, // Старий код для лева
            new() { R030 = 152, Name = "Chilean Peso", Code = "CLP" },
            new() { R030 = 191, Name = "Croatian Kuna", Code = "HRK" },
            new() { R030 = 414, Name = "Kuwaiti Dinar", Code = "KWD" },
            new() { R030 = 496, Name = "Mongolian Tugrik", Code = "MNT" },
            new() { R030 = 604, Name = "Peruvian Sol", Code = "PEN" },
            new() { R030 = 642, Name = "Romanian Leu (Old)", Code = "ROL" }, // Старий код для лея
            new() { R030 = 760, Name = "Syrian Pound", Code = "SYP" },
            new() { R030 = 952, Name = "West African CFA Franc", Code = "XOF" }
        ];

        foreach (var currency in currencies)
        {
            var existingCurrency =
                await dbContext.Set<Currency>().FirstOrDefaultAsync(c => c.R030 == currency.R030);
            if (existingCurrency == null)
            {
                await dbContext.Set<Currency>().AddAsync(currency);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}