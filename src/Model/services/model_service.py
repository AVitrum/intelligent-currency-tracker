import pandas as pd
import numpy as np
import os
import tensorflow as tf
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import LSTM, Dense
from sklearn.preprocessing import MinMaxScaler


def load_and_prepare_data(filepath, currency_r030=None, look_back=30):
    df = pd.read_csv(filepath, delimiter=',')
    df['Date'] = pd.to_datetime(df['Date'], format='%d.%m.%Y')
    df.sort_values('Date', inplace=True)

    if currency_r030 is not None:
        df = df[df['R030'] == currency_r030]

    df['Value'] = pd.to_numeric(df['Value'], errors='coerce')
    df.dropna(subset=['Value'], inplace=True)

    values = df['Value'].values.reshape(-1, 1)

    scaler = MinMaxScaler(feature_range=(0, 1))
    values_scaled = scaler.fit_transform(values)

    X, y = [], []
    for i in range(look_back, len(values_scaled)):
        X.append(values_scaled[i - look_back:i])
        y.append(values_scaled[i])

    X, y = np.array(X), np.array(y)
    return X, y, scaler, df


def build_model(input_shape):
    model = Sequential([
        LSTM(64, return_sequences=False, input_shape=input_shape),
        Dense(1)
    ])
    model.compile(optimizer='adam', loss=tf.keras.losses.MeanSquaredError())
    return model


def train_model(X, y, model_path='models/tf_model.h5', epochs=20, batch_size=32):
    model = build_model((X.shape[1], X.shape[2]))

    model.fit(X, y, epochs=epochs, batch_size=batch_size, verbose=1)
    model.save(model_path)

    return model



def load_model(model_path='models/tf_model.h5'):
    if not os.path.exists(model_path):
        raise FileNotFoundError(f"Model file {model_path} does not exist.")
    return tf.keras.models.load_model(model_path, compile=False)



def predict_next(model, last_sequence, scaler):
    last_sequence = np.expand_dims(last_sequence, axis=0)
    pred_scaled = model.predict(last_sequence, verbose=0)
    pred = scaler.inverse_transform(pred_scaled)
    return float(pred[0][0])


def forecast_periods(model, initial_sequence, scaler, periods=7):
    sequence = initial_sequence.copy()
    forecast = []
    for _ in range(periods):
        next_val = predict_next(model, sequence, scaler)
        forecast.append(next_val)
        next_scaled = scaler.transform([[next_val]])
        sequence = np.vstack([sequence[1:], next_scaled])
    return forecast
