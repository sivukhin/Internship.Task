import requests
import json

match = {
	"map": "TimeChanged!!!",
	"gameMode": "DM",
	"fragLimit": 20,
	"timeLimit": 20,
	"timeElapsed": 12.345678,
	"scoreboard": [
		{
			"name": "Player1",
			"frags": 20,
			"kills": 21,
			"deaths": 3
		},
		{
			"name": "Player2",
			"frags": 2,
			"kills": 2,
			"deaths": 21
		}
	]
}

server_data = {
	"name": "] My Worst!!! Server [",
	"gameModes": [ "DM", "TDM", "KEK", "PEK" ]
}

server = '100'
timestamp = '2017-01-22T15:18:00Z'

def put_query():
	return requests.put('http://127.0.0.1:12345/servers/{}/matches/{}'.format(server, timestamp), data=json.dumps(match))
	
def put_server():
	return requests.put('http://127.0.0.1:12345/servers/{}/info'.format(server), data=json.dumps(server_data))
# put_query()