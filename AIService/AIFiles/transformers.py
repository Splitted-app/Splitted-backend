from typing import Optional
from sklearn.base import BaseEstimator, TransformerMixin
from sklearn.preprocessing import OneHotEncoder
import pandas as pd
import numpy as np

from keyword_extractor import extract_keywords

class DateTransformer(BaseEstimator, TransformerMixin):

    def fit(self, X: pd.DataFrame, y=None):
        return self

    def transform(self, X: pd.DataFrame, y=None):
        X = X.copy()
        date = pd.to_datetime(X['date'])
        X.drop(columns=['date'], inplace=True)
        X['year'] = date.dt.year
        
        X['month'] = date.dt.month
        sin, cos = self.__sincos_encode(date.dt.month, 12)
        X = X.assign(month_sin = sin, month_cos = cos)

        X['dayOfYear'] = date.dt.day_of_year
        sin, cos = self.__sincos_encode(date.dt.day_of_year, 366)
        X = X.assign(dayOfYear_sin = sin, dayOfYear_cos = cos)

        X['dayOfMonth'] = date.dt.day
        sin, cos = self.__sincos_encode(date.dt.day, 31)
        X = X.assign(dayOfMonth_sin = sin, dayOfMonth_cos = cos)

        X['dayOfWeek'] = date.dt.day_of_week
        sin, cos = self.__sincos_encode(date.dt.day_of_week, 7)
        X = X.assign(dayOfWeek_sin = sin, dayOfMonth_cos = cos)

        return X
    
    def __sincos_encode(self, data : pd.Series, units_in_span : int) -> tuple[float, float]:
        sin = np.sin(2*np.pi*data/units_in_span)
        cos = np.cos(2*np.pi*data/units_in_span)

        return sin, cos


class CurrencyEncoder(BaseEstimator, TransformerMixin):

    def __init__(self, primary_currency: str = 'PLN') -> None:
        super().__init__()
        self.primary_currency = primary_currency

    def fit(self, X: pd.DataFrame, y=None):
        return self

    def transform(self, X: pd.DataFrame, y=None):
        X = X.copy()
        X['currency'] = (
            X['currency'] == self.primary_currency).astype(np.int32)
        return X


class BankCategoryEncoder(BaseEstimator, TransformerMixin):

    def __init__(self, categories=None) -> None:
        super().__init__()
        if categories is not None:
            raise Exception("Categories Not Implemented")
        self.categories = categories
        self.ohc = OneHotEncoder(
            sparse_output=False, max_categories=10, handle_unknown='ignore')

    def fit(self, X: pd.DataFrame, y=None):
        self.ohc.fit(X[['bankCategory']])
        return self

    def transform(self, X: pd.DataFrame, y=None):
        X = X.copy()
        X[self.ohc.get_feature_names_out(['bankCategory'])] = self.ohc.transform(
            X[['bankCategory']])
        return X.drop(columns=['bankCategory'])


class DescriptionEncoder(BaseEstimator, TransformerMixin):

    def __init__(self, keywords : Optional[list[str]] = None, top_n : int = 25) -> None:
        super().__init__()
        self.provided_keywords = keywords
        self.top_n = top_n

    def get_feature_names_out(self):
        if self.keywords is None:
            raise Exception("No keywords assigned")
        return [f'contains_"{kw}"' for kw in self.keywords]

    def fit(self, X: pd.DataFrame, y=None):
        if self.provided_keywords is None:
            self.keywords = extract_keywords(X['description'], top_n = self.top_n)
        else:
            self.keywords = self.provided_keywords
        return self

    def transform(self, X: pd.DataFrame, y=None):
        X = X.copy()
        M = np.zeros(shape=(len(X), len(self.keywords)))
        for i, row in enumerate(X['description']):
            for j, kw in enumerate(self.keywords):
                if kw in row:
                    M[i, j] = 1
        M_df = pd.DataFrame(columns=self.get_feature_names_out(), data=M)
        X.reset_index(drop=True, inplace=True)
        X = pd.concat([X, M_df], axis=1)
        return X.drop(columns=['description'])
        
