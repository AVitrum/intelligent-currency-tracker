ARG PYTHON_VERSION=3.12
FROM python:${PYTHON_VERSION}-slim as base
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1
WORKDIR /app
COPY CurrencyRatePredictionModel/requirements.txt requirements.txt
RUN pip3 install -r requirements.txt
COPY CurrencyRatePredictionModel .
EXPOSE 8050
CMD [ "python3", "app.py" ]
