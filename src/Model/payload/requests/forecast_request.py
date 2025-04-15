from pydantic import BaseModel


class ForecastRequest(BaseModel):
    currency_r030: int
    periods: int