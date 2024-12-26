import pandas as pd
import numpy as np
import torch

def predict_exchange_rate(model, scaler_X, scaler_y, currency_mapping, date, currency_code):
    new_date = pd.to_datetime(date, format='%d.%m.%Y')

    day = new_date.day
    month = new_date.month
    year = new_date.year

    sin_day = np.sin(2 * np.pi * day / 31)
    cos_day = np.cos(2 * np.pi * day / 31)
    sin_month = np.sin(2 * np.pi * month / 12)
    cos_month = np.cos(2 * np.pi * month / 12)

    currency_enum = currency_mapping.get(currency_code, -1)

    new_data = pd.DataFrame({
        'day': [day],
        'month': [month],
        'year': [year],
        'sin_day': [sin_day],
        'cos_day': [cos_day],
        'sin_month': [sin_month],
        'cos_month': [cos_month],
        'CurrencyEnum': [currency_enum],
    })

    new_data_scaled = scaler_X.transform(new_data.values)
    new_data_tensor = torch.tensor(new_data_scaled, dtype=torch.float32).unsqueeze(0)

    with torch.no_grad():
        prediction = model(new_data_tensor)
        prediction_denorm = scaler_y.inverse_transform(prediction.numpy())

    sale_rate_nb = prediction_denorm[0][0]
    purchase_rate_nb = prediction_denorm[0][1]

    return sale_rate_nb, purchase_rate_nb