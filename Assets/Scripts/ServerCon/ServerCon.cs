using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TCGSim.CardResources;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Net;
using Unity.VisualScripting;
using TCGSim.CardScripts;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Threading;

namespace TCGSim
{
    public class ServerCon : MonoBehaviour
    {
        public static ServerCon Instance { get; private set; }
        public string url { get; private set; }
        public string serverUrl { get; } = "http://localhost:5000";
        public string gameID { get; private set; }
        public string playerName { get; private set; }
        public bool firstTurnIsMine { get; private set; }
        private bool enemyFinishedWithStartingHand = false;


        private HubConnection connection;
        private TaskCompletionSource<bool> ConnectionTask;
        private TaskCompletionSource<bool> EnemyConnectionTask;
        private TaskCompletionSource<bool> AddingToGroupTask;
        private TaskCompletionSource<bool> GameGroupCreatedTask;

        // Start is called before the first frame update
        void Start()
        {
           
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public async Task ConnectToServer()
        {
            connection = new HubConnectionBuilder()
           .WithUrl(serverUrl + "/websocket")
           .Build();

            connection.On<string,string>("Connected", (message,enemyName) =>
            {
                Debug.Log(message+enemyName);
                if (ConnectionTask != null)
                {
                    ConnectionTask.TrySetResult(true);
                }
                EnemyBoard.Instance.SetName(enemyName);
            });

            connection.On<string,string>("EnemyConnected", (message,name) =>
            {
                Debug.Log(message);
                if (EnemyConnectionTask != null)
                {
                    EnemyConnectionTask.TrySetResult(true);
                }
                EnemyBoard.Instance.SetName(name);
                Debug.Log("Enemy name is: " + EnemyBoard.Instance.playerName);
            });

            connection.On<string>("ReceiveMessage", (message) =>
            {
                Debug.Log("Message received: " + message);
            });

            connection.On<string>("AddedToGroup", (message) =>
            {
                Debug.Log(message);
                if (AddingToGroupTask != null)
                {
                    AddingToGroupTask.TrySetResult(true);
                }
            });

            connection.On<string>("GameGroupCreated", (message) =>
            {
                Debug.Log(message);
                if (GameGroupCreatedTask != null)
                {
                    GameGroupCreatedTask.TrySetResult(true);
                }
            });

            connection.On<string>("GroupAlreadyExist", (message) =>
            {
                Debug.Log(message);
            });

            connection.On<string>("GroupDoesntExist", (message) =>
            {
                Debug.Log(message);
            });

            connection.On<string>("TwoPlayerAlreadyInGame", (message) =>
            {
                Debug.Log(message);
            });

            connection.On<string>("DoneWithStartingHand", async (message) =>
            {
                Debug.Log(message);
                await UpdateEnemyBoard();
            });

            connection.On<string>("UpdateEnemyBoard", async (message) =>
            {
                Debug.Log(message);
                await UpdateEnemyBoard();
            });

            connection.On<string>("UpdateThisEnemyCard", async (message) =>
            {
                Debug.Log(message);
                await UpdateEnemyCard(message);
            });

            connection.On<string>("WhoIsFirst", (message) =>
            {
                Debug.Log("The first player is :"+message);
                if (message == playerName)
                {
                    firstTurnIsMine = true;
                }
            });
            connection.On<string>("ChangeGameStateToPlayerPhase", (message) =>
            {
                Debug.Log(message);
                GameManager.Instance.ChangeGameState(GameState.PLAYERPHASE);
            });

            await connection.StartAsync();
            Debug.Log("WebSocket connection is succesfull!");
        }

        public void Init(string gameID,string playerName)
        {
            this.gameID = gameID;
            this.playerName = playerName;
        }

        public async Task WaitForConnection()
        {
            Debug.Log("Waiting for a connection...");
            await connection.InvokeAsync<string>("ConnectedAsEnemy", gameID,playerName);
            ConnectionTask = new TaskCompletionSource<bool>();
            await ConnectionTask.Task; // Wait here until a message is received
            Debug.Log("Connected");
        }

        public async Task WaitForEnemyToConnect()
        {
            Debug.Log("Waiting for enemy connection...");
            EnemyConnectionTask = new TaskCompletionSource<bool>();
            await EnemyConnectionTask.Task; // Wait here until a message is received
            Debug.Log("Enemy connected");
            await connection.InvokeAsync<string>("ReAssureEnemyConnected", gameID,playerName);
        }

        public async Task WaitForEnemyToFinishStartingHand()
        {
            Debug.Log("Waiting for enemy to finish starting hand");
            while (!enemyFinishedWithStartingHand)
            {
                await Task.Delay(1000);
            }
            Debug.Log("Enemy is done with starting hand!");
            if (firstTurnIsMine)
            {
                GameManager.Instance.ChangeGameState(GameState.PLAYERPHASE);
            }
            else
            {
                GameManager.Instance.ChangeGameState(GameState.ENEMYPHASE);
            }
            await Task.CompletedTask;
        }

        public async void SendMessageToServer(string message)
        {
            await connection.InvokeAsync<string>("ReceiveMessageFromClient", gameID,message);
        }

        public async Task AddPlayerToGroupInSocket(string gameID,string playerName)
        {
            Debug.Log("Adding player to the following group in socket: "+gameID);
            AddingToGroupTask = new TaskCompletionSource<bool>();
            await connection.InvokeAsync<string>("AddClientToGroupByGameID",gameID,playerName);
            await AddingToGroupTask.Task;
        }

        public async Task CreateGroupInSocket(string gameID, string playerName)
        {
            Debug.Log("Adding player to the following group in socket: " + gameID);
            GameGroupCreatedTask = new TaskCompletionSource<bool>();
            await connection.InvokeAsync<string>("CreateGroupByGameIDAndAddFirstClient", gameID, playerName);
            await GameGroupCreatedTask.Task;
            Debug.Log("Player added to group in socket");
        }

        public async Task DoneWithMulliganOrKeep()
        {
            await connection.InvokeAsync<string>("DoneWithMulliganOrKeep", gameID);
            await WaitForEnemyToFinishStartingHand();
        }
        public async Task UpdateMyBoardAtEnemy()
        {
            await connection.InvokeAsync<string>("UpdateMyBoardAtEnemy", gameID);
        }

        public async Task UpdateMyCardAtEnemy(string customCardID)
        {
            await connection.InvokeAsync<string>("UpdateMyCardAtEnemy", gameID,customCardID);
        }

        public async Task UpdateEnemyBoard()
        {
            Debug.Log("UpdateEnemyBoard called! Enemy name: "+EnemyBoard.Instance.playerName+"! My name: "+PlayerBoard.Instance.playerName);
            await EnemyBoard.Instance.UpdateBoardFromGameDB();
            enemyFinishedWithStartingHand = true;
        }

        public async Task UpdateEnemyCard(string customCardID)
        {
            Debug.Log("UpdateEnemyBoard called! Enemy name: " + EnemyBoard.Instance.playerName + "! My name: " + PlayerBoard.Instance.playerName);
            await EnemyBoard.Instance.UpdateCardFromGameDB(customCardID);
        }

        public async Task ChangeEnemyGameStateToPlayerPhase(string customCardID)
        {
            await connection.InvokeAsync<string>("ChangeEnemyGameStateToPlayerPhase", gameID);
        }

        private async void OnApplicationQuit()
        {
            await connection.StopAsync();
        }

        public IEnumerator GetRequest(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        string jsonResponse = webRequest.downloadHandler.text;
                        //Debug.Log(pages[page] + ":\nReceived: " + jsonResponse);
                        CardData card = JsonUtility.FromJson<CardData>(jsonResponse);
                        break;
                }
            }
        }
        public async Task<CardData> GetCardByCardID(string cardID)
        {
            string url = "http://localhost:5000/api/TCG/GetCardByCardID/";
            using (UnityWebRequest request = UnityWebRequest.Get(url + cardID))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(request.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(request.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        string jsonResponse = request.downloadHandler.text;
                        //Debug.Log("Received: " + jsonResponse);
                        return JsonConvert.DeserializeObject<CardData>(jsonResponse);
                }
                return null;
            }

        }
        
