import pandas as pd
import os
from datetime import datetime
from sklearn.model_selection import train_test_split

from transaction_classifier import TransactionClassifier

model_path = f'../AIService/AIFiles/Models/model_'

def convert_to_python_list(transactions_list):
    converted_list = list(map(map_transaction, transactions_list))
    return pd.DataFrame(converted_list, columns=['date', 'description', 'bankCategory', 'amount', 'currency', 'userCategory'])

def map_transaction(transaction):
    return [transaction.Date, transaction.Description, transaction.BankCategory, float(str(transaction.Amount).replace(",", ".")), transaction.Currency, transaction.UserCategory]

def read_data(transactions_list):
    df = convert_to_python_list(transactions_list)
    X = df.drop(columns='userCategory')
    y = df['userCategory'].fillna("None")
    return X,y

def train_model(transactions_list, user_id):
    classifier = TransactionClassifier()
    X_train, y_train = read_data(transactions_list)
    classifier.fit(X_train, y_train)
    classifier.save(f"{model_path}{user_id}.pickle")

def predict_categories(transactions_list, user_id):
    model_exists = os.path.isfile(f"{model_path}{user_id}.pickle")
    print(model_exists)
    if model_exists is True:
        classifier = TransactionClassifier.load(f"{model_path}{user_id}.pickle")
    else:
        return None
    
    X,_ = read_data(transactions_list)
    return classifier.predict(X, return_as_strings=True)