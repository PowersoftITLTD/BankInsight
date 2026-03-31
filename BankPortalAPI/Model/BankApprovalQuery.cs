using System.Globalization;
using System.Text.Json.Serialization;

namespace BankPortalAPI.Model
{
    public class BankApprovalQuery
    {
        [JsonPropertyName("acctNumber")]
        public string? AcctNumber { get; set; }

        [JsonPropertyName("totalCurrentBalance")]
        public decimal? TotalCurrentBalance { get; set; } // Better as decimal, not string

        [JsonPropertyName("customerName")]
        public string? CustomerName { get; set; }
    }
     public class BankDetails_Model
    {
        public int? businessGroupId { get; set;}
        public string? entity { get; set;}
        public string? project { get; set;}
        public string? building { get; set;}
        public string? bank { get; set;}
        public string? account { get; set;}
        public string? balanceRange { get; set;}

        public int? UserId { get; set;}

    }

}
