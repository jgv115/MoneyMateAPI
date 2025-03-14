from flask import Flask

app = Flask(__name__)

@app.route("/v1/places/<path:placeId>")
def get_place_details(placeId):
    return {
        "formattedAddress": "1 Hello Street Vic Australia 3123",
        "id": "place_id_123"
    }