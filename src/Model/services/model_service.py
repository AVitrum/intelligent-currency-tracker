import pandas as pd
import numpy as np
from sklearn.preprocessing import MinMaxScaler
import tensorflow as tf
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import LSTM, Dense, Input, Dropout
from tensorflow.keras.callbacks import EarlyStopping
import os
import joblib
import logging

os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'


logger = logging.getLogger(__name__)

MODEL_DIR = 'models'
SEQUENCE_LENGTH = 60

if not os.path.exists(MODEL_DIR):
    os.makedirs(MODEL_DIR)

def _create_sequences(data, sequence_length):
    X, y = [], []
    for i in range(len(data) - sequence_length):
        X.append(data[i:(i + sequence_length)])
        y.append(data[i + sequence_length])
    return np.array(X), np.array(y)

def load_and_prepare_data(filepath, currency_r030=None):
    try:
        df = pd.read_csv(filepath, delimiter=',')
        df['Date'] = pd.to_datetime(df['Date'], format='%d.%m.%Y')
        df.sort_values('Date', inplace=True)

        if currency_r030 is None:
            raise ValueError("currency_r030 must be provided for LSTM model processing.")

        df_currency = df[df['R030'] == currency_r030].copy()

        if df_currency.empty:
            logger.warning(f"No data found for currency {currency_r030} in {filepath}")
            return np.array([]), np.array([]), None, None, None

        df_currency['Value'] = pd.to_numeric(df_currency['Value'], errors='coerce')
        df_currency.dropna(subset=['Value'], inplace=True)

        if len(df_currency) < SEQUENCE_LENGTH + 1:
            logger.warning(f"Not enough data for currency {currency_r030} to form sequences (need > {SEQUENCE_LENGTH} data points). Found {len(df_currency)}.")
            return np.array([]), np.array([]), None, None, None

        values = df_currency['Value'].values.reshape(-1, 1)
        scaler = MinMaxScaler(feature_range=(0, 1))
        scaled_values = scaler.fit_transform(values)

        X, y = _create_sequences(scaled_values, SEQUENCE_LENGTH)

        if X.shape[0] == 0:
            logger.warning(f"Could not create training sequences for currency {currency_r030}. Check data length and SEQUENCE_LENGTH.")
            return np.array([]), np.array([]), scaler, None, None

        X = X.reshape(X.shape[0], X.shape[1], 1)

        last_date_in_data = df_currency['Date'].iloc[-1]
        last_raw_sequence_scaled = scaled_values[-SEQUENCE_LENGTH:]
        last_sequence_for_prediction_input = last_raw_sequence_scaled.reshape(1, SEQUENCE_LENGTH, 1)

        return X, y, scaler, last_sequence_for_prediction_input, last_date_in_data
    except Exception as e:
        logger.error(f"Error in load_and_prepare_data for {currency_r030}: {e}")
        return np.array([]), np.array([]), None, None, None


def train_lstm_model(X_train, y_train):
    if X_train.size == 0 or y_train.size == 0:
        raise ValueError("Training data (X_train or y_train) is empty. Cannot train model.")

    model = Sequential([
        Input(shape=(X_train.shape[1], X_train.shape[2])),
        LSTM(50, return_sequences=True),
        Dropout(0.25),
        LSTM(50, return_sequences=False),
        Dropout(0.25),
        Dense(25),
        Dense(1)
    ])
    model.compile(optimizer='adam', loss='mean_squared_error')
    logger.info(f"Training LSTM model with X_train shape: {X_train.shape}, y_train shape: {y_train.shape}")

    early_stopping = EarlyStopping(monitor='val_loss', patience=8, restore_best_weights=True)

    model.fit(X_train, y_train, epochs=100, batch_size=32, verbose=1, validation_split=0.1, callbacks=[early_stopping])
    logger.info("LSTM model training complete.")
    return model

