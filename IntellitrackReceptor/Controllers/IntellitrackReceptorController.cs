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
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace IntellitrackReceptor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntellitrackReceptorController : ControllerBase
    {
        private readonly ILogger<IntellitrackReceptorController> _logger;
        private readonly IReseptorService _reseptorService;
        private readonly string _eventSource = "IntellitrackReceptorApp"; // Fuente de eventos
        private readonly string _logName = "Application"; // Log de aplicación (por defecto)

        public IntellitrackReceptorController(ILogger<IntellitrackReceptorController> logger, IReseptorService reseptorService)
        {
            try
            {
                _logger = logger;
                _reseptorService = reseptorService;
                if (!EventLog.SourceExists(_eventSource))
                {
                    EventLog.CreateEventSource(_eventSource, _logName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> HandleEvent([FromBody] JsonElement data, [FromHeader(Name = "Client-ID")] string clientId, [FromHeader(Name = "Client-Secret")] string clientSecret)
        {
            //var expectedClientId = configuration["Authentication:ClientId"];
            //var expectedClientSecret = configuration["Authentication:ClientSecret"];
            var expectedClientId = "77c3b787-783a-4188-a20f-49906e78d4e9";
            var expectedClientSecret = "atc~PTRXjBYoM0Lz6hIvnH6QDc5GE1fpzjS8";

            // Verificar si el Client ID y Client Secret coinciden
            if (clientId != expectedClientId || clientSecret != expectedClientSecret)
            {
                _logger.LogWarning("Client ID o Client Secret inválidos");
                return Unauthorized("Client ID o Client Secret inválidos.");
            }

            try
            {
                if (data.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement eventData in data.EnumerateArray())
                    {
                        if (eventData.TryGetProperty("eventType", out JsonElement eventTypeElement))
                        {
                            var eventType = eventTypeElement.GetString();
                            if (eventType == "Microsoft.EventGrid.SubscriptionValidationEvent")
                            {
                                if (eventData.TryGetProperty("data", out JsonElement dataElement) &&
                                    dataElement.TryGetProperty("validationCode", out JsonElement validationCodeElement))
                                {
                                    var validationCode = validationCodeElement.GetString();
                                    _logger.LogInformation("Subscription validation event handled successfully");
                                    return Ok(new { validationResponse = validationCode });
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
                                }
                            }
                            else
                            {
                                EventLog.WriteEntry(_eventSource, "Se hara consultaal servicio rfid", EventLogEntryType.Information);
                                eventData.TryGetProperty("data", out JsonElement dataElement);
                                var eventObject = System.Text.Json.JsonSerializer.Deserialize<EventData>(dataElement.ToString());
                                _logger.LogWarning($"Unhandled event type: {eventType}");
                                _logger.LogWarning($"Unhandled event type: {eventType}");

                                var reseptorRequest = new ReseptorRequest
                                {
                                    Rfid = eventObject.Data.IOTId,
                                    Evnt = eventObject.Data.DestinationLocation.FullName.ToString(),
                                    Logn = "Interface"
                                };

                                var result = await _reseptorService.SendReseptorRequest(reseptorRequest);

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
                if (EventLog.SourceExists(_eventSource))
                {
                    EventLog.WriteEntry(_eventSource, "Error: "+ex, EventLogEntryType.Information);
                }
                else
                {
                    _logger.LogWarning($"La fuente de eventos '{_eventSource}' no existe. No se registró en el Visor de Eventos.");
                }

                _logger.LogError(ex, "Error processing the event");
                return StatusCode(500, "Internal server error");
            }
        }


    }

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
