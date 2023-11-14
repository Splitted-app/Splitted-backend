import numpy as np
import numpy.typing as npt
import pandas as pd
import re
from typing import Union, List
from collections import Counter, defaultdict
import itertools


def extract_tokens(data : pd.Series, merge_entries=True) -> Union[List[str], List[List[str]]]:
    tokens = []
    for entry in data.values:
        entry = re.sub(r"[\s]", " ", entry)
        entry = re.sub(r"[^A-Za-zÀ-ȕ ]", "", entry)
        entry_tokens = list(set(entry.split(' ')))
        if merge_entries:
            tokens.extend(entry_tokens)
        else:
            tokens.append(entry_tokens)
    return tokens

def calculate_token_correlation(data : pd.Series) -> tuple[npt.NDArray, dict, dict]:
    tokens = extract_tokens(data)
    unique_tokens = set(tokens)
    unique_token_count = len(unique_tokens)

    # create mappings between id and tokens and vice versa
    t2i = {token: i for i, token in enumerate(unique_tokens)}
    i2t = {i: token for i, token in enumerate(unique_tokens)}

    pair_occurances = np.zeros(shape=(unique_token_count, unique_token_count))
    entries_tokens = extract_tokens(data, merge_entries=False)

    for entry_tokens in entries_tokens:
        for t1, t2 in itertools.combinations(set(entry_tokens), r=2):
            pair_occurances[t2i[t1], t2i[t2]] += 1
            pair_occurances[t2i[t2], t2i[t1]] += 1

    token_occurances = Counter(tokens)
    for i in range(unique_token_count):
        pair_occurances[i,:] /= token_occurances[i2t[i]]

    return pair_occurances, i2t

def extract_phrases(M : npt.NDArray) -> tuple[dict, dict]:
    phrases = defaultdict(lambda : [])
    visited_idxs = set()
    for i in range(M.shape[0]):
        if i in visited_idxs:
            continue

        set.add(visited_idxs, i)
        phrases[i].append(i)
        for j in range(i+1, M.shape[0]):
            if M[i,j] == 1 and M[j,i] == 1:
                phrases[i].append(j)
                set.add(visited_idxs, j)
    return phrases

def reduce_phrases(M : npt.NDArray, i2t : dict) -> tuple[npt.NDArray, dict, dict, dict]:
    phrases = extract_phrases(M)
    n_tokens = M.shape[0]

    phrase_representative_idxs = list(phrases.keys())

    new_i2t = {i : i2t[i] for i in np.arange(n_tokens) if i in set(phrase_representative_idxs)}
    idx_map = {i : j for i, j in enumerate(phrase_representative_idxs)}

    return M[:,phrase_representative_idxs][phrase_representative_idxs,:], new_i2t, idx_map

def get_basic_subsets(M : npt.NDArray) -> npt.NDArray:
    is_superset = np.any(M == 1, axis=0)
    is_subset = np.any(M == 1, axis=1)

    is_basic_subset = (is_subset & ~is_superset)
    basic_subset_idx = np.where(is_basic_subset)[0]

    return basic_subset_idx

def get_frequencies(tokens : List[str]) -> dict:
    n_tokens = len(tokens)
    token_counts = Counter(tokens)
    for k, v in token_counts.items():
        token_counts[k] = v/n_tokens
    return token_counts

def extract_keywords(data : pd.Series, top_n : int = 1_000_000) -> List[str]:
    M, i2t = calculate_token_correlation(data)
    M, i2t, idx_map = reduce_phrases(M, i2t)
    basic_components = get_basic_subsets(M)
    freq = get_frequencies(extract_tokens(data))

    # keywords =  [i2t[idx_map[i]] for i in basic_components if freq[i2t[idx_map[i]]] > 0.001]
    keywords = [i2t[idx_map[i]] for i in basic_components]
    keywords.sort(key=lambda t : -freq[t])
    return keywords[:top_n]



def main():
    df = pd.read_csv('./data/data.csv', decimal=',', thousands=" ").drop(columns=['dayOfWeek'])
    data = df['description']
    keywords = extract_keywords(data)
    print(keywords)

if __name__ == '__main__':
    main()

