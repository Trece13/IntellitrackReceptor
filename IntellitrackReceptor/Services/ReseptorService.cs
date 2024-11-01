using System.Threading.Tasks;
using ServiceRfidTekni;
using IntellitrackReceptor.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Management.Automation;

namespace IntellitrackReceptor.Services
{
    public class ReseptorService : IReseptorService
    {
        private readonly Service1Client _service1Client;
        private readonly ILogger<ReseptorService> _logger;

        public ReseptorService(ILogger<ReseptorService> logger,Service1Client service1Client)
        {
            _service1Client = service1Client;
            _logger = logger;
        }

        public async Task<ReseptorResponse> SendReseptorRequest(Models.ReseptorRequest request)
        {
            // Lógica para invocar el servicio WCF
            return await _service1Client.ReseptorAsync(request.Rfid, request.Evnt, request.Logn);
        }

        public async Task RestartRemoteServiceAsync()
        {
            try
            {
                var remoteServer = "https://gpbfpr.tekni-plex.com/"; // Cambia por el nombre o IP del servidor remoto
                var serviceName = "rfidservice"; // Cambia por el nombre real del servicio

                using (PowerShell powerShell = PowerShell.Create())
                {
                    // Script de PowerShell para detener e iniciar el servicio en el servidor remoto
                    string script = $@"
                Invoke-Command -ComputerName {remoteServer} -ScriptBlock {{
                    Stop-Service -Name '{serviceName}' -Force;
                    Start-Service -Name '{serviceName}';
                }}";

                    powerShell.AddScript(script);

                    var results = await Task.Run(() => powerShell.Invoke());

                    if (powerShell.HadErrors)
                    {
                        foreach (var error in powerShell.Streams.Error)
                        {
                            _logger.LogError($"PowerShell Error: {error}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Service {serviceName} restarted successfully on {remoteServer}.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart the remote service.");
            }
        }
    }
}