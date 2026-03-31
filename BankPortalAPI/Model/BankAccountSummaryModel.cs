using System.Text.Json.Serialization;

namespace BankPortalAPI.Model
{
    public class EntityBalance_Model
    {
        
        public string legalEntityId { get; set; }
        
        public string legalEntityName { get; set; }
        
        public decimal totalCurrentBalance { get; set; }
        
        public decimal bankCount { get; set; }
        
        //public string customerName { get; set; }
        public int activeProject { get; set; }
        public DateTime lastUpdatedON { get; set; }
    }
    public class BankTopProjectbyBalance
    {
        public string projectId { get; set; }
        public decimal totalCurrentBalance { get; set; }
        public decimal bankCount { get; set; }
        public string projectName { get; set; }
        public int activeProject { get; set; }
        public DateTime lastUpdatedON { get; set; }
    }

    public class AccountType_BalanceModel
    {
        public string accountType { get; set; }
        public decimal totalCurrentBalance { get; set; }
    }

    public class BankAccountDetailsModel
    {
        public int bankId { get; set; }
        public string bankName { get; set; }
        public decimal totalCurrentBalance { get; set; }
        public DateTime lastUpdatedON { get; set; }
        public string accountType { get; set; }
        public string branchname { get; set; }
        public string ifscCode { get; set; }
        public string accountCount { get; set; }
    }

    public class EntityModel
    {
        public string legalEntityId { get; set; }
        public string legalEntityName { get; set; }
        public decimal balance { get; set; }
        public DateTime lastUpdatedON { get; set; }
        public int ProjectCount { get; set; }
        public string BankCount { get; set; }
        public string accountCount { get; set; }
        public string contact_No { get; set; }
        public string contact_Person { get; set; }
        public string gst_Number { get; set; }
    }

    public class ProjectModel
    {
        public string projectId {get; set;}
        public string projectName { get; set; }
        public decimal totalLastBalance { get; set; }
        public DateTime lastUpdatedON { get; set; }
        public int legalEntityCount { get; set; }
        public string bankCount { get; set; }
        public string accountCount { get; set; }
        public string contact_No { get; set; }
        public string contact_Person { get; set; }
    }
        
}
