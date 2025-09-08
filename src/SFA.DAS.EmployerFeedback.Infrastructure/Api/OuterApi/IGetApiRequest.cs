using Newtonsoft.Json;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi
{
    public interface IGetApiRequest 
    {
        [JsonIgnore]
        string GetUrl { get; }
    }
}