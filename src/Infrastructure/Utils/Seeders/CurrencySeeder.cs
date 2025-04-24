using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Utils.Seeders;

public static class CurrencySeeder
{
    public static async Task SeedCurrenciesAsync(IApplicationBuilder applicationBuilder)
    {
        using IServiceScope serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        ApplicationDbContext dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        ICollection<Currency> currencies =
        [
            new Currency { R030 = 36, Name = "Australian Dollar", Code = "AUD" },
            new Currency { R030 = 124, Name = "Canadian Dollar", Code = "CAD" },
            new Currency { R030 = 156, Name = "Yuan Renminbi", Code = "CNY" },
            new Currency { R030 = 203, Name = "Czech Koruna", Code = "CZK" },
            new Currency { R030 = 208, Name = "Danish Krone", Code = "DKK" },
            new Currency { R030 = 344, Name = "Hong Kong Dollar", Code = "HKD" },
            new Currency { R030 = 348, Name = "Forint", Code = "HUF" },
            new Currency { R030 = 356, Name = "Indian Rupee", Code = "INR" },
            new Currency { R030 = 360, Name = "Rupiah", Code = "IDR" },
            new Currency { R030 = 376, Name = "New Israeli Shekel", Code = "ILS" },
            new Currency { R030 = 392, Name = "Yen", Code = "JPY" },
            new Currency { R030 = 398, Name = "Tenge", Code = "KZT" },
            new Currency { R030 = 410, Name = "Won", Code = "KRW" },
            new Currency { R030 = 484, Name = "Mexican Peso", Code = "MXN" },
            new Currency { R030 = 498, Name = "Moldovan Leu", Code = "MDL" },
            new Currency { R030 = 554, Name = "New Zealand Dollar", Code = "NZD" },
            new Currency { R030 = 578, Name = "Norwegian Krone", Code = "NOK" },
            new Currency { R030 = 643, Name = "Russian Ruble", Code = "RUB" },
            new Currency { R030 = 702, Name = "Singapore Dollar", Code = "SGD" },
            new Currency { R030 = 710, Name = "Rand", Code = "ZAR" },
            new Currency { R030 = 752, Name = "Swedish Krona", Code = "SEK" },
            new Currency { R030 = 756, Name = "Swiss Franc", Code = "CHF" },
            new Currency { R030 = 818, Name = "Egyptian Pound", Code = "EGP" },
            new Currency { R030 = 826, Name = "Pound Sterling", Code = "GBP" },
            new Currency { R030 = 840, Name = "US Dollar", Code = "USD" },
            new Currency { R030 = 933, Name = "Belarusian Ruble", Code = "BYN" },
            new Currency { R030 = 944, Name = "Azerbaijani Manat", Code = "AZN" },
            new Currency { R030 = 946, Name = "Romanian Leu", Code = "RON" },
            new Currency { R030 = 949, Name = "Turkish Lira", Code = "TRY" },
            new Currency { R030 = 960, Name = "SDR (Special Drawing Rights)", Code = "XDR" },
            new Currency { R030 = 975, Name = "Bulgarian Lev", Code = "BGN" },
            new Currency { R030 = 978, Name = "Euro", Code = "EUR" },
            new Currency { R030 = 985, Name = "Zloty", Code = "PLN" },
            new Currency { R030 = 12, Name = "Algerian Dinar", Code = "DZD" },
            new Currency { R030 = 50, Name = "Taka", Code = "BDT" },
            new Currency { R030 = 51, Name = "Armenian Dram", Code = "AMD" },
            new Currency { R030 = 214, Name = "Dominican Peso", Code = "DOP" },
            new Currency { R030 = 364, Name = "Iranian Rial", Code = "IRR" },
            new Currency { R030 = 368, Name = "Iraqi Dinar", Code = "IQD" },
            new Currency { R030 = 417, Name = "Som", Code = "KGS" },
            new Currency { R030 = 422, Name = "Lebanese Pound", Code = "LBP" },
            new Currency { R030 = 434, Name = "Libyan Dinar", Code = "LYD" },
            new Currency { R030 = 458, Name = "Malaysian Ringgit", Code = "MYR" },
            new Currency { R030 = 504, Name = "Moroccan Dirham", Code = "MAD" },
            new Currency { R030 = 586, Name = "Pakistani Rupee", Code = "PKR" },
            new Currency { R030 = 682, Name = "Saudi Riyal", Code = "SAR" },
            new Currency { R030 = 704, Name = "Dong", Code = "VND" },
            new Currency { R030 = 764, Name = "Baht", Code = "THB" },
            new Currency { R030 = 784, Name = "UAE Dirham", Code = "AED" },
            new Currency { R030 = 788, Name = "Tunisian Dinar", Code = "TND" },
            new Currency { R030 = 860, Name = "Uzbekistan Sum", Code = "UZS" },
            new Currency { R030 = 901, Name = "New Taiwan Dollar", Code = "TWD" },
            new Currency { R030 = 934, Name = "Turkmenistan New Manat", Code = "TMT" },
            new Currency { R030 = 941, Name = "Serbian Dinar", Code = "RSD" },
            new Currency { R030 = 972, Name = "Somoni", Code = "TJS" },
            new Currency { R030 = 981, Name = "Lari", Code = "GEL" },
            new Currency { R030 = 986, Name = "Brazilian Real", Code = "BRL" },
            new Currency { R030 = 959, Name = "Gold", Code = "XAU" },
            new Currency { R030 = 961, Name = "Silver", Code = "XAG" },
            new Currency { R030 = 962, Name = "Platinum", Code = "XPT" },
            new Currency { R030 = 964, Name = "Palladium", Code = "XPD" },

            new Currency { R030 = 31, Name = "Azerbaijani Manat (Old)", Code = "AZM" }, // Старий код для маната
            new Currency { R030 = 352, Name = "Iceland Krona", Code = "ISK" },
            new Currency { R030 = 428, Name = "Latvian Lats", Code = "LVL" },
            new Currency { R030 = 440, Name = "Lithuanian Litas", Code = "LTL" },
            new Currency { R030 = 792, Name = "Turkish Lira (Old)", Code = "TRL" }, // Старий код для ліри
            new Currency { R030 = 795, Name = "Turkmenistan Manat (Old)", Code = "TMM" }, // Старий код для маната
            new Currency
                { R030 = 974, Name = "Belarusian Ruble (Old)", Code = "BYR" }, // Старий код для білоруського рубля
            new Currency { R030 = 100, Name = "Bulgarian Lev (Old)", Code = "BGL" }, // Старий код для лева
            new Currency { R030 = 152, Name = "Chilean Peso", Code = "CLP" },
            new Currency { R030 = 191, Name = "Croatian Kuna", Code = "HRK" },
            new Currency { R030 = 414, Name = "Kuwaiti Dinar", Code = "KWD" },
            new Currency { R030 = 496, Name = "Mongolian Tugrik", Code = "MNT" },
            new Currency { R030 = 604, Name = "Peruvian Sol", Code = "PEN" },
            new Currency { R030 = 642, Name = "Romanian Leu (Old)", Code = "ROL" }, // Старий код для лея
            new Currency { R030 = 760, Name = "Syrian Pound", Code = "SYP" },
            new Currency { R030 = 952, Name = "West African CFA Franc", Code = "XOF" }
        ];

        foreach (Currency currency in currencies)
        {
            Currency? existingCurrency =
                await dbContext.Set<Currency>().FirstOrDefaultAsync(c => c.R030 == currency.R030);
            if (existingCurrency == null)
            {
                await dbContext.Set<Currency>().AddAsync(currency);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}