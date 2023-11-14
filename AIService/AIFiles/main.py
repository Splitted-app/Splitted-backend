import pandas as pd
import os
from datetime import datetime
from sklearn.model_selection import train_test_split

from transaction_classifier import TransactionClassifier

model_path = '../AIService/AIFiles/Models/model.pickle'

def convert_to_python_list(transactions_list):
    converted_list = list(map(map_transaction, transactions_list))
    return pd.DataFrame(converted_list, columns=['date', 'description', 'bankCategory', 'amount', 'currency', 'userCategory'])

def map_transaction(transaction):
    return [transaction.Date, transaction.Description, transaction.BankCategory, float(str(transaction.Amount).replace(",", ".")), transaction.Currency, transaction.UserCategory]

def read_data(transactions_list):
    df = convert_to_python_list(transactions_list)
    X = df.drop(columns='userCategory')
    y = df['userCategory'].fillna("None")
    return train_test_split(X, y, test_size=0.2, stratify=y, random_state=0)

def train_model(transactions_list):
    classifier = TransactionClassifier()
    X_train, _, y_train, _ = read_data(transactions_list)
    classifier.fit(X_train, y_train)
    classifier.save(model_path)

def predict_categories():
    model_exists = os.path.isfile(model_path)
    if model_exists is True:
        classifier = TransactionClassifier.load(model_path)
    else:
        classifier = TransactionClassifier()
    
    _, X_test, _, _ = read_data()
    return classifier.predict(X_test, return_as_strings=True)