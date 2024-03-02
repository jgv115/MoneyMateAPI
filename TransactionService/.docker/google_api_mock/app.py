from flask import Flask

app = Flask(__name__)

@app.route("/maps/api/place/details/json")
def get_place_details():
    return {
        "result": {
            "formatted_address": "1 Hello Street Vic Australia 3123",
            "place_id": "place_id_123"
        }
    }