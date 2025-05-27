namespace Domain.Events;

public class AlertSenderEventArgs : EventArgs
{
    public AlertSenderEventArgs(Guid currencyId, decimal percentDifference)
    {
        CurrencyId = currencyId;
        PercentDifference = percentDifference;
    }

    public Guid CurrencyId { get; set; }
    public decimal PercentDifference { get; set; }
}

public delegate Task AlertSenderEventHandler(object sender, AlertSenderEventArgs e);