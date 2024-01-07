def train_model(transactions_list, user_id):
    with open("../../../Data/FakeAIServiceData/model.pickle", "x") as file:
        file.write(user_id)

def predict_categories(transactions_list, user_id):
    if user_id == "invalid":
        return None
    else:
        return ["None", "Biedronka", "None", "Auchan", "Żabka", "None", "Praca"]