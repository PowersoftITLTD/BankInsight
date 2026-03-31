using BankPortalAPI.Model;

namespace BankPortalAPI.Repository.Iservices
{
    public interface IUserMasterAuthServices
    {
        Task<string> Authenticate(string username, string password);
        Task<bool> ValidateUserByUserInput(string userName);
        Task<string?> GetUserMasterTokenByUserInputAsync(string userName);
        Task<string> LoginRegistration(UserLoginModel userLogin);
        string DecryptPassword(string encryptedPassword, string keyString);
        byte[] UserEncryptedReponsone(UserModel userModel, string keyString);

        Task<string> VerifyingResponse(string userLogin, string Password);
        Task<int> GetUserIdbyUserName(string userName);
        Task<bool> GetSingleSignOnLogin(string userInput);
        Task<User_MasterModel> GetUserMasterDetails(string userEmail);
        byte[] UserMasterEncryptedReponsone(UserMasterLoginModel userMasterModel, string keyString);
        // Encrypted and Decrypted The User 

        byte[] EncryptedInputbuUser(string userInput, string keyString);
        Task<IEnumerable<T>> GetDataFromSpAsync<T>(string storedProcedureName, CommonSpParameters parametersModel);

        Task<IEnumerable<ForgotPasswordOutPut_List>> GetForgotPasswordAsync(string LoginName);
        Task<IEnumerable<ResetPasswordOutPut_List>> GetResetPasswordAsync(string TEMPPASSWORD, string LoginName);
        Task<IEnumerable<PutChangePasswordOutPutNT>> PostChangePasswordAsync(ChangePasswordInputNT changePasswordInput);
        Task<string> InsertBankUserDetailsListAsync(List<bankUserDetails_HDR> userList);
        Task<string> InsertBankAccountDetailsAsync(List<BankAccountDetails> accountList);
    }
}
