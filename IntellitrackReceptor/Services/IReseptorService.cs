using ServiceRfidTekni;
using IntellitrackReceptor.Models;
namespace IntellitrackReceptor.Services
{
    public interface IReseptorService
    {
        Task<ReseptorResponse> SendReseptorRequest(Models.ReseptorRequest request);
    }
}