from flask import Flask
import torch
from torch.utils.data import DataLoader, TensorDataset
import pandas as pd
from data_processing import load_and_preprocess_data
from model import CurrencyPredictor, train_model
from predict import predict_exchange_rate
import io
import logging
from confluent_kafka import Consumer, KafkaError
import json

app = Flask(__name__)

model = None
scaler_X = None
scaler_y = None
currency_mapping = None
MODEL_PATH = "models/best_model.pth"

logging.basicConfig(level=logging.DEBUG)

# Kafka configuration
KAFKA_BROKER = "localhost:9092"
TRAIN_TOPIC = "train-model"
PREDICT_TOPIC = "predict-model"

def kafka_consume(topic, group_id, process_function):
    consumer_conf = {
        'bootstrap.servers': KAFKA_BROKER,
        'group.id': group_id,
        'auto.offset.reset': 'earliest'
    }
    consumer = Consumer(consumer_conf)
    consumer.subscribe([topic])

    try:
        while True:
            msg = consumer.poll(1.0)
            if msg is None:
                continue
            if msg.error():
                if msg.error().code() == KafkaError._PARTITION_EOF:
                    continue
                else:
                    logging.error(f"Consumer error: {msg.error()}")
                    continue

            logging.info(f"Received message: {msg.value().decode('utf-8')}")
            process_function(json.loads(msg.value().decode('utf-8')))
    finally:
        consumer.close()


def process_training_message(message):
    global model, scaler_X, scaler_y, currency_mapping

    try:
        file_content = message.get('content', '')
        file_content = file_content.lstrip('\ufeff')
        df = pd.read_csv(io.StringIO(file_content))

        X_train, X_test, y_train, y_test, scaler_X, scaler_y, currency_mapping = load_and_preprocess_data(df)

        X_train_tensor = torch.tensor(X_train, dtype=torch.float32)
        y_train_tensor = torch.tensor(y_train, dtype=torch.float32)

        train_dataset = TensorDataset(X_train_tensor, y_train_tensor)
        train_loader = DataLoader(train_dataset, batch_size=64, shuffle=True)

        input_size = X_train.shape[1]
        hidden_size = 128
        output_size = y_train.shape[1]
        num_layers = 2
        dropout_rate = 0.3
        lr = 0.001
        epochs = 50
        early_stop_patience = 5

        model = CurrencyPredictor(input_size, hidden_size, output_size, num_layers, dropout_rate)
        criterion = torch.nn.MSELoss()
        optimizer = torch.optim.AdamW(model.parameters(), lr=lr)

        train_model(model, train_loader, None, criterion, optimizer, epochs, early_stop_patience)

        torch.save({
            'model_state_dict': model.state_dict(),
            'input_size': input_size,
            'hidden_size': hidden_size,
            'output_size': output_size,
            'num_layers': num_layers,
            'dropout_rate': dropout_rate
        }, MODEL_PATH)

        logging.info("Model trained and saved successfully.")
    except Exception as e:
        logging.error(f"Error during model training: {e}")


def process_prediction_message(message):
    global model, scaler_X, scaler_y, currency_mapping

    try:
        pre_date = message.get("pre_date")
        currency_code = message.get("currency_code")

        checkpoint = torch.load(MODEL_PATH)
        input_size = checkpoint['input_size']
        hidden_size = checkpoint['hidden_size']
        output_size = checkpoint['output_size']
        num_layers = checkpoint['num_layers']
        dropout_rate = checkpoint['dropout_rate']

        model = CurrencyPredictor(input_size, hidden_size, output_size, num_layers, dropout_rate)
        model.load_state_dict(checkpoint['model_state_dict'])

        sale_rate_nb, purchase_rate_nb = predict_exchange_rate(model, scaler_X, scaler_y, currency_mapping, pre_date, currency_code)

        logging.info({
            "Predicted Sale Rate (NB)": float(sale_rate_nb),
            "Predicted Purchase Rate (NB)": float(purchase_rate_nb)
        })
    except Exception as e:
        logging.error(f"Error during prediction: {e}")


if __name__ == '__main__':
    from threading import Thread

    # Start Kafka consumers in separate threads
    Thread(target=kafka_consume, args=(TRAIN_TOPIC, 'test-consumer-group', process_training_message)).start()
    Thread(target=kafka_consume, args=(PREDICT_TOPIC, 'test-consumer-group', process_prediction_message)).start()

    logging.info("Kafka consumers started. Listening for messages...")
    app.run(host='0.0.0.0', port=8050)
