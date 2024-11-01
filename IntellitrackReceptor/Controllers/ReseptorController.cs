using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using ServiceRfidTekni;

namespace IntellitrackReceptor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReseptorController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ReseptorController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost("SendReseptorRequest")]
        public async Task<IActionResult> SendReseptorRequest([FromBody] ReseptorRequest request)
        {
            var client = new Service1Client();
            var res =  await client.ReseptorAsync(request.Rfid,request.Evnt,request.Logn);
            return Ok(res);
        }
    }
}