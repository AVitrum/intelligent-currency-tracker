import pandas as pd
import numpy as np
from sklearn.preprocessing import StandardScaler
from sklearn.model_selection import train_test_split

def load_and_preprocess_data(df):
    df['Date'] = pd.to_datetime(df['Date'], format='%d.%m.%Y')
    df['day'] = df['Date'].dt.day
    df['month'] = df['Date'].dt.month
    df['year'] = df['Date'].dt.year

    df['sin_day'] = np.sin(2 * np.pi * df['day'] / 31)
    df['cos_day'] = np.cos(2 * np.pi * df['day'] / 31)
    df['sin_month'] = np.sin(2 * np.pi * df['month'] / 12)
    df['cos_month'] = np.cos(2 * np.pi * df['month'] / 12)

    df.drop(columns=['Date'], inplace=True)

    currency_mapping = {currency: idx for idx, currency in enumerate(df['Currency'].unique())}
    df['CurrencyEnum'] = df['Currency'].map(currency_mapping)
    df.drop(columns=['Currency'], inplace=True)

    X = df.drop(columns=['SaleRateNb', 'PurchaseRateNb', 'SaleRate', 'PurchaseRate']).values
    y = df[['SaleRateNb', 'PurchaseRateNb']].values

    scaler_X = StandardScaler()
    scaler_y = StandardScaler()

    X_scaled = scaler_X.fit_transform(X)
    y_scaled = scaler_y.fit_transform(y)

    X_train, X_test, y_train, y_test = train_test_split(X_scaled, y_scaled, test_size=0.2, random_state=42)

    return X_train, X_test, y_train, y_test, scaler_X, scaler_y, currency_mapping