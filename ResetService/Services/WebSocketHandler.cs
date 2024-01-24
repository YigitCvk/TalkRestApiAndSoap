using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace ResetService.Services
{
    public class WebSocketHandler : IWebSocketHandler
    {
        private ClientWebSocket _webSocket;
        //Logging
        readonly ILogger<WebSocketHandler> _logger;
        private readonly IConfiguration _configuration;

        public WebSocketHandler(ILogger<WebSocketHandler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            bool res = ConnectWebSocket().Result;
        }
        public async Task<bool> ConnectWebSocket()
        {
            string webSocketUrl = "";
            try
            {
                if (_webSocket == null || _webSocket.State != WebSocketState.Open)
                {
                    _webSocket = new ClientWebSocket();

                    webSocketUrl = Environment.GetEnvironmentVariable("SOCKET_SERVER");
                    if (string.IsNullOrEmpty(webSocketUrl))
                    {
                        webSocketUrl = _configuration.GetSection("socketServer").Value;
                    }

                    await _webSocket.ConnectAsync(new Uri(webSocketUrl), CancellationToken.None);
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogError("WebSocket connection not established " + ex.Message + "Socket Host: " + webSocketUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError("WebSocket connection not established " + ex.Message);
            }

            return true;
        }
        public async Task SendMessage(dynamic data)
        {
            try
            {
                if (_webSocket.State == WebSocketState.Connecting || _webSocket.State == WebSocketState.Open)
                {
                    var message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                    await _webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
                    _logger.LogInformation("Message sent to Web Socket Server");
                }
                else
                {
                    _logger.LogError("Can not send message because websocket connection not established.Will be try to connection...");
                    await ConnectWebSocket();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Can not send message");
                _logger.LogError(ex.Message);
            }
        }

        public async Task SendWebSocketMessage(ClientWebSocket webSocket, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task<string> ReceiveWebSocketMessage(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }


        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
        {
            // Kullanıcı kimliğini almak için uygun bir yöntem kullanın (örneğin, bağlantı parametrelerinden, tokenlardan, vb.)
            var userId = GetUserId(context);

            _sockets.TryAdd(userId, webSocket);

            try
            {
                await ReceiveMessage(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // Gelen JSON mesajını işle
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        // SOAP servisine gönderme işlemleri burada yapılabilir
                        await ProcessSoapServiceMessage(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Bağlantıyı kapatma işlemleri burada yapılabilir
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                        _sockets.TryRemove(userId, out _);
                    }
                });
            }
            catch (Exception ex)
            {
                // Hata yönetimi
            }
        }

        private async Task ReceiveMessage(WebSocket webSocket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                handleMessage(result, buffer);
            }
        }

        private string GetUserId(HttpContext context)
        {
            // Kullanıcı kimliğini almak için uygun bir yöntem kullanın
            // Örneğin, context.User.Identity.Name veya token kullanabilirsiniz
            return "UserId123"; // Örnek bir kullanıcı kimliği
        }

        private async Task ProcessSoapServiceMessage(string message)
        {
            // SOAP servisi URL'sini alın
            string soapServiceUrl = GetSoapServiceUrl();

            // SOAP servisi için HTTP isteği oluşturun
            using (var httpClient = new HttpClient())
            {
                // SOAP isteği için gerekli başlıkları ve içerik tipini ayarlayın
                httpClient.DefaultRequestHeaders.Add("Content-Type", "text/xml");
                httpClient.DefaultRequestHeaders.Add("SOAPAction", "YourSOAPActionHeader");

                // SOAP servisi için XML içeriğini oluşturun
                string soapXmlContent = BuildSoapXmlContent(message);

                // HTTP POST isteğini gönderin
                var response = await httpClient.PostAsync(soapServiceUrl, new StringContent(soapXmlContent, Encoding.UTF8, "text/xml"));

                // HTTP yanıtını kontrol edin ve gerekirse işleyin
                if (response.IsSuccessStatusCode)
                {
                    // Yanıt başarılı ise burada işlem yapabilirsiniz
                    var soapResponse = await response.Content.ReadAsStringAsync();
                    // SOAP servisinin yanıtını işleyin
                }
                else
                {
                    // Yanıt başarısız ise burada işlem yapabilirsiniz
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    // Hata mesajını işleyin veya loglayın
                }
            }
        }
        private string GetSoapServiceUrl()
        {
            // SOAP servisinin URL'sini döndürün
            // Örnek: return "http://example.com/YourSoapService.asmx";
            return "https://localhost:44322/ServiceTest.asmx"; // Örneğin bir URL döndürüldü
        }

        public string BuildSoapXmlContent(string message)
        {
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:web=""http://www.example.com/"">
                  <soapenv:Header/>
                  <soapenv:Body>
                    <web:GreetUser>
                      <web:UserName>{message}</web:UserName>
                    </web:GreetUser>
                  </soapenv:Body>
                </soapenv:Envelope>";
        }
    }
}

