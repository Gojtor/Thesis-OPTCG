using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCGSim;
using TCGSim.CardResources;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.ServerCon
{
    public class LoginManager : MonoBehaviour
    {
        public static LoginManager Instance { get; private set; }
        public string serverUrl { get; private set; }

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

            TextAsset configText = Resources.Load<TextAsset>("server_config");
            if (configText != null)
            {
                ServerSettings config = JsonUtility.FromJson<ServerSettings>(configText.text);
                serverUrl = config.serverUrl;
            }
            else
            {
                serverUrl = "http://localhost:5000";
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterUser(string userName, string password)
        {
            var registerData = new
            {
                UserName = userName,
                Password = password,
                ConfirmPassword = password
            };

            string jsonData = JsonConvert.SerializeObject(registerData);
            Debug.Log(jsonData);
            StartCoroutine(SendRegisterRequest(jsonData));
        }

        private IEnumerator SendRegisterRequest(string jsonData)
        {
            using (UnityWebRequest www = new UnityWebRequest(serverUrl + "/api/Account/register", "POST"))
            {
                Debug.Log(serverUrl + "/api/Account/register");
                byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Registration successful: " + www.downloadHandler.text);
                    Menu.InvokeRegisterSuccess();
                }
                else
                {
                    Debug.Log("Registration failed: " + www.error);
                    Menu.InvokeRegisterFail();
                }
            }
        }

        public void LoginUser(string userName, string password)
        {
            var loginData = new
            {
                UserName = userName,
                Password = password
            };

            string jsonData = JsonConvert.SerializeObject(loginData);

            StartCoroutine(SendLoginRequest(jsonData));
        }

        private IEnumerator SendLoginRequest(string jsonData)
        {
            using (UnityWebRequest www = new UnityWebRequest(serverUrl + "/api/Account/login", "POST"))
            {
                Debug.Log(serverUrl + "/api/Account//login");
                byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Login successful: " + www.downloadHandler.text);
                    Menu.InvokeLoginSucces();
                }
                else
                {
                    Debug.Log("Login failed: " + www.error);
                    Menu.InvokeLoginFail();
                }
            }
        }

        public async Task<List<string>> GetUserDecks(string userName)
        {
            string url = serverUrl + "/api/Account/GetDeckJson?userName="+userName;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
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
                        return JsonConvert.DeserializeObject<List<string>>(jsonResponse);
                }
                return null;
            }
        }

        public async Task AddToUserDecks(string userName, string newDeckItem)
        {
            string url = serverUrl + "/api/Account/AddToDeckJson";

            var messageJson = new
            {
                UserName = userName,
                NewDeckItem = newDeckItem
            };

            string json = JsonConvert.SerializeObject(messageJson);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield(); 
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + request.error);
                }
            }
        }

        public async Task RemoveFromUserDeck(string userName, string deckName)
        {
            string url = serverUrl + "/api/Account/RemoveDeck";

            var messageJson = new
            {
                UserName = userName,
                DeckItemName = deckName
            };

            string json = JsonConvert.SerializeObject(messageJson);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + request.error);
                }
            }
        }

        public async Task AcceptFriendRequest(string senderName, string toName)
        {
            string url = serverUrl + "/api/Account/Friends/AcceptFriendRequest";

            var messageJson = new
            {
                SenderName = senderName,
                ToUserName = toName
            };

            string json = JsonConvert.SerializeObject(messageJson);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + request.error);
                }
            }
        }

        public async Task SendFriendRequest(string senderName, string toName)
        {
            string url = serverUrl + "/api/Account/Friends/AddFriend";

            var messageJson = new
            {
                SenderName = senderName,
                ToUserName = toName
            };

            string json = JsonConvert.SerializeObject(messageJson);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + request.error);
                }
            }
        }

        public async Task<List<string>> GetFriends(string userName)
        {
            string url = serverUrl + "/api/Account/Friends/GetFriends?userName=" + userName;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
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
                        return JsonConvert.DeserializeObject<List<string>>(jsonResponse);
                }
                return null;
            }
        }

        public async Task<List<string>> GetFriendRequest(string userName)
        {
            string url = serverUrl + "/api/Account/Friends/GetFriendRequest?userName=" + userName;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
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
                        return JsonConvert.DeserializeObject<List<string>>(jsonResponse);
                }
                return null;
            }
        }

        public async Task DeclineFriendRequest(string senderName, string toName)
        {
            string url = serverUrl + "/api/Account/Friends/DeclineFriendRequest";

            var messageJson = new
            {
                SenderName = senderName,
                ToUserName = toName
            };

            string json = JsonConvert.SerializeObject(messageJson);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(jsonBytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + request.error);
                }
            }
        }
    }
}
