import torch
import torch.nn as nn

class CurrencyPredictor(nn.Module):
    def __init__(self, input_size, hidden_size, output_size, num_layers, dropout_rate):
        super(CurrencyPredictor, self).__init__()
        self.lstm = nn.LSTM(input_size, hidden_size, num_layers, batch_first=True, dropout=dropout_rate)
        self.fc = nn.Linear(hidden_size, output_size)

    def forward(self, x):
        out, _ = self.lstm(x)
        out = self.fc(out[:, -1, :])
        return out

def train_model(model, train_loader, criterion, optimizer, epochs, early_stop_patience):
    best_loss = float('inf')
    patience_counter = 0

    for epoch in range(epochs):
        model.train()
        total_loss = 0
        for inputs, targets in train_loader:
            optimizer.zero_grad()

            # Forward pass - remove unsqueeze(1) if not using sequential data
            outputs = model(inputs)

            # Ensure target and output shapes match
            if outputs.shape != targets.shape:
                print(f"Output shape {outputs.shape} doesn't match target shape {targets.shape}")
                continue

            loss = criterion(outputs, targets)
            loss.backward()
            optimizer.step()
            total_loss += loss.item()

        # Evaluate on training set (or validation set if available)
        model.eval()
        with torch.no_grad():
            val_loss = 0
            for inputs, targets in train_loader:  # If no validation data, use the same loader
                outputs = model(inputs)
                val_loss += criterion(outputs, targets).item()

        avg_train_loss = total_loss / len(train_loader)
        avg_val_loss = val_loss / len(train_loader)

        if avg_val_loss < best_loss:
            best_loss = avg_val_loss
            patience_counter = 0
            torch.save(model.state_dict(), 'best_model.pth')
        else:
            patience_counter += 1

        if patience_counter >= early_stop_patience:
            print(f"Early stopping at epoch {epoch + 1}")
            break

        if (epoch + 1) % 10 == 0:
            print(f'Epoch [{epoch + 1}/{epochs}], Train Loss: {avg_train_loss:.4f}, Val Loss: {avg_val_loss:.4f}')
