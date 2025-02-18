using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace TCGSim
{
    public class WebSocketClient : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {

        }

        private ClientWebSocket webSocket = new ClientWebSocket();
        private Uri serverUri = new Uri("ws://localhost:5000/gameHub");
        /*
        async void Start()
        {
            await Connect();
        }
        */

        public async Task Connect()
        {
            Debug.Log("Kapcsolódás a szerverhez!");
            await webSocket.ConnectAsync(serverUri, CancellationToken.None);
            Debug.Log("Kapcsolódva a szerverhez!");

            await ReceiveMessages();
        }

        async Task SendMessageToServer(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        async Task ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log("Üzenet érkezett a szervertõl: " + receivedMessage);
            }
        }

        private void OnDestroy()
        {
            webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
        }
    }
}
