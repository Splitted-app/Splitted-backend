import os
import tempfile
from typing import Type, Union
import zipfile

import numpy.typing as npt
from sklearn.calibration import LabelEncoder
from sklearn.pipeline import Pipeline
from xgboost import XGBClassifier
import pickle

from transformers import *

class TransactionClassifier:
    __slots__ = ['preprocessor', 'xgboost', 'label_encoder']

    def __init__(self, 
                 description_keywords: Optional[list[str]] = None, 
                 primary_currency: str = "PLN",
                 model: Type[Pipeline] = None,
                 le: Type[LabelEncoder] = None) -> None:
        
        if (model is None) ^ (le is None):
            raise ValueError("Both model and label encoder have to passed or neither.")
        if model is not None:
            self.xgboost = model
            self.label_encoder = le
            return

        preprocessor = Pipeline(steps=[
            ('date_transformer', DateTransformer()),
            ('currency_enocoder', CurrencyEncoder(primary_currency=primary_currency)),
            ('bank_encoder', BankCategoryEncoder()),
            ('description_encoder', DescriptionEncoder(keywords=description_keywords, top_n=15))
        ])
        self.xgboost = Pipeline(
            steps=[
                ('preproc', preprocessor),
                ('xgboost_classifier', XGBClassifier(n_estimators=25, eval_metric='mlogloss'))
            ]
        )
        self.label_encoder = LabelEncoder()

    def fit(self, X : pd.DataFrame, y : pd.Series) -> None:
        y = self.label_encoder.fit_transform(y)
        self.xgboost.fit(X, y)
    
    def predict(self, X : pd.DataFrame, return_as_strings=False) -> Union[npt.NDArray[np.int32], npt.NDArray[np.object_]]:
        y_hat = self.xgboost.predict(X)
        if return_as_strings:
            y_hat = self.label_encoder.inverse_transform(y_hat)
        return y_hat
    
    def save(self, path):
        if not path.endswith('.pickle'):
            path += '.pickle'

        object_to_save = {
            'model': self.xgboost,
            'le': self.label_encoder
        }
        with open(path, 'wb') as file:
            pickle.dump(object_to_save, file)

    @classmethod
    def load(cls, path):
        if not path.endswith('.pickle'):
            path += '.pickle'

        with open(path, 'rb') as file:
            loaded_object = pickle.load(file)

        return TransactionClassifier(model=loaded_object['model'], le=loaded_object['le'])
        