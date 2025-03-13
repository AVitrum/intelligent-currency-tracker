using Application.Common.Interfaces.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Utils
{
    public static class RateSeeder
    {
        public static async Task SeedRatesAsync(IApplicationBuilder applicationBuilder)
        {
            List<Rate> rates;

            using (IServiceScope scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                IRateRepository rateRepository = scope.ServiceProvider.GetRequiredService<IRateRepository>();
                rates = (List<Rate>)await rateRepository.GetAllAsync();
            }

            List<Rate> sortedRates = rates.OrderBy(r => r.Date).ThenBy(r => r.CurrencyId).ToList();
            List<Task> updateTasks = [];
            Dictionary<Guid, Rate> lastRateForCurrency = new();

            foreach (Rate rate in sortedRates)
            {
                if (lastRateForCurrency.TryGetValue(rate.CurrencyId, out Rate? previousRate))
                {
                    if (rate.ValueCompareToPrevious != 0 || sortedRates[0].Date.Date == rate.Date.Date)
                    {
                        continue;
                    }

                    if (Math.Abs((rate.Date.Date - previousRate.Date.Date).TotalDays - 1) < 1e-6)
                    {
                        rate.ValueCompareToPrevious = rate.Value - previousRate.Value;
                        
                        updateTasks.Add(Task.Run(async () =>
                        {
                            using (IServiceScope updateScope = applicationBuilder.ApplicationServices.CreateScope())
                            {
                                IRateRepository repo = updateScope.ServiceProvider.GetRequiredService<IRateRepository>();
                                await repo.UpdateAsync(rate);
                                Console.WriteLine($"Rate {rate.Currency} for {rate.Date:d} updated.");
                            }
                        }));
                    }
                }

                lastRateForCurrency[rate.CurrencyId] = rate;

                if (IsLastDayOfMonth(rate.Date))
                {
                    if (updateTasks.Count != 0)
                    {
                        await Task.WhenAll(updateTasks);
                        updateTasks.Clear();
                    }
                }
            }

            if (updateTasks.Count != 0)
            {
                await Task.WhenAll(updateTasks);
            }
        }

        private static bool IsLastDayOfMonth(DateTime date)
        {
            return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
        }
    }
}
