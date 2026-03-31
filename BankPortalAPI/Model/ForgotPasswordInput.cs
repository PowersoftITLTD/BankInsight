using System.Text.Json.Serialization;

namespace BankPortalAPI.Model
{
    public class ForgotPasswordInput
    {
        [JsonPropertyName("LoginName")]
        public string LoginName { get; set; }
    }
    public class ForgotPasswordOutPut_List
    {
        [JsonPropertyName("Status")]
        public string? Status { get; set; }
        [JsonPropertyName("Message")]
        public string? Message { get; set; }

        public IEnumerable<ForgotPasswordOutPut> Data { get; set; }
    }
    public class ForgotPasswordOutPut
    {
        [JsonPropertyName("MessageText")]
        public string? MessageText { get; set; }
        [JsonIgnore]
        public int ErrorNumber { get; set; }
    }
}