def save_model(model, scaler, last_sequence, last_date, currency_r030):
    model_path = os.path.join(MODEL_DIR, f'lstm_model_{currency_r030}.keras')
    scaler_path = os.path.join(MODEL_DIR, f'scaler_{currency_r030}.pkl')
    last_sequence_path = os.path.join(MODEL_DIR, f'last_sequence_{currency_r030}.npy')
    last_date_path = os.path.join(MODEL_DIR, f'last_date_{currency_r030}.txt')

    model.save(model_path)
    joblib.dump(scaler, scaler_path)
    np.save(last_sequence_path, last_sequence)
    with open(last_date_path, 'w') as f:
        f.write(last_date.strftime('%Y-%m-%d'))

    logger.info(f"LSTM model and artifacts for currency {currency_r030} saved. Model: {model_path}")
    return model_path

def load_model(currency_r030):
    model_path = os.path.join(MODEL_DIR, f'lstm_model_{currency_r030}.keras')
    scaler_path = os.path.join(MODEL_DIR, f'scaler_{currency_r030}.pkl')
    last_sequence_path = os.path.join(MODEL_DIR, f'last_sequence_{currency_r030}.npy')
    last_date_path = os.path.join(MODEL_DIR, f'last_date_{currency_r030}.txt')

    if not all(os.path.exists(p) for p in [model_path, scaler_path, last_sequence_path, last_date_path]):
        raise FileNotFoundError(f"One or more model files for currency {currency_r030} not found in {MODEL_DIR}.")

    model = tf.keras.models.load_model(model_path)
    scaler = joblib.load(scaler_path)
    last_sequence = np.load(last_sequence_path)
    with open(last_date_path, 'r') as f:
        last_date_str = f.read().strip()
    last_date_obj = pd.to_datetime(last_date_str)

    logger.info(f"LSTM model and artifacts for currency {currency_r030} loaded.")
    return model, scaler, last_sequence, last_date_obj

def predict_rate(model, scaler, last_known_sequence, training_end_date, target_date_str):
    target_date = pd.to_datetime(target_date_str, format='%d.%m.%Y')
    training_end_date_dt = pd.to_datetime(training_end_date)

    if target_date <= training_end_date_dt:
        raise ValueError(f"Target date ({target_date.date()}) must be after the model's training end date ({training_end_date_dt.date()}).")

    periods_to_forecast = (target_date - training_end_date_dt).days

    current_sequence = last_known_sequence.copy()

    for i in range(periods_to_forecast):
        predicted_scaled_value = model.predict(current_sequence, verbose=0)[0, 0]
        new_step = np.array([[predicted_scaled_value]]).reshape(1, 1, 1)
        current_sequence = np.append(current_sequence[:, 1:, :], new_step, axis=1)

    final_predicted_value_scaled = current_sequence[0, -1, 0]
    actual_prediction = scaler.inverse_transform(np.array([[final_predicted_value_scaled]]))

    return [{"ds": target_date.strftime('%Y-%m-%d %H:%M:%S'), "yhat": float(actual_prediction[0, 0])}]

def forecast_periods(model, scaler, last_known_sequence, training_end_date, num_periods: int):
    if num_periods <= 0:
        return []

    current_sequence = last_known_sequence.copy()
    predictions_list = []
    current_prediction_date = pd.to_datetime(training_end_date)

    for _ in range(num_periods):
        predicted_scaled_value = model.predict(current_sequence, verbose=0)[0, 0]
        actual_value = scaler.inverse_transform(np.array([[predicted_scaled_value]]))[0, 0]

        current_prediction_date += pd.Timedelta(days=1)
        predictions_list.append({"ds": current_prediction_date.strftime('%Y-%m-%d %H:%M:%S'), "yhat": float(actual_value)})

        new_step = np.array([[predicted_scaled_value]]).reshape(1, 1, 1)
        current_sequence = np.append(current_sequence[:, 1:, :], new_step, axis=1)

    return predictions_list