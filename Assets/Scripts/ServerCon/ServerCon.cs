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
using UnityEditor.MemoryProfiler;

namespace TCGSim
{
    public class ServerCon : MonoBehaviour
    {
        public string url { get; private set; }
        public string serverUrl { get; } = "http://localhost:5000";
        public string gameID { get; private set; }
        public string playerName { get; private set; }

        private HubConnection connection;

        // Start is called before the first frame update
        void Start()
        {
           
        }

        // Update is called once per frame
        void Update()
        {

        }

        async void Awake()
        {
            connection = new HubConnectionBuilder()
           .WithUrl(serverUrl + "/websocket")
           .Build();

            connection.On<string>("ReceiveMessage", (message) =>
            {
                Debug.Log("Message received: " + message);
            });

            connection.On<string>("AddedToClient", (message) =>
            {
                Debug.Log("Message received: " + message);
            });

            await connection.StartAsync();
            Debug.Log("WebSocket connection is succesfull!");
            AddPlayerToGroupInSocket(this.gameID, this.playerName);
        }

        public void Init(string gameID,string playerName)
        {
            this.gameID = "TEST";
            this.playerName = playerName;
            
        }

        public async void SendMessageToServer(string message)
        {
            await connection.InvokeAsync<string>("ReceiveMessageFromClient", gameID,message);
        }

        public async void AddPlayerToGroupInSocket(string gameID,string playerName)
        {
            await connection.InvokeAsync<string>("AddClientToGroupByGameID",gameID,playerName);
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
        
        public IEnumerator AddCardToInGameStateDB(Card card)
        {
            CardData cardData = card.cardData;
            string url = "http://localhost:5000/api/TCG/SetCardToGameDB";
            string json = JsonConvert.SerializeObject(cardData);
            //Debug.Log("Sent JSON: " + json);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    //Debug.Log("Json sent successfully! Reply: " + request.downloadHandler.text);
                }
                else
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

    }
}
