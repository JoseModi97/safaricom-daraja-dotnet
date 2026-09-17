using System.Text.Json.Serialization;

namespace Safaricom.Daraja.IoT.Models
{
    public class CheckAtiRequest
    {
        [JsonPropertyName("customerNumber")]
        public string CustomerNumber { get; set; } = string.Empty;
    }
}
