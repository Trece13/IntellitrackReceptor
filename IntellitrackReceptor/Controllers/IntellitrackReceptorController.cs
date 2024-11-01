using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using IntellitrackReceptor.Models;
using IntellitrackReceptor.Services;

namespace IntellitrackReceptor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntellitrackReceptorController : ControllerBase
    {
        private readonly ILogger<IntellitrackReceptorController> _logger;
        //private readonly IHttpClientFactory _httpClientFactory;
        private readonly IReseptorService _reseptorService;

        //public IntellitrackReceptorController(ILogger<IntellitrackReceptorController> logger, IHttpClientFactory httpClientFactory)
        public IntellitrackReceptorController(ILogger<IntellitrackReceptorController> logger, IReseptorService reseptorService)
        {
            _logger = logger;
            //_httpClientFactory = httpClientFactory;
            _reseptorService = reseptorService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleEvent([FromBody] JsonElement data)
        {
            try
            {
                   if (data.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement eventData in data.EnumerateArray())
                    {
                        if (eventData.TryGetProperty("eventType", out JsonElement eventTypeElement))
                        {
                            var eventType = eventTypeElement.GetString();

                            // Validación de suscripción
                            if (eventType == "Microsoft.EventGrid.SubscriptionValidationEvent")
                            {
                                if (eventData.TryGetProperty("data", out JsonElement dataElement) &&
                                    dataElement.TryGetProperty("validationCode", out JsonElement validationCodeElement))
                                {
                                    var validationCode = validationCodeElement.GetString();

                                    //var responseData = new JObject                            
                                    //{ "validationResponse", validationCode };

                                    _logger.LogInformation("Subscription validation event handled successfully");
                                    return Ok(new { validationResponse = validationCode });
                                    //return Ok(responseData);
                                }
                            }
                            // Manejo de otros eventos
                            else if (eventType == "Microsoft.Storage.BlobCreated")
                            {
                                if (eventData.TryGetProperty("data", out JsonElement dataElement) &&
                                    dataElement.TryGetProperty("url", out JsonElement blobUrlElement))
                                {
                                    var blobUrl = blobUrlElement.GetString();
                                    _logger.LogInformation($"Blob created event received. URL: {blobUrl}");
                                    // Procesa el evento del blob aquí...
                                }
                            }
                            else
                            {
                                eventData.TryGetProperty("data", out JsonElement dataElement);
                                var eventObject = System.Text.Json.JsonSerializer.Deserialize<EventData>(dataElement.ToString());
                                _logger.LogWarning($"Unhandled event type: {eventType}");

                                _logger.LogWarning($"Unhandled event type: {eventType}");

                                // Aquí hacemos la llamada a SendReseptorRequest
                                var reseptorRequest = new ReseptorRequest
                                {
                                    Rfid = eventObject.Data.IOTId,
                                    Evnt = eventObject.Data.DestinationLocation.FullName.ToString(),
                                    Logn = "Interface"
                                };

                                var result = await _reseptorService.SendReseptorRequest(reseptorRequest);

                            //    // Serializar el objeto ReseptorRequest a JSON
                            //    var jsonContent = System.Text.Json.JsonSerializer.Serialize(reseptorRequest);
                            //    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                                //    // URL del método SendReseptorRequest
                                //    var url = "https://gpt.tekni-plex.com:8092/IntellitrackReceptor/api/Reseptor/SendReseptorRequest"; // Cambia tu puerto

                                //    // Crear una instancia de HttpClient usando IHttpClientFactory
                                //    var httpClient = _httpClientFactory.CreateClient();

                                //    // Realizar la solicitud POST al método SendReseptorRequest
                                //    var response = await httpClient.PostAsync(url, content);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Event received with no eventType");
                        }
                    }
                }
                else
                {
                    return BadRequest("Expected an array of events.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the event");
                return StatusCode(500, "Internal server error");
            }
        }


    }

    //public class ReseptorRequest
    //{
    //    public string Rfid { get; set; }
    //    public string Evnt { get; set; }
    //    public string Logn { get; set; }
    //}

    public class AssetData
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("IsEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("IsArchived")]
        public bool IsArchived { get; set; }

        [JsonPropertyName("CreatedOnUTC")]
        public DateTime CreatedOnUTC { get; set; }

        [JsonPropertyName("UpdatedOnUTC")]
        public DateTime UpdatedOnUTC { get; set; }

        [JsonPropertyName("AssetNumber")]
        public string AssetNumber { get; set; }

        [JsonPropertyName("ItemId")]
        public int ItemId { get; set; }

        [JsonPropertyName("ItemNumber")]
        public string ItemNumber { get; set; }

        [JsonPropertyName("SerialNumber")]
        public string SerialNumber { get; set; }

        [JsonPropertyName("IOTId")]
        public string IOTId { get; set; }

        [JsonPropertyName("Comments")]
        public string Comments { get; set; }

        [JsonPropertyName("SourceLocation")]
        public Location SourceLocation { get; set; }

        [JsonPropertyName("DestinationLocation")]
        public Location DestinationLocation { get; set; }
    }

    public class EventData
    {
        [JsonPropertyName("UserId")]
        public string UserId { get; set; }

        [JsonPropertyName("Data")]
        public AssetData Data { get; set; }

        [JsonPropertyName("DataType")]
        public string DataType { get; set; }
    }


    public class Location
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        [JsonPropertyName("FullName")]
        public string FullName { get; set; }
        [JsonPropertyName("SiteId")]
        public int SiteId { get; set; }
        [JsonPropertyName("SiteName")]
        public string SiteName { get; set; }
    }

    public class Event
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("data")]
        public EventData Data { get; set; }

        [JsonPropertyName("eventType")]
        public string EventType { get; set; }

        [JsonPropertyName("dataVersion")]
        public string DataVersion { get; set; }

        [JsonPropertyName("metadataVersion")]
        public string MetadataVersion { get; set; }

        [JsonPropertyName("eventTime")]
        public DateTime EventTime { get; set; }

        [JsonPropertyName("topic")]
        public string Topic { get; set; }
    }
}