        public async Task AddCardToInGameStateDB(Card card)
        {
            CardData cardData = card.cardData;
            string url = "http://localhost:5000/api/TCG/SetCardToGameDB";
            string json = JsonConvert.SerializeObject(cardData);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone) // Wait until the request is done
                {
                    await Task.Yield(); // Let Unity handle the next frame
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + request.error);
                }
            }
        }

        public async Task UpdateCardAtInGameStateDB(Card card)
        {
            CardData cardData = card.cardData;
            string url = "http://localhost:5000/api/TCG/UpdateCardInGameDB";
            string json = JsonConvert.SerializeObject(cardData);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone) // Wait until the request is done
                {
                    await Task.Yield(); // Let Unity handle the next frame
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + request.error);
                }
            }
        }

        public async Task<List<CardData>> GetAllCardByGameID(string gameID)
        {
            string url = "http://localhost:5000/api/TCG/GetAllCardByFromGameDBByGameID?gameCustomID=";
            Debug.Log(url + gameID);
            using (UnityWebRequest request = UnityWebRequest.Get(url + gameID))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(request.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(request.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        string jsonResponse = request.downloadHandler.text;
                        //Debug.Log("Received: " + jsonResponse);
                        return JsonConvert.DeserializeObject<List<CardData>>(jsonResponse);

                }
                return null;
            }

        }
        public async Task<List<CardData>> GetAllCardByGameIDAndPlayerName(string gameID, string playerName)
        {
            string url = "http://localhost:5000/api/TCG/GetAllCardByFromGameDBByGameIDAndPlayer?";
            //Debug.Log(url + gameID);
            using (UnityWebRequest request = UnityWebRequest.Get(url + "gameCustomID=" + gameID + "&playerName=" + playerName))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(request.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(request.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        string jsonResponse = request.downloadHandler.text;
                        Debug.Log("Received: " + jsonResponse);
                        return JsonConvert.DeserializeObject<List<CardData>>(jsonResponse);

                }
                return null;
            }

        }

        public async Task<CardData> GetCardByFromGameDBByGameIDAndPlayerAndCustomCardID(string gameID, string playerName,string customCardID)
        {
            string url = "http://localhost:5000/api/TCG/GetCardByFromGameDBByGameIDAndPlayerAndCustomCardID?";
            Debug.Log(url + "gameCustomID=" + gameID + "&playerName=" + playerName + "&customCardID=" + customCardID);
            using (UnityWebRequest request = UnityWebRequest.Get(url + "gameCustomID=" + gameID + "&playerName=" + playerName+ "&customCardID="+customCardID))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(request.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(request.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        string jsonResponse = request.downloadHandler.text;
                        Debug.Log("Received: " + jsonResponse);
                        return JsonConvert.DeserializeObject<CardData>(jsonResponse);

                }
                return null;
            }

        }

    }
}
