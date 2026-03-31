using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankPortalAPI.Model
{
    public class Bank_ApprovalConfig
    {
        [Key]
        public int Mkey { get; set; }
        public string KeyType { get; set; }
        public string AsymKey { get; set; }
        public string PubKey { get; set; }
        public string PvtKey { get; set; }
        public string clientid { get; set; }
        public string clientsecret { get; set; }
        public string clientcertificate { get; set; }
        public string apiinteractionid { get; set; }
        public string CallURL { get; set; }
        public string Attribute1 { get; set; }
        public string Attribute2 { get; set; }
        public string Attribute3 { get; set; }
        public string Attribute4 { get; set; }
        public string Attribute5 { get; set; }
        public decimal CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreationDate { get; set; }
        public decimal? LastUpdatedBy { get; set; }
        public string LastUpdatedByName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        [DefaultValue("N")]
        [Required(ErrorMessage = "DeleteFlag must be a single letter.")]
        public char DeleteFlag { get; set; }
    }

    public class BankAccDetails
    {
        [Key]
        public int Mkey { get; set; }

        public string AcctNumber { get; set; }

        public string BranchCode { get; set; }

        public string AuthorizationVal { get; set; }

        public string CustomerID { get; set; }

        public string KeyVal { get; set; }

        public string ATTRIBUTE1 { get; set; }

        public string ATTRIBUTE2 { get; set; }

        public string ATTRIBUTE3 { get; set; }

        public string ATTRIBUTE4 { get; set; }

        public string ATTRIBUTE5 { get; set; }

        [Required]
        public decimal CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        public decimal? LastUpdatedBy { get; set; }

        public string LastUpdatedByName { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        [Required]
        public char DeleteFlag { get; set; }
    }

    public class BodyModelPlain
    {
        public string branchCode { get; set; }
        public CustomerRequestModel encryptData { get; set; }
    }
    public class CustomerRequestModel
    {
        [JsonPropertyName("Authorization")]
        public string Authorization { get; set; }

        [JsonPropertyName("acctNumber")]
        public string AccountNumber { get; set; }

        public string CustomerID { get; set; }

        public string Key { get; set; }
    }

    public class AccountBalanceDetails
    {
        public int Mkey { get; set; }
        public string acctNumber { get; set; }
        public string customerID { get; set; }
        public string keyVal { get; set; }
        public string ResponseData { get; set; }

        // ✅ Newly added fields
        public string result { get; set; }
        public string currency { get; set; }
        public string BankStatus { get; set; }
        public string relationship { get; set; }
        public string chequeBookFacility { get; set; }
        public decimal? minBalance { get; set; }
        public string dateString { get; set; }
        public string branchCode { get; set; }
        public string acctTypeCode { get; set; }
        public string currencyDesc { get; set; }
        public string currencyCode { get; set; }
        public string accountType { get; set; }

        // ✅ Existing fields
        public decimal? currentBalance { get; set; }
        public decimal? unclearFunds { get; set; }
        public decimal? netBalance { get; set; }
        public decimal? balAvailable { get; set; }
        public decimal? holdAmount { get; set; }
        public decimal? overdraft { get; set; }
        public string customerName { get; set; }
        public DateTime? ResponseTime { get; set; }
        public string Status { get; set; }
        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE2 { get; set; }
        public string ATTRIBUTE3 { get; set; }
        public string ATTRIBUTE4 { get; set; }
        public string ATTRIBUTE5 { get; set; }
        public decimal? CREATED_BY { get; set; }
        public string CREATED_BY_Name { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public decimal? LAST_UPDATED_BY { get; set; }
        public string LAST_UPDATED_BY_Name { get; set; }
        public DateTime? LAST_UPDATE_DATE { get; set; }
        public char DELETE_FLAG { get; set; }
    }
}
