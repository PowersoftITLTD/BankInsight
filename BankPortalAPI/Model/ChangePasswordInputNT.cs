using System.Text.Json.Serialization;

namespace BankPortalAPI.Model
{
    public class ChangePasswordInputNT
    {
        [JsonPropertyName("LoginName")]
        public string LoginName { get; set; }
        [JsonPropertyName("Old_Password")]
        public string Old_Password { get; set; }
        [JsonPropertyName("New_Password")]
        public string New_Password { get; set; }
        [JsonPropertyName("Session_User_ID")]
        public int Session_User_ID { get; set; }
        [JsonPropertyName("Business_Group_ID")]
        public int Business_Group_ID { get; set; }
    }
    public class PutChangePasswordOutPutNT
    {
        [JsonPropertyName("Status")]
        public string? Status { get; set; }
        [JsonPropertyName("Message")]
        public string? Message { get; set; }

        public IEnumerable<PutChangePasswordNT> Data { get; set; }
    }
    public class PutChangePasswordNT
    {

        [JsonPropertyName("MessageText")]
        public string? MessageText { get; set; }
    }

    public class PutChangePasswordOutPut_List
    {
        [JsonPropertyName("Status")]
        public string? Status { get; set; }
        [JsonPropertyName("Message")]
        public string? Message { get; set; }

        public IEnumerable<PutChangePasswordNT> Data { get; set; }
    }
}
