import os
import shutil
import logging
from datetime import datetime

import numpy as np
import pandas as pd

from fastapi import APIRouter, HTTPException, UploadFile, File, Form
from sklearn.preprocessing import MinMaxScaler

from payload.requests.forecast_request import ForecastRequest
from payload.requests.predict_request import PredictRequest
from services.model_service import (
    load_and_prepare_data,
    train_model,
    load_model,
    forecast_periods,
    predict_next
)

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

router = APIRouter()

MODEL_DIR = 'models'
if not os.path.exists(MODEL_DIR):
    os.makedirs(MODEL_DIR)

@router.get("/")
def read_root():
    return {"message": "Welcome to the TensorFlow Exchange Rate Forecast API"}

@router.post("/train")
async def train_model_route(
    currency_r030: int = Form(...),
    file: UploadFile = File(...)
):
    logger.info(f"Received currency_r030: {currency_r030}, file: {file.filename}")
    file_location = f"temp_{file.filename}"

    try:
        with open(file_location, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)

        X, y, scaler, df = load_and_prepare_data(file_location, currency_r030)

        if len(X) == 0:
            raise HTTPException(status_code=404, detail="Not enough data to train the model.")

        model_path = os.path.join(MODEL_DIR, f"tf_model_{currency_r030}.h5")
        scaler_path = os.path.join(MODEL_DIR, f"scaler_{currency_r030}.npy")
        np.save(scaler_path, scaler.data_max_)  # Save the max value used in scaling

        train_model(X, y, model_path=model_path)

        os.remove(file_location)

        return {"message": f"Model trained and saved at {model_path}"}

    except Exception as e:
        logger.error(f"Training error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@router.post("/predict")
def predict(request: PredictRequest):
    try:
        model_path = os.path.join(MODEL_DIR, f"tf_model_{request.currency_r030}.h5")
        scaler_path = os.path.join(MODEL_DIR, f"scaler_{request.currency_r030}.npy")
        csv_path = os.path.join(MODEL_DIR, f"rates_{request.currency_r030}.csv")

        model = load_model(model_path)

        if not os.path.exists(scaler_path):
            raise HTTPException(status_code=404, detail="Scaler file not found.")
        data_max = np.load(scaler_path)
        scaler = MinMaxScaler()
        scaler.fit([[0], [data_max]])

        if not os.path.exists(csv_path):
            raise HTTPException(status_code=404, detail="Rates file not found.")

        df = pd.read_csv(csv_path)
        df['Date'] = pd.to_datetime(df['Date'], format='%d.%m.%Y')
        df = df[df['R030'] == request.currency_r030]
        df = df.sort_values('Date')
        df['Value'] = pd.to_numeric(df['Value'], errors='coerce')
        df = df.dropna(subset=['Value'])

        target_date = datetime.strptime(request.date, '%d.%m.%Y').date()
        history = df[df['Date'] < pd.Timestamp(target_date)]

        if len(history) < 30:
            raise HTTPException(status_code=400, detail="Not enough historical data before the target date.")

        last_30 = history['Value'].values[-30:]
        scaled_input = scaler.transform(last_30.reshape(-1, 1))
        X = scaled_input.reshape(1, 30, 1)

        prediction = model.predict(X)
        predicted_value = scaler.inverse_transform(prediction)[0][0]

        return {
            "currency_r030": request.currency_r030,
            "date": request.date,
            "predicted_rate": round(float(predicted_value), 4)
        }

    except FileNotFoundError as fnf:
        raise HTTPException(status_code=404, detail=str(fnf))
    except Exception as e:
        logger.error(f"Prediction error: {e}")
        raise HTTPException(status_code=500, detail=str(e))


@router.post("/forecast")
def forecast(request: ForecastRequest):
    try:
        model_path = os.path.join(MODEL_DIR, f"tf_model_{request.currency_r030}.h5")
        scaler_path = os.path.join(MODEL_DIR, f"scaler_{request.currency_r030}.npy")

        model = load_model(model_path)

        if not os.path.exists(scaler_path):
            raise HTTPException(status_code=404, detail="Scaler file not found.")
        data_max = np.load(scaler_path)
        from sklearn.preprocessing import MinMaxScaler
        scaler = MinMaxScaler()
        scaler.fit([[0], [data_max]])

        # Потрібно мати останнє вікно значень для прогнозу
        raise HTTPException(status_code=501, detail="Last 30 values required for forecast. Not implemented.")

    except FileNotFoundError as fnf:
        raise HTTPException(status_code=404, detail=str(fnf))
    except Exception as e:
        logger.error(f"Forecast error: {e}")
        raise HTTPException(status_code=500, detail=str(e))
