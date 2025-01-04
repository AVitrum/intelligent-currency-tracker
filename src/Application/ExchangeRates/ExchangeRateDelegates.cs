using Domain.Entities;

namespace Application.ExchangeRates;

public delegate ExchangeRate? ExchangeRateDelegate();
public delegate Task PostCreationDelegate(ExchangeRate rate);