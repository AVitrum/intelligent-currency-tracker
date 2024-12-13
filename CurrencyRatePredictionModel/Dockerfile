FROM python:3.12-slim
WORKDIR /app
COPY CurrencyRatePredictionModel/requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt gunicorn
COPY . .
EXPOSE 8050
CMD ["gunicorn", "-b", "0.0.0.0:8050", "CurrencyRatePredictionModel.app:app"]