import os
import shutil
import logging

from fastapi import APIRouter, HTTPException, UploadFile, File, Form
from payload.requests.forecast_request import ForecastRequest
from payload.requests.predict_request import PredictRequest
from services.model_service import load_and_prepare_data, train_prophet_model, save_model, load_model, predict_rate, \
    forecast_periods


logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

router = APIRouter()

CSV_FILE = 'rates.csv'
MODEL_DIR = '../models'
if not os.path.exists(MODEL_DIR):
    os.makedirs(MODEL_DIR)

@router.get("/")
def read_root():
    return {"message": "Welcome to the Exchange Rate Forecast API"}

@router.post("/train")
async def train_model(
        currency_r030: int = Form(...),
        file: UploadFile = File(None)
):
    logger.info(f"Received currency_r030: {currency_r030}")
    if file:
        logger.info(f"Received file: {file.filename}")
    else:
        logger.error("No file provided in the request.")

    try:
        if file:
            file_location = f"temp_{file.filename}"
            with open(file_location, "wb") as buffer:
                shutil.copyfileobj(file.file, buffer)
        else:
            raise HTTPException(status_code=404, detail="File not found")

        df_prophet = load_and_prepare_data(file_location, currency_r030)

        if df_prophet.empty:
            raise HTTPException(status_code=404, detail="No data found for the specified currency.")

        model = train_prophet_model(df_prophet)
        model_file = save_model(model, currency_r030)

        os.remove(file_location)

        return {"message": f"Model trained and saved for currency {currency_r030} at {model_file}"}

    except Exception as e:
        logger.error(f"Error during training: {e}")
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
