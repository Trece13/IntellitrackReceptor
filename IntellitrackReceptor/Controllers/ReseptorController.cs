using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using ServiceRfidTekni;
using System.Diagnostics;

namespace IntellitrackReceptor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReseptorController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _eventSource = "IntellitrackReceptorApp"; // Fuente de eventos
        private readonly string _logName = "Application"; // Log de aplicación (por defecto)

        public ReseptorController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (!EventLog.SourceExists(_eventSource))
            {
                EventLog.CreateEventSource(_eventSource, _logName);
            }
        }

        [HttpPost("SendReseptorRequest")]
        public async Task<IActionResult> SendReseptorRequest([FromBody] ReseptorRequest request)
        {
            try
            {
                var client = new Service1Client();
                var res = await client.ReseptorAsync(request.Rfid, request.Evnt, request.Logn);
                return Ok(res);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(_eventSource, "Resultado servicio rfid+++" + ex, EventLogEntryType.Information);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}