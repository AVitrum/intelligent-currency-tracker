import os
import shutil
import logging

from fastapi import APIRouter, HTTPException, UploadFile, File, Form
from payload.requests.forecast_request import ForecastRequest
from payload.requests.predict_request import PredictRequest
from services.model_service import (
    load_and_prepare_data,
    train_lstm_model,
    save_model,
    load_model,
    predict_rate,
    forecast_periods
)


logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

router = APIRouter()

MODEL_DIR = 'models'
if not os.path.exists(MODEL_DIR):
    os.makedirs(MODEL_DIR)
    logger.info(f"Created model directory: {MODEL_DIR}")


@router.get("/")
def read_root():
    return {"message": "Welcome to the Exchange Rate Forecast API (LSTM Version)"}

@router.post("/train")
async def train_model(
        currency_r030: int = Form(...),
        file: UploadFile = File(...) 
):
    logger.info(f"Received training request for currency_r030: {currency_r030} with file: {file.filename}")

    file_location = f"temp_{file.filename}"
    try:
        with open(file_location, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)
        logger.info(f"Temporary file saved at: {file_location}")

        X_train, y_train, scaler, last_sequence, last_date = load_and_prepare_data(
            file_location, currency_r030=currency_r030
        )

        if X_train.size == 0 or y_train.size == 0 or scaler is None or last_sequence is None or last_date is None:
            logger.error(f"Failed to load or prepare data for currency {currency_r030}. Not enough data or other issue.")
            raise HTTPException(status_code=404, detail=f"Not enough data or error preparing data for currency {currency_r030}.")

        logger.info(f"Data prepared for currency {currency_r030}. X_train shape: {X_train.shape}, y_train shape: {y_train.shape}")

        lstm_model = train_lstm_model(X_train, y_train)

        model_artifact_path = save_model(lstm_model, scaler, last_sequence, last_date, currency_r030)

        return {"message": f"LSTM Model trained and artifacts saved for currency {currency_r030}. Main model at {model_artifact_path}"}

    except HTTPException as http_exc:
        raise http_exc
    except ValueError as ve: 
        logger.error(f"ValueError during training for {currency_r030}: {ve}")
        raise HTTPException(status_code=400, detail=str(ve))
    except Exception as e:
        logger.error(f"Unexpected error during training for {currency_r030}: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"An unexpected error occurred during training: {str(e)}")
    finally:
        if os.path.exists(file_location):
            os.remove(file_location)
            logger.info(f"Temporary file {file_location} removed.")


@router.post("/predict")
def predict(request: PredictRequest):
    logger.info(f"Received prediction request: {request}")
    try:
        lstm_model, scaler, last_train_seq, training_end_date = load_model(request.currency_r030)

        result = predict_rate(lstm_model, scaler, last_train_seq, training_end_date, request.date)
        return {"prediction": result}
    except FileNotFoundError as fnf:
        logger.error(f"Model not found for prediction: {fnf}")
        raise HTTPException(status_code=404, detail=str(fnf))
    except ValueError as ve: 
        logger.error(f"ValueError during prediction: {ve}")
        raise HTTPException(status_code=400, detail=str(ve))
    except Exception as e:
        logger.error(f"Error during prediction: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/forecast")
def forecast(request: ForecastRequest):
    logger.info(f"Received forecast request: {request}")
    try:
        lstm_model, scaler, last_train_seq, training_end_date = load_model(request.currency_r030)
        
        result = forecast_periods(lstm_model, scaler, last_train_seq, training_end_date, request.periods)
        return {"forecast": result}
    except FileNotFoundError as fnf:
        logger.error(f"Model not found for forecast: {fnf}")
        raise HTTPException(status_code=404, detail=str(fnf))
    except ValueError as ve: # Catch specific errors
        logger.error(f"ValueError during forecast: {ve}")
        raise HTTPException(status_code=400, detail=str(ve))
    except Exception as e:
        logger.error(f"Error during forecast: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=str(e))