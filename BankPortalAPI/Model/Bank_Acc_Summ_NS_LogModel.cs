namespace BankPortalAPI.Model
{
    public class Bank_Acc_Summ_NS_LogModel
    {
        public int Mkey { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string ActionName { get; set; }
        public string MethodName { get; set; }
        public string Subsidiary { get; set; }
        public string Project { get; set; }

        public int projectid { get; set; }
        public int SubId { get; set; }

        public string Closing_Balance_As_Per_Bank_Statement { get; set; }
        public string Current_Account_Balance_As_Per_Bank_Book { get; set; }
        public string accounttype { get; set; }
        public string custrecord_htl_bank_account_number { get; set; }
        public string displaynamewithhierarchy { get; set; }
        public string lastrecodate { get; set; }
        public string account_bal { get; set; }
        public string banktotal { get; set; }
        public string notcleardbanktotal { get; set; }
        public string closing_bal { get; set; }
        public string description { get; set; }
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

        public string DeleteFlag { get; set; }
        public decimal? LastBalance { get; set; }
        public decimal? unclearFunds { get; set; }
        public decimal? netBalance { get; set; }
        public decimal? balAvailable { get; set; }
        public decimal? holdAmount { get; set; }
        public decimal? overdraft { get; set; }
        public string customerName { get; set; }
        public string LastTransactionDatetime { get; set; }
    }

    public class BankDetails_By_Subsidiary_LogModel
    {
        // 🔹 Primary / Identity Fields

        public int Mkey { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string ActionName { get; set; }
        public string MethodName { get; set; }
        // public int Mkey { get; set; }
        public int SrNo { get; set; }

        // 🔹 Bank & Subsidiary Details
        public string Subsidiary { get; set; }
        public string Custrecord_Htl_Bank_Account_Number { get; set; }
        public string DisplayNameWithHierarchy { get; set; }
        public string AccountType { get; set; }
        public int? SubId { get; set; }
        public string Project { get; set; }
        public int projectid { get; set; }

        // 🔹 Financial Values
        public string Account_Bal { get; set; }
        public string Closing_Bal { get; set; }
        public string Closing_Balance_As_Per_Bank_Statement { get; set; }
        public string Current_Account_Balance_As_Per_Bank_Book { get; set; }

        // 🔹 Dates
        public string LastRecoDate { get; set; }

        // 🔹 Attributes
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

    public class Bank_Acc_Subsidiary_Summ_NS_LogModel
    {
        public int Mkey { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string ActionName { get; set; }
        public string MethodName { get; set; }
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


    public class Bank_Acc_Log_NS
    {
        public int Mkey { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public string ActionName { get; set; }

        public string MethodName { get; set; }

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
        public string projectid { get; set; }

        public decimal? closing_balance_as_per_bank_statement { get; set; }

        public decimal? current_account_balance_as_per_bank_book { get; set; }

        public string ATTRIBUTE1 { get; set; }

        public string ATTRIBUTE2 { get; set; }

        public string ATTRIBUTE3 { get; set; }

        public string ATTRIBUTE4 { get; set; }

        public string ATTRIBUTE5 { get; set; }

        public decimal CREATED_BY { get; set; }

        public string CREATED_BY_Name { get; set; }

        public DateTime CREATION_DATE { get; set; }

        public decimal? LAST_UPDATED_BY { get; set; }

        public string LAST_UPDATED_BY_Name { get; set; }

        public DateTime? LAST_UPDATE_DATE { get; set; }

        public string DELETE_FLAG { get; set; }

        public decimal? LastBalance { get; set; }

        public decimal? unclearFunds { get; set; }

        public decimal? netBalance { get; set; }

        public decimal? balAvailable { get; set; }

        public decimal? holdAmount { get; set; }

        public decimal? overdraft { get; set; }

        public string customerName { get; set; }

        public string LastTransactionDatetime { get; set; }
        public string LastRecoDate { get; set; }
    }
}
