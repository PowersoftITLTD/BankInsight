namespace BankPortalAPI.Model
{
    public class BankPortal_model
    {
        public int Mkey { get; set; }
        public DateTime EntryDateTime { get; set; }
        public int LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int BuildingId { get; set; }
        public string BuildingName { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public string IFSCCode { get; set; }
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string AccountType { get; set; }
        public string TagType { get; set; }
        public string CustId { get; set; }
        public decimal LastBalance { get; set; }
        public decimal unclearFunds { get; set; }
        public decimal netBalance { get; set; }
        public decimal balAvailable { get; set; }
        public decimal holdAmount { get; set; }
        public decimal overdraft { get; set; }
        public DateTime? LastTransactionDatetime { get; set; }
        public string ActiveFlag { get; set; }
        public string Status { get; set; }
        public string Process_Flag { get; set; }
        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE2 { get; set; }
        public string ATTRIBUTE3 { get; set; }
        public string ATTRIBUTE4 { get; set; }
        public string ATTRIBUTE5 { get; set; }
        public decimal CREATED_BY { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public decimal? LAST_UPDATED_BY { get; set; }
        public DateTime? LAST_UPDATE_DATE { get; set; }
        public char DELETE_FLAG { get; set; }
    }

    public class BankAccountDetails
    {
        public int Mkey { get; set; }
        public string? acctNumber { get; set; }
        public string? branchCode { get; set; }
        public string? AuthorizationVal { get; set; }
        public string? customerID { get; set; }
        public string? keyVal { get; set; }
        public string? CompanyName { get; set; }
        public int? CompanyId { get; set; }
        public string? ProjectName { get; set; }
        public int? ProjectId { get; set; }
        public string? BuildingName { get; set; }
        public int? BuildingId { get; set; }
        public string? ATTRIBUTE1 { get; set; }
        public string? ATTRIBUTE2 { get; set; }
        public string? ATTRIBUTE3 { get; set; }
        public string? ATTRIBUTE4 { get; set; }
        public string? ATTRIBUTE5 { get; set; }
        public decimal CREATED_BY { get; set; }
         public string? CREATED_BY_Name { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public decimal? LAST_UPDATED_BY { get; set; }
        public string? LAST_UPDATED_BY_Name { get; set; }
        public DateTime? LAST_UPDATE_DATE { get; set; }
        public string DELETE_FLAG { get; set; }
    }



}
