from pydantic import BaseModel


class PredictRequest(BaseModel):
    currency_r030: int
    date: str