from flask import Flask, request, jsonify
import torch
from torch.utils.data import DataLoader, TensorDataset
import pandas as pd
from data_processing import load_and_preprocess_data
from model import CurrencyPredictor, train_model
from predict import predict_exchange_rate
import io
import logging

app = Flask(__name__)
app.config['MAX_CONTENT_LENGTH'] = 64 * 1024 * 1024  # Increase the limit to 64MB

model = None
scaler_X = None
scaler_y = None
currency_mapping = None
MODEL_PATH = "models/best_model.pth"

logging.basicConfig(level=logging.DEBUG)

@app.route('/train-model/', methods=['POST'])
def train_model_endpoint():
    global model, scaler_X, scaler_y, currency_mapping

    if 'file' not in request.files:
        return jsonify({"error": "No file part"}), 400

    file = request.files['file']

    if file.filename == '':
        return jsonify({"error": "No selected file"}), 400

    if not file.filename.endswith('.csv'):
        return jsonify({"error": "Invalid file type. Only CSV files are accepted."}), 400

    try:
        data = file.read()
        df = pd.read_csv(io.StringIO(data.decode('utf-8')))

        X_train, X_test, y_train, y_test, scaler_X, scaler_y, currency_mapping = load_and_preprocess_data(df)

        X_train_tensor = torch.tensor(X_train, dtype=torch.float32)
        y_train_tensor = torch.tensor(y_train, dtype=torch.float32)
        X_test_tensor = torch.tensor(X_test, dtype=torch.float32)
        y_test_tensor = torch.tensor(y_test, dtype=torch.float32)

        train_dataset = TensorDataset(X_train_tensor, y_train_tensor)
        train_loader = DataLoader(train_dataset, batch_size=64, shuffle=True)

        test_dataset = TensorDataset(X_test_tensor, y_test_tensor)
        test_loader = DataLoader(test_dataset, batch_size=64, shuffle=False)

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

        train_model(model, train_loader, test_loader, criterion, optimizer, epochs, early_stop_patience)

        torch.save({
            'model_state_dict': model.state_dict(),
            'input_size': input_size,
            'hidden_size': hidden_size,
            'output_size': output_size,
            'num_layers': num_layers,
            'dropout_rate': dropout_rate
        }, MODEL_PATH)

        return jsonify({"message": "Model trained successfully"})
    except Exception as e:
        logging.error(f"Error during model training: {e}")
        return jsonify({"error": str(e)}), 500

@app.route('/debug-request', methods=['POST'])
def debug_request():
    return jsonify({"headers": dict(request.headers), "content_length": request.content_length})

@app.route('/predict/', methods=['GET'])
def predict_endpoint():
    global model, scaler_X, scaler_y, currency_mapping

    pre_date = request.args.get('pre_date')
    currency_code = request.args.get('currency_code')

    try:
        checkpoint = torch.load(MODEL_PATH, weights_only=True)
        input_size = checkpoint['input_size']
        hidden_size = checkpoint['hidden_size']
        output_size = checkpoint['output_size']
        num_layers = checkpoint['num_layers']
        dropout_rate = checkpoint['dropout_rate']

        model = CurrencyPredictor(input_size, hidden_size, output_size, num_layers, dropout_rate)
        model.load_state_dict(checkpoint['model_state_dict'])

        sale_rate_nb, purchase_rate_nb = predict_exchange_rate(model, scaler_X, scaler_y, currency_mapping, pre_date, currency_code)

        return jsonify({
            "Predicted Sale Rate (NB)": float(sale_rate_nb),
            "Predicted Purchase Rate (NB)": float(purchase_rate_nb)
        })
    except Exception as e:
        logging.error(f"Error during prediction: {e}")
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=8050)