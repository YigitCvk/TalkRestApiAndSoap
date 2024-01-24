using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResetService.Services;

namespace RestService.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class WebSocketsController : Controller
    {
        private readonly IWebSocketHandler _webSocketHandler;
        public WebSocketsController(IWebSocketHandler webSocketHandler)
        {
            _webSocketHandler = webSocketHandler;
        }

        [HttpGet("ws/")]
        public async Task Get()
        {
            await _webSocketHandler.SendMessage("test");

            //if (HttpContext.WebSockets.IsWebSocketRequest)
            //{
            //}
            //else
            //{
            //    HttpContext.Response.StatusCode = 400;
            //}
        }

        [HttpGet("ws/isConnected")]
        public async Task<IActionResult> ConnectWebSocket([FromQuery] object lokiEvent)//kontrol et
        {
            try
            {
                // Örnek olay gönderme kodu
                var eventData = new
                {
                    eventMessage = lokiEvent
                };
                var eventDataConvert = JsonConvert.SerializeObject(eventData);

                bool isWebSocketConnected = await _webSocketHandler.ConnectWebSocket();

                // WebSocket bağlantısı zaten açıksa tekrar açmaya gerek yok
                if (isWebSocketConnected)
                {
                    await _webSocketHandler.SendMessage(eventDataConvert);
                    return Ok("WebSocket is already connected,and the event is sent.");
                }

                // WebSocket bağlantısı kapalıysa veya yoksa bağlantıyı başlatmaya çalış
                bool isConnected = await _webSocketHandler.ConnectWebSocket();
                if (isConnected)
                {
                    // SendMessage metodunu kullanarak olayı gönderin
                    await _webSocketHandler.SendMessage(eventDataConvert);
                    return Ok("WebSocket connection opened successfully, and the event is sent.");
                }
                else
                {
                    return BadRequest("Failed to open WebSocket connection.");
                }

            }
            catch (Exception ex)
            {
                // _logger.LogError($"Failed to initialise WebSocket connection: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

    }
}