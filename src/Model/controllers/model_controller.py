import os
import shutil
import logging
from fastapi import APIRouter, HTTPException, UploadFile, File, Form
from payload.requests.forecast_request import ForecastRequest
from payload.requests.predict_request import PredictRequest
from services.model_service import load_and_prepare_data, train_lstm_model, save_model, load_model, predict_rate, \
    forecast_periods

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)
router = APIRouter()
MODEL_DIR = 'models'
if not os.path.exists(MODEL_DIR):
    os.makedirs(MODEL_DIR)


@router.get("/")
def read_root():
    return {"message": "Welcome to the Exchange Rate Forecast API (LSTM Version)"}


@router.post("/train")
async def train_model(currency_r030: int = Form(...), file: UploadFile = File(...)):
    f = "temp_" + file.filename
    try:
        with open(f, "wb") as b:
            shutil.copyfileobj(file.file, b)
        X, y, s, ls, ld = load_and_prepare_data(f, currency_r030)
        if X.size == 0 or y.size == 0 or s is None or ls is None or ld is None:
            raise HTTPException(status_code=404, detail="Not enough data or error preparing data")
        m = train_lstm_model(X, y)
        pa = save_model(m, s, ls, ld, currency_r030)
        return {"message": "LSTM Model trained and artifacts saved at " + pa}
    except HTTPException as e:
        raise e
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        if os.path.exists(f):
            os.remove(f)


@router.post("/predict")
def predict(req: PredictRequest):
    try:
        m, s, ls, ld = load_model(req.currency_r030)
        r = predict_rate(m, s, ls, ld, req.date)
        return {"prediction": r}
    except FileNotFoundError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.post("/forecast")
def forecast(req: ForecastRequest):
    try:
        m, s, ls, ld = load_model(req.currency_r030)
        r = forecast_periods(m, s, ls, ld, req.periods)
        return {"forecast": r}
    except FileNotFoundError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
