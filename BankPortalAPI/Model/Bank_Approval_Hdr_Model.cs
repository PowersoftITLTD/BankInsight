using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BankPortalAPI.Model
{
    public class Bank_Approval_Hdr_Model
    {
        public int Mkey { get; set; }
        public DateTime? EntryDateTime { get; set; }
        public int? LegalEntityId { get; set; }
        public string? LegalEntityName { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public string? TransactionType { get; set; }
        public decimal? Amount { get; set; }
        public string? DisplayText { get; set; }
        public int? NS_Internal_ID { get; set; }
        public string? NS_User_ID { get; set; }
        public string? NS_User_Name { get; set; }
        public DateTime? LastTransactionDatetime { get; set; }
        public int? AppoverID { get; set; }
        public string? ApproverName { get; set; }
        public int? RequestedBy { get; set; }
        public string? RequestedByName { get; set; }
        public string? ActionCode { get; set; }
        public DateTime? ActionTime { get; set; }

        [Column(TypeName = "nchar(1)")]
        [StringLength(1, ErrorMessage = "ActiveFlag must be 1 character long.")]
        public string? ActiveFlag { get; set; }
        public string? Status { get; set; }
        public string Process_Flag { get; set; } = null!;
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
        [DefaultValue("N")]
        public string DELETE_FLAG { get; set; } = "N";

        public int? UserId { get; set; }

        public int? BusinessGroupId { get; set; }
    }

    public class Update_BnakPortalApproval
    {
        public int Mkey { get; set; }
        public string ActionCode { get; set; }
        public DateTime ActionTime { get; set; }
        public int Amount { get; set; }

        public int? UserId { get; set; }

        public int? BusinessGroupId { get; set; }
    }
    
   public class Bank_Approval_Hdr_H_Model
    {
        public int HISTSEQ_NO { get; set; }
        public DateTime HIST_DATE { get; set; }
        public int Mkey { get; set; }
        public DateTime? EntryDateTime { get; set; }
        public int? LegalEntityId { get; set; }
        public string? LegalEntityName { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public int? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public string? TransactionType { get; set; }
        public decimal? Amount { get; set; }
        public string? DisplayText { get; set; }
        public int? NS_Internal_ID { get; set; }
        public string? NS_User_ID { get; set; }
        public string? NS_User_Name { get; set; }
        public DateTime? LastTransactionDatetime { get; set; }
        public int? AppoverID { get; set; }
        public string? ApproverName { get; set; }
        public int? RequestedBy { get; set; }
        public string? RequestedByName { get; set; }
        public string? ActionCode { get; set; }
        public DateTime? ActionTime { get; set; }
        public string? ActiveFlag { get; set; }
        public string? Status { get; set; }
        public string Process_Flag { get; set; } = string.Empty;
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
        public char DELETE_FLAG { get; set; }

        // Extra Property For Future used 
        public int UserId { get; set;}
        public int BusinessGroupId { get; set;}

    }

    public class BankDetailByCard
    {
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string Bank_Acount { get; set; }
        public string AccountType { get; set; }
        public string LinkedProject { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public decimal TotalBalance { get; set; }
    }


    public class CommonTransactionsModel
    {
        public string? legalEntityName { get; set; }
        public string? ProjectName { get; set; }
        public string? from { get; set; }
        public string? to { get; set; }
        public decimal? amount { get; set; }
        public string status { get; set;}
    }

    public class NetSuiteBankDetails
    {
        public string consumer_key { get; set; }
        public string consumer_secret { get; set; }
        public string access_token { get; set; }
        public string token_secret { get; set; }
        public string ApiBaseUrl { get; set; }
        public string Realm { get; set; }
    }





    //public class ViewBankDetailsModel
    //{
    //    public int Mkey { get; set; }
    //    public DateTime? EntryDateTime { get; set; }
    //    public int? LegalEntityId { get; set; }
    //    public string? LegalEntityName { get; set; }
    //    public int? ProjectId { get; set; }
    //    public string? ProjectName { get; set; }
    //    public int? BuildingId { get; set; }
    //    public string? BuildingName { get; set; }
    //    public string? AccountId { get; set; }
    //    public string? AccountName { get; set; }
    //    public string? AccountNo { get; set; }
    //    public string? IFSCCode { get; set; }

    //}
}
