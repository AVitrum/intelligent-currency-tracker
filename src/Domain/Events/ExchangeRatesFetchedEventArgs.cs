namespace Domain.Events;

public class ExchangeRatesFetchedEventArgs : EventArgs
{
    public ExchangeRatesFetchedEventArgs(List<int> r030)
    {
        R030 = r030;
    }

    public List<int> R030 { get; set; } = [];
}

public delegate Task ExchangeRatesFetchedEventHandler(object sender, ExchangeRatesFetchedEventArgs e);