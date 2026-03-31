using BankPortalAPI.Model;

namespace BankPortalAPI.Repository.Iservices
{
    public interface IBankPortalServices
    {
        Task<List<BankPortal_model>> GetAllBankDetails(CommonListParameters commonListParametersint );  // userId, int businessGroupId
        Task<CommonResponseObject> GetAllBankDetails_Ps(CommonListParameters commonListParameters);
        Task<string> InsertOrUpdateBankApprovalAsync(Bank_Approval_Hdr_Model model);
        Task<Bank_Approval_Hdr_Model> GetBankApprovalByUserId(int mkey, int? userId, int? BusinessGroupId);
        Task<List<Bank_Approval_Hdr_Model>> GetBankApprovalHdrList(char activeflag , int? userId, int? businessGroupId);
        Task<List<BankApprovalQuery>> GetBankApprovalQueryListAsync(int? userId = null, int? businessGroupId = null);
        Task<List<EntityBalance_Model>> GetEntityBankAccountBalanceAsync(int? userId = null, int? businessGroupId = null);
        Task<List<AccountType_BalanceModel>> GetAccountTypeBalanceAsync(int? userId = null, int? businessGroupId = null);
        Task<List<BankAccountDetailsModel>> GetBankAccountDetailsAsync(int? userId = null, int? businessGroupId = null);
        Task<List<EntityModel>> GetEntityListAsync(CommonListParameters commonListParameters );  //int? userId = null, int? businessGroupId = null
        Task<List<ProjectModel>> GetProjectListAsync(CommonListParameters commonListParameters);  //  int? userId = null, int? businessGroupId = null
        Task<List<BankTopProjectbyBalance>> GetBankTopbyProjectBalanceAsync(int? userId = null, int? businessGroupId = null);
        Task<List<BankDetailByCard>> GetBankDetailsByCardAsync(int? userId = null, int? businessGroupId = null);
        Task<List<BankPortal_model>> GetALLBankDetailsByCardAsync(int? userId = null, int? businessGroupId = null, int? BankId = null, string BankName = null);
        Task<List<CommonTransactionsModel>> GetApprovedBankApproval_HdrList(int? userId, int? BusinessGroupId);
        Task<List<CommonTransactionsModel>> GetPendingBankApproval_HdrList(int? userId, int? BusinessGroupId);
        Task<List<CommonTransactionsModel>> GetRejectedBankApproval_HdrList(int? userId, int? BusinessGroupId);
        Task<List<CommonTransactionsModel>> GetCompletedBankApproval_HdrList(int? userId, int? BusinessGroupId);
        Task<IEnumerable<BankPortal_model>> GetAdditionalInformationAsync(CommonSpParameters commonSp);

        // Add more method signatures as needed


        Task<IEnumerable<BankAccountSummaryModel>> GetBankAccountSummaryActiveAsync();
        Task<IEnumerable<BankAccountSummary_Ns_Model>> GetBankAccountSummaryBySubIdAsync(int? subId);
        Task<IEnumerable<Bank_Acc_Details_NS_Model>> GetBankAccountSummaryBySubId_ProjectIdAsync(int? subId, int? project);
        Task<BankAccountSummary_ResponseModel> GetMapBankAccountSummary_IntoBankAccountSummary_BySubId(BankAccountSummaryModel bankAccountSummary);
       // Task<CommonResponseObject> TriggerBank_Acc_Subsidiary_Summ_NSAsync_L1();
    }
}
