using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TCGSim.CardResources;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEditor.PackageManager;
using Newtonsoft.Json;
using System.Net;
using Unity.VisualScripting;

namespace TCGSim
{
    public class ServerCon : MonoBehaviour
    {
        public string serverApiUrl { get; } = "http://localhost:5000/api/TCG/";
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        public IEnumerator GetRequest(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
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
                        Debug.Log(pages[page] + ":\nReceived: " + jsonResponse);
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
                        Debug.Log("Received: " + jsonResponse);
                        return JsonConvert.DeserializeObject<CardData>(jsonResponse);
                }
                return null;
            }

        }
    }
}
