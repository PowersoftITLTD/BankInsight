using System.Text.Json.Serialization;

namespace BankPortalAPI.Model
{
    public class ResetPasswordOutPut_List
    {
        [JsonPropertyName("Status")]
        public string? Status { get; set; }
        [JsonPropertyName("Message")]
        public string? Message { get; set; }

        public IEnumerable<ResetPasswordOutPut> Data { get; set; }
    }
    public class ResetPasswordOutPut
    {
        [JsonPropertyName("TEMPPASSWORD")]
        public string? TEMPPASSWORD { get; set; }
        [JsonPropertyName("login_name")]
        public string? login_name { get; set; }
        [JsonPropertyName("EMAIL_ID_OFFICIAL")]
        public string? EMAIL_ID_OFFICIAL { get; set; }
        [JsonIgnore]
        public string? Message { get; set; }
        [JsonIgnore]
        public int ErrorNumber { get; set; }
    }
}
