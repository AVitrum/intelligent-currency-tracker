namespace Domain.Events;

public class AiModelUpdateEventArgs : EventArgs
{
    public AiModelUpdateEventArgs(List<int> r030)
    {
        R030 = r030;
    }

    public List<int> R030 { get; set; }
}

public delegate Task ExchangeRatesFetchedEventHandler(object sender, AiModelUpdateEventArgs e);