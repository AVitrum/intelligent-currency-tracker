import pandas as pd
from prophet import Prophet
from prophet.serialize import model_to_json, model_from_json
import os

def load_and_prepare_data(filepath, currency_r030=None):
    df = pd.read_csv(filepath, delimiter=',')
    df['Date'] = pd.to_datetime(df['Date'], format='%d.%m.%Y')
    df.sort_values('Date', inplace=True)

    if currency_r030 is not None:
        df = df[df['R030'] == currency_r030]

    df['Value'] = pd.to_numeric(df['Value'], errors='coerce')
    df.dropna(subset=['Value'], inplace=True)

    df_prophet = df[['Date', 'Value']].rename(columns={'Date': 'ds', 'Value': 'y'})
    return df_prophet

def train_prophet_model(df_prophet):
    model = Prophet(yearly_seasonality=True, weekly_seasonality=True, daily_seasonality=False)
    model.fit(df_prophet)
    return model

def save_model(model, currency_r030, filename=None):
    if filename is None:
        filename = os.path.join('models', f'prophet_model_{currency_r030}.json')
    with open(filename, 'w') as fout:
        fout.write(model_to_json(model))
    return filename

def load_model(currency_r030, filename=None):
    if filename is None:
        filename = os.path.join('models', f'prophet_model_{currency_r030}.json')
    if not os.path.exists(filename):
        raise FileNotFoundError(f"Model file {filename} does not exist.")
    with open(filename, 'r') as fin:
        model = model_from_json(fin.read())
    return model

def predict_rate(model, date_str):
    target_date = pd.to_datetime(date_str, format='%d.%m.%Y')
    last_date = model.history['ds'].max()
    days_diff = (target_date - last_date).days
    if days_diff <= 0:
        future = pd.DataFrame({'ds': [target_date]})
    else:
        future = model.make_future_dataframe(periods=days_diff)
        future = future[future['ds'] == target_date]
        if future.empty:
            future = pd.DataFrame({'ds': [target_date]})
    forecast = model.predict(future)
    result = forecast[['ds', 'yhat', 'yhat_lower', 'yhat_upper']].to_dict(orient='records')
    return result

def forecast_periods(model, periods: int):
    future = model.make_future_dataframe(periods=periods)
    forecast = model.predict(future)
    result = forecast[['ds', 'yhat', 'yhat_lower', 'yhat_upper']].to_dict(orient='records')
    return result
