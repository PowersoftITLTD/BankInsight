using System.Text.Json.Serialization;

namespace BankPortalAPI.Model
{
    public class RootRequestModel
    {
        public RequestModel Request { get; set; }
    }

    public class RootRequestModelPlain
    {
        [JsonPropertyName("Request")]
        public RequestModelPlain Request { get; set; }
    }
    public class RequestModel
    {
        public BodyModel body { get; set; }
    }
    public class RequestModelPlain
    {
        public BodyModelPlain body { get; set; }
    }
    public class BodyModel
    {
        public string branchCode { get; set; }
        public string encryptData { get; set; }
    }
    //public class BodyModelPlain
    //{
    //    public string branchCode { get; set; }
    //    public CustomerRequestModel encryptData { get; set; }
    //}
    //public class CustomerRequestModel
    //{
    //    [JsonPropertyName("Authorization")]
    //    public string Authorization { get; set; }

    //    [JsonPropertyName("acctNumber")]
    //    public string AccountNumber { get; set; }

    //    public string CustomerID { get; set; }

    //    public string Key { get; set; }
    //}

    public class AccountBalanceModel
    {
        public string acctNumber { get; set; }
        public string currentBalance { get; set; }
        public string unclearFunds { get; set; }
        public string netBalance { get; set; }
        public string balAvailable { get; set; }
        public string holdAmount { get; set; }
        public string overdraft { get; set; }
        public string customerName { get; set; }
        public string customerID { get; set; }
    }

    public class RootResponse
    {
        public Response Response { get; set; }
    }

    public class Response
    {
        public Metadata metadata { get; set; }
        public Body body { get; set; }
    }

    public class Metadata
    {
        public Status status { get; set; }
    }

    public class Status
    {
        public string contextID { get; set; }
        public Message message { get; set; }
        public string result { get; set; }
    }

    public class Message
    {
        public string code { get; set; }
        public string type { get; set; }
    }




    public class Body
    {
        public AccountDetailss accountDetails { get; set; }
        public CustomerAccountDetails customerAccountDetails { get; set; }
        public string encryptData { get; set; }
    }

    public class AccountDetailss
    {
        public string currency { get; set; }
        public string status { get; set; }
        public string relationship { get; set; }
        public string chequeBookFacility { get; set; }
        public string minBalance { get; set; }
        public OpeningDate openingDate { get; set; }
    }

    public class OpeningDate
    {
        public string dateString { get; set; }
    }

    public class CustomerAccountDetails
    {
        public AccountDetailsInner accountDetails { get; set; }
    }

    public class AccountDetailsInner
    {
        public string branchCode { get; set; }
        public string acctTypeCode { get; set; }
        public string currencyDesc { get; set; }
        public string currencyCode { get; set; }
        public string accountType { get; set; }
    }
}
