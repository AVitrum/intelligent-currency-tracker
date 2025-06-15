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


def _create_sequences(data, s):
    X = []
    y = []
    for i in range(len(data) - s):
        X.append(data[i:i + s])
        y.append(data[i + s])
    return np.array(X), np.array(y)


def load_and_prepare_data(f, c=None):
    try:
        d = pd.read_csv(f, delimiter=',')
        d['Date'] = pd.to_datetime(d['Date'], format='%d.%m.%Y')
        d.sort_values('Date', inplace=True)
        if c is None:
            raise ValueError()
        dc = d[d['R030'] == c].copy()
        if dc.empty:
            return np.array([]), np.array([]), None, None, None
        dc['Value'] = pd.to_numeric(dc['Value'], errors='coerce')
        dc.dropna(subset=['Value'], inplace=True)
        if len(dc) < SEQUENCE_LENGTH + 1:
            return np.array([]), np.array([]), None, None, None
        v = dc['Value'].values.reshape(-1, 1)
        s = MinMaxScaler((0, 1))
        sv = s.fit_transform(v)
        X, y = _create_sequences(sv, SEQUENCE_LENGTH)
        if X.shape[0] == 0:
            return np.array([]), np.array([]), s, None, None
        X = X.reshape(X.shape[0], X.shape[1], 1)
        ld = dc['Date'].iloc[-1]
        l = sv[-SEQUENCE_LENGTH:].reshape(1, SEQUENCE_LENGTH, 1)
        return X, y, s, l, ld
    except:
        return np.array([]), np.array([]), None, None, None


def train_lstm_model(X, y):
    if X.size == 0 or y.size == 0:
        raise ValueError()
    m = Sequential([Input(shape=(X.shape[1], X.shape[2])), LSTM(50, return_sequences=True), Dropout(0.25),
                    LSTM(50, return_sequences=False), Dropout(0.25), Dense(25), Dense(1)])
    m.compile(optimizer='adam', loss='mean_squared_error')
    e = EarlyStopping(monitor='val_loss', patience=8, restore_best_weights=True)
    m.fit(X, y, epochs=100, batch_size=32, verbose=1, validation_split=0.1, callbacks=[e])
    return m


def save_model(m, s, ls, ld, c):
    mp = os.path.join(MODEL_DIR, f'lstm_model_{c}.keras')
    sp = os.path.join(MODEL_DIR, f'scaler_{c}.pkl')
    lp = os.path.join(MODEL_DIR, f'last_sequence_{c}.npy')
    dp = os.path.join(MODEL_DIR, f'last_date_{c}.txt')
    m.save(mp)
    joblib.dump(s, sp)
    np.save(lp, ls)
    with open(dp, 'w') as f:
        f.write(ld.strftime('%Y-%m-%d'))
    return mp


def load_model(c):
    mp = os.path.join(MODEL_DIR, f'lstm_model_{c}.keras')
    sp = os.path.join(MODEL_DIR, f'scaler_{c}.pkl')
    lp = os.path.join(MODEL_DIR, f'last_sequence_{c}.npy')
    dp = os.path.join(MODEL_DIR, f'last_date_{c}.txt')
    if not all(os.path.exists(p) for p in [mp, sp, lp, dp]):
        raise FileNotFoundError()
    m = tf.keras.models.load_model(mp)
    sc = joblib.load(sp)
    ls = np.load(lp)
    with open(dp, 'r') as f:
        lds = f.read().strip()
    ldo = pd.to_datetime(lds)
    return m, sc, ls, ldo


def predict_rate(m, sq, l, td, ts):
    t = pd.to_datetime(ts, format='%d.%m.%Y')
    d = pd.to_datetime(td)
    if t <= d:
        raise ValueError()
    p = (t - d).days
    seq = l.copy()
    for i in range(p):
        r = []
        for j in range(10):
            r.append(m(seq, training=True).numpy()[0, 0])
        mv = np.mean(r)
        seq = np.append(seq[:, 1:, :], np.array([[[mv]]]), axis=1)
    mn = np.min(r)
    mx = np.max(r)
    mm = np.mean(r)
    f = sq.inverse_transform(np.array([[mm]]))[0, 0]
    fm = sq.inverse_transform(np.array([[mn]]))[0, 0]
    fx = sq.inverse_transform(np.array([[mx]]))[0, 0]
    return [{"ds": t.strftime('%Y-%m-%d %H:%M:%S'), "yhat": float(f), "yhat_min": float(fm), "yhat_max": float(fx)}]


def forecast_periods(m, sq, l, td, n):
    if n <= 0:
        return []
    seq = l.copy()
    r = []
    d = pd.to_datetime(td)
    for i in range(n):
        x = []
        for j in range(10):
            x.append(m(seq, training=True).numpy()[0, 0])
        mv = np.mean(x)
        seq = np.append(seq[:, 1:, :], np.array([[[mv]]]), axis=1)
        d += pd.Timedelta(days=1)
        mn = np.min(x)
        mx = np.max(x)
        fv = sq.inverse_transform(np.array([[mv]]))[0, 0]
        fm = sq.inverse_transform(np.array([[mn]]))[0, 0]
        fx = sq.inverse_transform(np.array([[mx]]))[0, 0]
        r.append(
            {"ds": d.strftime('%Y-%m-%d %H:%M:%S'), "yhat": float(fv), "yhat_min": float(fm), "yhat_max": float(fx)})
    return r
