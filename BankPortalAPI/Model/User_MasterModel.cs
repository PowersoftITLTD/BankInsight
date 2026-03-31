namespace BankPortalAPI.Model
{
    public class User_MasterModel
    {
        public decimal MKEY { get; set; }
        public decimal COMPANY_ID { get; set; }
        public string? USER_CODE { get; set; }
        public string? USER_FULL_NAME { get; set; }
        public string? FIRST_NAME { get; set; }
        public string? LAST_NAME { get; set; }
        public string? ROLE_ID { get; set; }
        public string? PROJECT_ID { get; set; }
        public decimal? DESIGNATION_ID { get; set; }
        public decimal? DEPARTMENT_ID { get; set; }
        public decimal? CONTACT_NO { get; set; }
        public string? EMAIL_ID_OFFICIAL { get; set; }
        //public string? EMAIL_ID_PERSONAL { get; set; }
        // string? LOGIN_NAME { get; set; }
        //public byte[] LOGIN_PASSWORD { get; set; }
        public decimal? RA1_MKEY { get; set; }
        public decimal? RA2_MKEY { get; set; }
        public DateTime? EFFECTIVE_START_DATE { get; set; }
        //public DateTime? EFFECTIVE_END_DATE { get; set; }
        //public string? EMAIL_FREQUENCY { get; set; }
        //public string? BROWSER_NOTIFICATION { get; set; }
        //public string? WEB_TOKEN { get; set; }
        //public string? MOBILE_TOKEN { get; set; }
        public string? ATTRIBUTE1 { get; set; }
        public string? ATTRIBUTE2 { get; set; }
        public string? ATTRIBUTE3 { get; set; }
        public string? ATTRIBUTE4 { get; set; }
        public string? ATTRIBUTE5 { get; set; }
        //public decimal? CREATED_BY { get; set; }
        //public DateTime CREATION_DATE { get; set; }
        //public decimal? LAST_UPDATED_BY { get; set; }
        //public DateTime? LAST_UPDATE_DATE { get; set; }
        //public char DELETE_FLAG { get; set; }
        //public bool? ISFORGOTPASSWORD { get; set; }
        //public byte[] TEMPPASSWORD { get; set; }
        //public char? RESSET_FLAG { get; set; }
        public DateTime? Date_of_birth { get; set; }
        public decimal? ERP_EMP_MKEY { get; set; }
        public decimal? ORACLE_ID { get; set; }
        public int? JOB_ROLE { get; set; }
    }
    public class LegalEntityDetails
    {
        public int LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
    }
    public class BankDetails
    {
        public int? BankId { get; set; }
        public string? BankName { get; set; }
    }
    public class AccountStatus
    {
        public string? Status { get; set; }
    }
    public class BuildingDetails
    {
        public int? buildingId { get; set;}
        public string? buildingName { get; set;}
    }

    public class ProjectDetails
    {
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
    }

    public class AccountDetails
    {
        public int? AccountId { get; set; }
        public string? AccountName { get; set; }
        public string? AccountNo { get; set; }
    }
    public class CommonSpParameters
    {
        public int? UserId { get; set; }
        public int? BusinessGroupId { get; set; }
        public string? Attribute1 { get; set; }
        public string? Attribute2 { get; set; }
        public string? Attribute3 { get; set; }
        public string? Attribute4 { get; set; }
    }

    public class CommonListParameters
    {
        public int? UserId { get; set; }
        public int? BusinessGroupId { get; set; }
        public string? Entity { get; set; }
        public string? Project { get; set; }
        public string? Building { get; set; }
        public string? Bank { get; set; }
        public string? Account { get; set; }
        public string? BalanceRange { get; set; }
    }




    public class BankAccountInfo
    {
        public int legalEntityId { get; set; }
        public string? legalEntityName { get; set; }
        public int buildingId { get; set; }
        public string? buildingName { get; set; }
        public int bankId { get; set; }
        public string? bankName { get; set; }
        public string? accountType { get; set; }
        public string? accountNo { get; set; }
        public decimal lastBalance { get; set; }
        public DateTime? lastTransactionDatetime { get; set; }
        public string? status { get; set; }
    }
}
