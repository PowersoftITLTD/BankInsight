using BankPortalAPI.Model;
using BankPortalAPI.Repository.Services;

namespace BankPortalAPI.Repository.Iservices
{
    public interface IRefreshAll_Bank_Acc_DetailsServices
    {
        string GetBank_Acc_Subsidiary_Summ_NSQuery_L1();
        string GetSummaryStringQuery_L2();
        string GetBankAccountDetailbySubsidiaryid_Query_L3(int subsidiaryId);
        Task<CommonResponseObject> TriggerBankSummary_BY_NetSuiteQlAsync_L2();
        Task<CommonResponseObject> TriggerBankDetail_By_SusidiaryidAsync_L3(int Subsidiaryid);

        Task<CommonResponseObject> TriggerBank_Acc_Subsidiary_Summ_NSAsync_L1();

        Task<ProcessResponse> ProcessBankAccSubsidiarySummaryMakeHistoryAsync_L2(decimal? userId, int? businessGrp);
        Task<ProcessResponse> ProcessBankAccSubsidiarySummaryDeleteDetailsAsync(decimal? userId, int? businessGrp);
        Task<InsertResponse> AddBank_Acc_Summ_Async_L2(Bank_Acc_Summ_NS log, string userid);

        Task<InsertBankAcc_SummResponse> AddSubSidiaryBankDetailsSummary_L3(BankDetails_By_Subsidiary log, string userid);
        Task<InsertResponse> AddBank_Acc_Subsidiary_Summ_NSSummary_L1(Bank_Acc_Subsidiary_Summ_NS log, string userid);
        Task<string> AddBank_Acc_summ_LogAsyncL2(Bank_Acc_Summ_NS_LogModel log, string userid);

        Task<string> AddBank_Acc_SubSidiary_summ_LogAsyncL1(Bank_Acc_Subsidiary_Summ_NS_LogModel log);
        Task<string> AddSubSidairy_BankDetailsSummary_LogAsync_L3(BankDetails_By_Subsidiary_LogModel log, string userid);
        Task<Bank_Acc_Log_NS> MapBank_Acc_Log_NS_Model_L3(BankDetails_By_Subsidiary_LogModel bankAccSubsidiarySummNS);
        Task<Bank_Acc_Log_NS> MapBank_Acc_Log_NS_Model_L2(Bank_Acc_Summ_NS_LogModel bankAccSubsidiarySummNS);
        Task<Bank_Acc_Subsidiary_Summ_NS_LogModel> MapBank_Acc_Subsidiary_Summ_NS_ToLogModel(Bank_Acc_Subsidiary_Summ_NS bankAccSubsidiarySummNS);

        Task<Bank_Acc_Log_NS> MapBank_Acc_Log_NS_Model(Bank_Acc_Subsidiary_Summ_NS_LogModel bankAccSubsidiarySummNS);
        Task<string> AddBank_Acc_Log_NS_Async(Bank_Acc_Log_NS log, string userid);
        Task<BankDetails_By_Subsidiary_LogModel> MapBank_Details_By_Subsidiary_ToLogModel(BankDetails_By_Subsidiary bankDetailsBySubsidiary);

        Task<ProcessResponse> ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(decimal? userId, int? businessGrp);
        Task<Bank_Acc_Summ_NS_LogModel> MapBank_Acc_Summ_NS_ToLogModel(Bank_Acc_Summ_NS bankAccSummNS);


        // Update Method service Start Here 
        Task<InsertResponse> UpdateBank_Acc_Subsidiary_Summ_NsSummary_L1(Bank_Acc_Subsidiary_Summ_NS log, string userid, string UserName);
        Task<InsertBankAcc_SummResponse> Update_SubSidiaryBankDetailsSummary_L3(BankDetails_By_Subsidiary log, string userid, string UserName);

        Task<InsertResponse> Update_Bank_Acc_Summ_Async_L2(Bank_Acc_Summ_NS log, string userid, string UserName);
        Task<SubIdValidationResponse> CheckSubIdExistAsync(int subId);

        // Bank Account Details By Bank Api 

        Task<Bank_ApprovalConfig> GetBank_ApprovalConfig(string keytype);
        byte[] digest(string asymkey);
        Task<List<BankAccDetails>> GetBankAccDetailsAsync(string accountNo);

        Task<int> InsertAccountBalanceDetailsAsync(string acctNumber, string customerID, string Creadtedby, string createdbyName);
        Task<string> GenerateCustomerRequestJson(BankAccDetails bankAccDetails);
        Task<string> GenerateRequestJsonSignature(BankAccDetails bankAccDetails, string encrypt);
        Task<string> GenerateRequestJson(BankAccDetails bankAccDetails, string encrypt);

        string SignData(string data, string privateKeyPEM);
        bool VerifyData(string data, string signature, string publicKeyPEM);
        Task<string> CallBankApiWithRetryAsync(Bank_ApprovalConfig requestModel, string jsonPayload, string Signature);
        Task<AccountBalanceDetails> GetAccountBalanceDetailsByMkeyAsync(int mkey);
        Task<string> UpdateAccountBalanceDetailsAsync(AccountBalanceDetails model);
        string UpdateBankDetailsHdr(string lastBalance, DateTime lastTxnDate, string accountNo, string unclearFunds, string netBalance, string balAvailable, string holdAmount, string overdraft, string customerName);


        // Logging Service Methods
        Task<string> InsertBankAccResponseLogAsync(Bank_Acc_Response_log_Model model, string userid);

        Task<Bank_Acc_Response_log_Model> MapBank_Acc_Summ_NS_ToLogModel(BankAccDetails model);
        Task<Bank_Acc_Response_Details> GetBnakAccDetailResponse_byMkey(int mkey);
        Task<Bank_Acc_Response_log_Model> MapBank_Acc_Response_Details_ToLogModel(Bank_Acc_Response_Details model);

        Task<string> UpdateAccountBalanceDetailsAsync(Bank_Acc_Response_Details model, string userid);


    }
}
