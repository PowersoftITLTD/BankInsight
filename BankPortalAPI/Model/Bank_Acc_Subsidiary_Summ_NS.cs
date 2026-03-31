namespace BankPortalAPI.Model
{
    public class Bank_Acc_Subsidiary_Summ_NS
    {
        public int Mkey { get; set; }
        public string subsidiary { get; set; }
        public string custrecord_htl_bank_account_number { get; set; }
        public string description { get; set; }
        public string displaynamewithhierarchy { get; set; }
        public string accounttype { get; set; }
        public decimal? account_bal { get; set; }
        public decimal? banktotal { get; set; }
        public decimal? notcleardbanktotal { get; set; }
        public int subid { get; set; }
        public string project { get; set; }
        public decimal? closing_balance_as_per_bank_statement { get; set; }
        public decimal? current_account_balance_as_per_bank_book { get; set; }

        public string Attribute1 { get; set; }
        public string Attribute2 { get; set; }
        public string Attribute3 { get; set; }
        public string Attribute4 { get; set; }
        public string Attribute5 { get; set; }

        // 🔹 Audit Fields
        public decimal Created_By { get; set; }
        public string Created_By_Name { get; set; }
        public DateTime Creation_Date { get; set; }

        public decimal? Last_Updated_By { get; set; }
        public string Last_Updated_By_Name { get; set; }
        public DateTime? Last_Update_Date { get; set; }

        // 🔹 Soft Delete
        public string Delete_Flag { get; set; }

        public decimal? LastBalance { get; set; }
        public decimal? unclearFunds { get; set; }
        public decimal? netBalance { get; set; }
        public decimal? balAvailable { get; set; }
        public decimal? holdAmount { get; set; }
        public decimal? overdraft { get; set; }
        public string customerName { get; set; }
        public string LastTransactionDatetime { get; set; }
    }

    public class BankAccountSummaryModel
    {
        public int Mkey { get; set; }
        public string subsidiary { get; set; }
        public string custrecord_htl_bank_account_number { get; set; }
        public string description { get; set; }
        public string displaynamewithhierarchy { get; set; }
        public string accounttype { get; set; }
        public decimal? account_bal { get; set; }
        public decimal? banktotal { get; set; }
        public decimal? notcleardbanktotal { get; set; }
        public int? subid { get; set; }
        public string project { get; set; }
        public decimal? closing_balance_as_per_bank_statement { get; set; }
        public decimal? current_account_balance_as_per_bank_book { get; set; }
        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE2 { get; set; }
        public string ATTRIBUTE3 { get; set; }
        public string ATTRIBUTE4 { get; set; }
        public string ATTRIBUTE5 { get; set; }
        public long CREATED_BY { get; set; }
        public string CREATED_BY_Name { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public long? LAST_UPDATED_BY { get; set; }
        public string LAST_UPDATED_BY_Name { get; set; }
        public DateTime? LAST_UPDATE_DATE { get; set; }
        public string DELETE_FLAG { get; set; }
        public decimal? LastBalance { get; set; }
        public decimal? unclearFunds { get; set; }
        public decimal? netBalance { get; set; }
        public decimal? balAvailable { get; set; }
        public decimal? holdAmount { get; set; }
        public decimal? overdraft { get; set; }
        public string? customerName { get; set; }
        public string? LastTransactionDatetime { get; set; }

    }


    public class BankAccountSummary_ResponseModel
    {
        public int Mkey { get; set; }
        public string subsidiary { get; set; }
        public string custrecord_htl_bank_account_number { get; set; }
        public string description { get; set; }
        public string displaynamewithhierarchy { get; set; }
        public string accounttype { get; set; }
        public decimal? account_bal { get; set; }
        public decimal? banktotal { get; set; }
        public decimal? notcleardbanktotal { get; set; }
        public int? subid { get; set; }
        public string project { get; set; }
        public decimal? closing_balance_as_per_bank_statement { get; set; }
        public decimal? current_account_balance_as_per_bank_book { get; set; }
        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE2 { get; set; }
        public string ATTRIBUTE3 { get; set; }
        public string ATTRIBUTE4 { get; set; }
        public string ATTRIBUTE5 { get; set; }
        public long CREATED_BY { get; set; }
        public string CREATED_BY_Name { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public long? LAST_UPDATED_BY { get; set; }
        public string LAST_UPDATED_BY_Name { get; set; }
        public DateTime? LAST_UPDATE_DATE { get; set; }
        public string DELETE_FLAG { get; set; }

        

        public decimal? LastBalance { get; set; }
        public decimal? unclearFunds { get; set; }
        public decimal? netBalance { get; set; }
        public decimal? balAvailable { get; set; }
        public decimal? holdAmount { get; set; }
        public decimal? overdraft { get; set; }
        public string? customerName { get; set; }
        public string? LastTransactionDatetime { get; set; }
        public List<BankAccountSummary_Ns_Model> bankAccountSummary_Ns { get; set; }

    }


    public class BankAccountSummary_Ns_Model
    {
        public int Mkey { get; set; }

        public string subsidiary { get; set; }
        public string custrecord_htl_bank_account_number { get; set; }
        public string description { get; set; }
        public string displaynamewithhierarchy { get; set; }
        public string accounttype { get; set; }

        public decimal? account_bal { get; set; }
        public decimal? banktotal { get; set; }
        public decimal? notcleardbanktotal { get; set; }

        public int? subid { get; set; }
        public int? projectid { get; set; }

        public string project { get; set; }

        public decimal? closing_balance_as_per_bank_statement { get; set; }
        public decimal? current_account_balance_as_per_bank_book { get; set; }

        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE2 { get; set; }
        public string ATTRIBUTE3 { get; set; }
        public string ATTRIBUTE4 { get; set; }
        public string ATTRIBUTE5 { get; set; }

        public long CREATED_BY { get; set; }
        public string CREATED_BY_Name { get; set; }

        public DateTime CREATION_DATE { get; set; }

        public long? LAST_UPDATED_BY { get; set; }
        public string LAST_UPDATED_BY_Name { get; set; }

        public DateTime? LAST_UPDATE_DATE { get; set; }

        public string DELETE_FLAG { get; set; }
        public List<Bank_Acc_Details_NS_Model> bank_Acc_Details_NS_ { get; set; }

        public decimal? LastBalance { get; set; }
        public decimal? unclearFunds { get; set; }
        public decimal? netBalance { get; set; }
        public decimal? balAvailable { get; set; }
        public decimal? holdAmount { get; set; }
        public decimal? overdraft { get; set; }
        public string? customerName { get; set; }
        public string? LastTransactionDatetime { get; set; }
    }

    public class Bank_Acc_Details_NS_Model
    {
        public int Mkey { get; set; }
        public int Sr_no { get; set; }

        public string subsidiary { get; set; }
        public string custrecord_htl_bank_account_number { get; set; }
        public string displaynamewithhierarchy { get; set; }
        public string accounttype { get; set; }

        public decimal? account_bal { get; set; }
        public decimal? closing_bal { get; set; }

        public int? subid { get; set; }
        public int? projectid { get; set; }

        public string project { get; set; }

        public decimal? closing_balance_as_per_bank_statement { get; set; }
        public decimal? current_account_balance_as_per_bank_book { get; set; }

        public string lastrecodate { get; set; }

        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE2 { get; set; }
        public string ATTRIBUTE3 { get; set; }
        public string ATTRIBUTE4 { get; set; }
        public string ATTRIBUTE5 { get; set; }

        public long CREATED_BY { get; set; }
        public string CREATED_BY_Name { get; set; }

        public DateTime CREATION_DATE { get; set; }

        public long? LAST_UPDATED_BY { get; set; }
        public string LAST_UPDATED_BY_Name { get; set; }

        public DateTime? LAST_UPDATE_DATE { get; set; }

        public string DELETE_FLAG { get; set; }

        public decimal? LastBalance { get; set; }
        public decimal? unclearFunds { get; set; }
        public decimal? netBalance { get; set; }
        public decimal? balAvailable { get; set; }
        public decimal? holdAmount { get; set; }
        public decimal? overdraft { get; set; }
        public string? customerName { get; set; }
        public string? LastTransactionDatetime { get; set; }
    }

    public class RefreshBankAccDetailsByIdRequest
    {
        public int? UserId { get; set; }
        public int? BusinessGroupId { get; set; }
        //public int Mkey { get; set; }
        public int SubId { get; set; }
        public int? ProjectId { get; set; }
        public string? BankAccountNumber { get; set; }
    }





    public class SubIdValidationResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }

}
