using System.Net.WebSockets;

namespace ResetService.Services
{
    public interface IWebSocketHandler
    {
        public Task<bool> ConnectWebSocket();
        public Task SendMessage(dynamic data);
        Task SendWebSocketMessage(ClientWebSocket webSocket, string message);
        Task<string> ReceiveWebSocketMessage(ClientWebSocket webSocket);
        string BuildSoapXmlContent(string message);
    }
}

