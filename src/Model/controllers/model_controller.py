from fastapi import APIRouter, HTTPException
from payload.requests.forecast_request import ForecastRequest
from payload.requests.predict_request import PredictRequest
from payload.requests.train_request import TrainRequest
import os

from services.model_service import load_and_prepare_data, train_prophet_model, save_model, load_model, predict_rate, \
    forecast_periods

router = APIRouter()

CSV_FILE = 'rates.csv'
MODEL_DIR = '../models'
if not os.path.exists(MODEL_DIR):
    os.makedirs(MODEL_DIR)

@router.get("/")
def read_root():
    return {"message": "Welcome to the Exchange Rate Forecast API"}

@router.post("/train")
def train_model(request: TrainRequest):
    try:
        df_prophet = load_and_prepare_data(CSV_FILE, currency_r030=request.currency_r030)
        if df_prophet.empty:
            raise HTTPException(status_code=404, detail="No data found for the specified currency.")
        model = train_prophet_model(df_prophet)
        model_file = save_model(model, request.currency_r030)
        return {"message": f"Model trained and saved for currency {request.currency_r030} at {model_file}"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/predict")
def predict(request: PredictRequest):
    try:
        model = load_model(request.currency_r030)
        result = predict_rate(model, request.date)
        return {"prediction": result}
    except FileNotFoundError as fnf:
        raise HTTPException(status_code=404, detail=str(fnf))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/forecast")
def forecast(request: ForecastRequest):
    try:
        model = load_model(request.currency_r030)
        result = forecast_periods(model, request.periods)
        return {"forecast": result}
    except FileNotFoundError as fnf:
        raise HTTPException(status_code=404, detail=str(fnf))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
