from pydantic import BaseModel


class TrainRequest(BaseModel):
    currency_r030: int = None