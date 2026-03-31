using System.Data;
using System.Data.SqlClient;
using BankPortalAPI.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BankPortalAPI.Repository.Iservices;
using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using System.Net;

namespace BankPortalAPI.Repository.Services
{


    public class UserMasterAuthServices : IUserMasterAuthServices
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;

        public UserMasterAuthServices(IConfiguration configuration, SqlConnection sqlConnection)
        {
            _configuration = configuration;
            _connection = sqlConnection;
        }

        public async Task<string> Authenticate(string username, string password)
        {
            var responseMessage = string.Empty;

            // Ensure both username and password are provided
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return "Please Enter Username and Password";
            }


            var parameters = new DynamicParameters();
            parameters.Add("@pLoginName", username);
            parameters.Add("@pPassword", password);
            parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 250); // Output parameter

            try
            {
                await _connection.ExecuteAsync("[dbo].[uspMasterLogin]", parameters, commandType: CommandType.StoredProcedure);

                responseMessage = parameters.Get<string>("@responseMessage");

                if (responseMessage == "User successfully logged in")
                {
                    // Generate the JWT token
                    var claims = new[]
                    {
                new Claim(ClaimTypes.Name, username),  // The user's login name
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique identifier for the JWT
                     };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"])); // Secret key from configuration
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Signing credentials

                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],  // Issuer of the token
                        audience: _configuration["Jwt:Audience"], // Audience for the token
                        claims: claims, // Claims associated with the token
                        expires: DateTime.Now.AddHours(1), // Token expiration time
                        signingCredentials: creds // Signing credentials
                    );

                    // Return the JWT token
                    return new JwtSecurityTokenHandler().WriteToken(token);
                }
                else
                {
                    // If authentication failed, return the failure message
                    return "Invalid login name or password"; // Authentication failed
                }
            }
            catch (Exception ex)
            {
                // Return any errors that occurred during the process
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<bool> ValidateUserByUserInput(string userName)
        {
            try
            {
                using (var conn = new SqlConnection(_connection.ConnectionString))
                {
                    string query = @"
                SELECT 1
                FROM USER_MST
                WHERE 
                    LOGIN_NAME = @userName OR 
                    EMAIL_ID_OFFICIAL = @userName OR 
                    CAST(CONTACT_NO AS NVARCHAR) = @userName";

                    var result = await conn.ExecuteScalarAsync<int?>(query, new { userName });
                    return result.HasValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> GetUserMasterTokenByUserInputAsync(string userName)
        {
            try
            {
                using (var conn = new SqlConnection(_connection.ConnectionString))
                {
                    string query = @"
                SELECT CONVERT(NVARCHAR(MAX), LOGIN_PASSWORD, 1)
                FROM USER_MST
                WHERE 
                    LOGIN_NAME = @userName 
                    OR EMAIL_ID_OFFICIAL = @userName 
                    OR CAST(CONTACT_NO AS NVARCHAR) = @userName";

                    var result = await conn.ExecuteScalarAsync<string>(query, new { userName });
                    if (!string.IsNullOrEmpty(result))
                    {
                        var claims = new[]
                    {
                new Claim(ClaimTypes.Name, userName),  // The user's login name
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique identifier for the JWT
                     };
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"])); // Secret key from configuration
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Signing credentials

                        var token = new JwtSecurityToken(
                            issuer: _configuration["Jwt:Issuer"],  // Issuer of the token
                            audience: _configuration["Jwt:Audience"], // Audience for the token
                            claims: claims, // Claims associated with the token
                            expires: DateTime.Now.AddHours(1), // Token expiration time
                            signingCredentials: creds // Signing credentials
                        );

                        // Return the JWT token
                        return new JwtSecurityTokenHandler().WriteToken(token);

                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        //public async Task<int> GetUserIdbyUserName(string userName)
        //{
        //    var cmd = new SqlCommand("SELECT MKEY FROM USER_MST WHERE [LOGIN_NAME] = @UserName OR [EMAIL_ID_OFFICIAL] =@UserName", _connection);
        //    cmd.Parameters.AddWithValue("@UserName", userName);
        //    if (_connection.State != ConnectionState.Open)
        //        await _connection.OpenAsync();
        //    var userId = (int)await cmd.ExecuteScalarAsync();
        //    if (userId == null) throw new Exception("User not found");

        //    return userId;
        //}

        public async Task<int> GetUserIdbyUserName(string userName)
        {
            var cmd = new SqlCommand("SELECT UserID FROM [User] WHERE [LoginName] = @UserName", _connection);
            cmd.Parameters.AddWithValue("@UserName", userName);
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
            var userId = (int?)await cmd.ExecuteScalarAsync();
            if (userId == null) throw new Exception("User not found");

            return userId.Value;
        }

        // Single SignOn Login By Email Services 

        public async Task<bool>GetSingleSignOnLogin(string userInput)
        {
            try
            {
                using (var conn = new SqlConnection(_connection.ConnectionString))
                {
                    string query = @"
                SELECT 1
                FROM USER_MST
                WHERE 
                    EMAIL_ID_OFFICIAL = @userInput";
                    var result = await conn.ExecuteScalarAsync<int?>(query, new { userInput});
                    return result.HasValue;
                }

            }
            catch(Exception ex)
            {
                throw;
            }
        }

        //public async Task<string> LoginRegistration(UserLoginModel userLogin)
        //{
        //    try
        //    {
        //        if (userLogin == null)
        //        {
        //            return "UserLogin is Empty";
        //        }
        //        var keyString = _configuration["EncryptionKey"]; // Retrieve the key from the configuration
        //                                                         //var EncryptedHashPassword = EncryptPassword(userLogin.PasswordHash, keyString);
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@pLogin", userLogin.LoginName);
        //        parameters.Add("@pPassword", userLogin.PasswordHash); // Assuming this is already hashed or encrypted as needed
        //        parameters.Add("@pFirstName", userLogin.FirstName);
        //        parameters.Add("@pLastName", userLogin.LastName);
        //        parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        await _connection.ExecuteAsync("dbo.uspAddUser", parameters, commandType: CommandType.StoredProcedure);
        //        string responseMessage = parameters.Get<string>("@responseMessage");
        //        if (!string.IsNullOrEmpty(responseMessage))
        //        {
        //            return "User inserted successfully!";
        //        }
        //        else
        //        {
        //            return "User registration failed.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"An error occurred: {ex.Message}";
        //    }

        //}

        public byte[] UserEncryptedReponsone(UserModel userModel, string keyString)
        {
            try
            {
                string combined = userModel.Username + ":" + userModel.Password; // Combine username and password with a separator (e.g., colon)

                byte[] key = GetKey(keyString, 32); // AES-256 requires a 32-byte key

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = new byte[16]; // Zeroed IV (not recommended for production)

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(combined); // Write combined username and password to be encrypted
                            }
                        }
                        return msEncrypt.ToArray(); // Return the encrypted data as byte array
                    }
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public byte[] EncryptedInputbuUser(string userInput, string keyString)
        {
            try
            {
                string combined = userInput; // Combine username and password with a separator (e.g., colon)

                byte[] key = GetKey(keyString, 32); // AES-256 requires a 32-byte key

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = new byte[16]; // Zeroed IV (not recommended for production)

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(combined); // Write combined username and password to be encrypted
                            }
                        }
                        return msEncrypt.ToArray(); // Return the encrypted data as byte array
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Encryption failed", ex);
            }
        }

        // Decrypted Password 

        public string DecryptPassword(string encryptedPassword, string keyString)
        {
            byte[] key = GetKey(keyString, 32); // Ensure the key is 32 bytes (AES-256)

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key; // Set the AES key
                aesAlg.IV = new byte[16]; // Initialization Vector, set to 0 for simplicity (NOT recommended for production)

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedPassword)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd(); // Return the decrypted string
                        }
                    }
                }
            }
        }

        private static byte[] GetKey(string keyString, int requiredLength)
        {
            // Truncate or pad the key to the required length (AES-128 = 16 bytes, AES-192 = 24 bytes, AES-256 = 32 bytes)
            byte[] key = Encoding.UTF8.GetBytes(keyString);

            if (key.Length < requiredLength)
            {
                Array.Resize(ref key, requiredLength); // Pad with zeros if it's too short
            }
            else if (key.Length > requiredLength)
            {
                Array.Resize(ref key, requiredLength); // Truncate if it's too long
            }

            return key;
        }

        public async Task<string> VerifyingResponse(string userLogin, string Password)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@pLoginName", userLogin);
                parameters.Add("@pPassword", Password);
                parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 250); // Output parameter

                await _connection.ExecuteAsync("[dbo].[uspVerifyingUserMasterLogin]", parameters, commandType: CommandType.StoredProcedure);

                string responseMessage = parameters.Get<string>("@responseMessage");

                if (responseMessage == "User successfully logged in")
                {
                    return responseMessage;
                }
                else
                {
                    return responseMessage;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> LoginRegistration(UserLoginModel userLogin)
        {
            try
            {
                if (userLogin == null)
                {
                    return "UserLogin is Empty";
                }
                var keyString = _configuration["EncryptionKey"]; // Retrieve the key from the configuration
                                                                 //var EncryptedHashPassword = EncryptPassword(userLogin.PasswordHash, keyString);
                var parameters = new DynamicParameters();
                parameters.Add("@pLogin", userLogin.LoginName);
                parameters.Add("@pPassword", userLogin.PasswordHash); // Assuming this is already hashed or encrypted as needed
                parameters.Add("@pFirstName", userLogin.FirstName);
                parameters.Add("@pLastName", userLogin.LastName);
                parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                await _connection.ExecuteAsync("dbo.uspAddUser", parameters, commandType: CommandType.StoredProcedure);
                string responseMessage = parameters.Get<string>("@responseMessage");
                if (!string.IsNullOrEmpty(responseMessage))
                {
                    return "User inserted successfully!";
                }
                else
                {
                    return "User registration failed.";
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<User_MasterModel> GetUserMasterDetails(string userEmail)
        {
            try
            {
                using (var conn = new SqlConnection(_connection.ConnectionString))
                {
                    await conn.OpenAsync();

                    string query = @"
                                  SELECT *
                                  FROM USER_MST
                                  WHERE EMAIL_ID_OFFICIAL = @Value
                                     OR USER_FULL_NAME = @Value";
                    try
                    {
                        var user = await conn.QueryFirstOrDefaultAsync<User_MasterModel>(
                        query,
                        new { Value = userEmail }
                    );

                        return user; // Will be null if no match
                    }
                    catch(Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public byte[] UserMasterEncryptedReponsone(UserMasterLoginModel userMasterModel, string keyString)
        {
            try
            {
                string combined = userMasterModel.UserId + ":" + userMasterModel.UserName; // Combine username and password with a separator (e.g., colon)

                byte[] key = GetKey(keyString, 32); // AES-256 requires a 32-byte key

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = new byte[16]; // Zeroed IV (not recommended for production)

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(combined); // Write combined username and password to be encrypted
                            }
                        }
                        return msEncrypt.ToArray(); // Return the encrypted data as byte array
                    }
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetDataFromSpAsync<T>(string storedProcedureName, CommonSpParameters parametersModel)
        {
            try
            {
                using (var conn = new SqlConnection(_connection.ConnectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", parametersModel.UserId);
                    parameters.Add("@BusinessGroupId", parametersModel.BusinessGroupId);
                    parameters.Add("@Attribute1", parametersModel.Attribute1);
                    parameters.Add("@Attribute2", parametersModel.Attribute2);
                    parameters.Add("@Attribute3", parametersModel.Attribute3);
                    parameters.Add("@Attribute4", parametersModel.Attribute4);

                    return await conn.QueryAsync<T>(
                        storedProcedureName,
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        #region
        // Change Password and Forget Password


        public async Task<IEnumerable<ResetPasswordOutPut_List>> GetResetPasswordAsync(string TEMPPASSWORD, string LoginName)
        {
            try
            {
                string strMessage = string.Empty;
                string? strEmailResponse = string.Empty;
                int ErrorNumber = 0;
                const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";
                StringBuilder password = new StringBuilder();
                Random random = new Random();

                for (int i = 0; i < 10; i++)
                {
                    password.Append(validChars[random.Next(validChars.Length)]);
                }
                TEMPPASSWORD = password.ToString();

                if (_connection.State == ConnectionState.Closed)
                    await _connection.OpenAsync();


                //using (IDbConnection db = _dapperDbConnection.CreateConnection())
                //{
                    var parmeters = new DynamicParameters();
                    parmeters.Add("@TEMPPASSWORD", TEMPPASSWORD);
                    parmeters.Add("@LoginName", LoginName);
                    var ResetPass = await _connection.QueryAsync<ResetPasswordOutPut>("sp_reset_password", parmeters, commandType: CommandType.StoredProcedure);

                    foreach (var ResetResponse in ResetPass)
                    {
                        ErrorNumber = ResetResponse.ErrorNumber;
                        strMessage = ResetResponse.Message;
                    }

                    if (ErrorNumber == 0)
                    {
                        var AssiLoginName = await _connection.QueryAsync<string>(" Select UPPER(LEFT(FIRST_NAME,1))+LOWER(SUBSTRING(FIRST_NAME,2,LEN(FIRST_NAME))) + ' '+ " +
                                   " UPPER(LEFT(LAST_NAME,1))+LOWER(SUBSTRING(LAST_NAME,2,LEN(LAST_NAME))) as EMP_FULL_NAME " +
                                   " from USER_MST US_MST where " +
                                   " (US_MST.EMAIL_ID_OFFICIAL = '" + LoginName + "' " +
                                   " or Cast(US_MST.CONTACT_NO As nVarchar(20))= '" + LoginName + "')  " +
                                   " and US_MST.DELETE_FLAG='N' ", commandType: CommandType.Text);
                        string AssignBy = AssiLoginName.FirstOrDefault();
                        var parmetersMail = new DynamicParameters();
                        parmetersMail.Add("@MAIL_TYPE", "Auto");
                        var MailDetails = await _connection.QueryAsync<MailDetailsNT>("SP_GET_MAIL_TYPE", parmetersMail, commandType: CommandType.StoredProcedure);

                        string MailBody = "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    " +
                            "<meta charset=\"UTF-8\">\r\n    " +
                            "<title>Bank Insight Password Reset</title>\r\n</head>" +
                            "\r\n<body style=\"font-family: Arial, sans-serif; font-size: 14px; color: #333;\">\r\n    " +
                            "<p>Dear <strong>" + AssignBy + " </strong>,</p>\r\n\r\n    " +
                            "<p>Your password for <strong>Bank Insight</strong> has been successfully reset.</p>\r\n\r\n    " +
                            "<p>Your temporary password is: <strong style=\"color: #d9534f;\">" + TEMPPASSWORD.ToString() + "</strong></p>\r\n\r\n    " +
                            "<p>Please log in to <strong><a href=\"https://qui.piplapps.com\">QUI</a></strong> using this password and update it immediately for security reasons.</p>\r\n\r\n    " +
                            "<p>If you have any questions or need assistance, feel free to contact us at \r\n       " +
                            " <a href=\"mailto:qui.support@powersoft.in\">qui.support@powersoft.in</a>.\r\n    " +
                            "</p>\r\n\r\n    <p>Best regards,<br>\r\n    " +
                            "<strong>Bank Insight Team</strong></p>\r\n</body>\r\n</html>\r\n";

                             foreach (var Mail in MailDetails)
                             {
                                 try
                                 {
                                     strEmailResponse = SendEmail(LoginName, null, null,
                                         "Bank Insight - Your Temporary Password",
                                         MailBody, Mail.MAIL_TYPE, "Bank Insight", null, Mail);
                                 }
                                 catch (Exception ex)
                                 {
                                     return new List<ResetPasswordOutPut_List>
                                     { 
                                         new ResetPasswordOutPut_List { Status = "Error",Message = "Email sending failed: " + ex.Message,Data = null}
                                     };
                                 }

                             }
                             if (!string.IsNullOrEmpty(strEmailResponse) && strEmailResponse.Contains("Sent Email"))
                             {
                                 return new List<ResetPasswordOutPut_List>
                                 {
                                     new ResetPasswordOutPut_List
                                     {
                                         Status = "Ok",
                                         Message = strMessage,
                                         Data = ResetPass
                                     }
                                 };
                             }
                             else
                             {
                                 return new List<ResetPasswordOutPut_List>
                                 {
                                     new ResetPasswordOutPut_List
                                     {
                                         Status = "Error",
                                         Message = "Password reset succeeded but email sending failed.",
                                         Data = ResetPass
                                     }
                                 };
                             }

                    }
                    else
                    {
                        var successsResult = new List<ResetPasswordOutPut_List>
                     {
                         new ResetPasswordOutPut_List
                         {
                             Status = "Error",
                             Message = strMessage,
                             Data= null
                         }
                     };
                        return successsResult;
                    }
               // }
            }
            catch (Exception ex)
            {
                var errorResult = new List<ResetPasswordOutPut_List>
             {
                 new ResetPasswordOutPut_List
                 {
                    Status = "Error",
                     Message= ex.Message,
                     Data= null
                 }
             };
                return errorResult;
            }
        }

        public async Task<IEnumerable<ForgotPasswordOutPut_List>> GetForgotPasswordAsync(string LoginName)
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    await _connection.OpenAsync();

                int ErrorNumber = 0;
                string ResponseMeaage = string.Empty;
                var parmeters = new DynamicParameters();
                parmeters.Add("@LoginName", LoginName);

                //parmeters.Add("@LoginName", LoginName);
                var ForgotPass = await _connection.QueryAsync<ForgotPasswordOutPut>("Sp_USER_ForgotPassword", parmeters, commandType: CommandType.StoredProcedure);
                foreach (var SuccessMsg in ForgotPass)
                {
                    ErrorNumber = SuccessMsg.ErrorNumber;
                    ResponseMeaage = SuccessMsg.MessageText;
                }
                if (ErrorNumber != 1)
                {
                    var ErrorResult = new List<ForgotPasswordOutPut_List>
                 {
                     new ForgotPasswordOutPut_List
                     {
                         Status = "Error",
                         Message = ResponseMeaage,
                         Data= ForgotPass
                     }
                 };
                    return ErrorResult;
                }
                else
                {
                    var successsResult = new List<ForgotPasswordOutPut_List>
                 {
                     new ForgotPasswordOutPut_List
                     {
                         Status = "Ok",
                         Message = "Message",
                         Data= ForgotPass
                     }
                 };
                    return successsResult;
                }

                //using (IDbConnection db = _connection.CreateConnection())
                //{
                //    int ErrorNumber = 0;
                //    string ResponseMeaage = string.Empty;
                //    var parmeters = new DynamicParameters();
                //    parmeters.Add("@LoginName", LoginName);
                //    var ForgotPass = await db.QueryAsync<ForgotPasswordOutPut>("Sp_USER_ForgotPassword", parmeters, commandType: CommandType.StoredProcedure);
                //    foreach (var SuccessMsg in ForgotPass)
                //    {
                //        ErrorNumber = SuccessMsg.ErrorNumber;
                //        ResponseMeaage = SuccessMsg.MessageText;
                //    }
                //    if (ErrorNumber != 1)
                //    {
                //        var ErrorResult = new List<ForgotPasswordOutPut_List>
                // {
                //     new ForgotPasswordOutPut_List
                //     {
                //         Status = "Error",
                //         Message = ResponseMeaage,
                //         Data= ForgotPass
                //     }
                // };
                //        return ErrorResult;
                //    }
                //    else
                //    {
                //        var successsResult = new List<ForgotPasswordOutPut_List>
                // {
                //     new ForgotPasswordOutPut_List
                //     {
                //         Status = "Ok",
                //         Message = "Message",
                //         Data= ForgotPass
                //     }
                // };
                //        return successsResult;
                //    }
                //}
            }
            catch (Exception ex)
            {
                var errorResult = new List<ForgotPasswordOutPut_List>
             {
                 new ForgotPasswordOutPut_List
                 {
                    Status = "Error",
                     Message= ex.Message,
                     Data= null
                 }
             };
                return errorResult;
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    await _connection.CloseAsync();
            }
        }


        public async Task<IEnumerable<PutChangePasswordOutPutNT>> PostChangePasswordAsync(ChangePasswordInputNT changePasswordInput)
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    await _connection.OpenAsync();

                   var parameters = new DynamicParameters();
                   parameters.Add("@LoginName", changePasswordInput.LoginName);
                   parameters.Add("@Old_LOGIN_PASSWORD", changePasswordInput.Old_Password);
                   parameters.Add("@New_LOGIN_PASSWORD", changePasswordInput.New_Password);
                   parameters.Add("@Session_User_Id", changePasswordInput.Session_User_ID);
                   parameters.Add("@Business_Group_Id", changePasswordInput.Business_Group_ID);
                   
                   var changePass = await _connection.QueryAsync<PutChangePasswordNT>("Sp_USER_ChangeLOGIN_PASSWORD_NT",parameters,commandType: CommandType.StoredProcedure);
                   var successResult = new List<PutChangePasswordOutPutNT>
                       {
                           new PutChangePasswordOutPutNT
                           {
                               Status = "Ok",
                               Message = "Password updated successfully",
                               Data = changePass
                           }
                   };
                return successResult;
            }
            catch (Exception ex)
            {
                var errorResult = new List<PutChangePasswordOutPutNT>
               {
                   new PutChangePasswordOutPutNT
                   {
                      Status = "Error",
                       Message= ex.Message,
                       Data= null
                   }
               };
                return errorResult;
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    await _connection.CloseAsync();
            }
        }

        public string SendEmail(string sp_to, string sp_cc, string sp_bcc, string sp_subject, string sp_body, string sp_mailtype, string sp_display_name, List<string> lp_attachment, MailDetailsNT mailDetailsNT)
        {
            string strerror = string.Empty;
            try
            {
                using (MailMessage mail1 = new MailMessage())
                {
                    mail1.From = new System.Net.Mail.MailAddress(mailDetailsNT.MAIL_FROM, sp_display_name.ToUpper());//, sp_display_name == "" ? dt.Rows[0]["MAIL_DISPLAY_NAME"].ToString() : sp_display_name
                    //mail1.To.Add("narendrakumar.soni@powersoft.in");
                    foreach (var to_address in sp_to.Replace(",", ";").Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mail1.To.Add(new MailAddress(to_address));
                        //mail1.To.Add(new MailAddress("narendrakumar.soni@powersoft.in"));
                        //mail.To.Add("ashish.tripathi@powersoft.in");
                        //mail.CC.Add("brijesh.tiwari@powersoft.in");
                    }
                    if (sp_cc != null)
                        foreach (var cc_address in sp_cc.Replace(",", ";").Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            mail1.CC.Add(new MailAddress(cc_address));
                            // mail.CC.Add("brijesh.tiwari@powersoft.in");
                        }
                    if (sp_bcc != null)
                        foreach (var bcc_address in sp_bcc.Replace(",", ";").Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            mail1.Bcc.Add(new MailAddress(bcc_address));
                        }

                    mail1.Subject = sp_subject;
                    mail1.Body = sp_body;
                    mail1.IsBodyHtml = true;
                    //mail1.Attachments.Add(new Attachment("C:\\file.zip"));

                    using (SmtpClient smtp1 = new SmtpClient(mailDetailsNT.SMTP_HOST.ToString(), Convert.ToInt32(mailDetailsNT.SMTP_PORT)))
                    {
                        smtp1.Credentials = new NetworkCredential(mailDetailsNT.MAIL_FROM, mailDetailsNT.SMTP_PASS.ToString());
                        //new NetworkCredential("autosupport@powersoft.in", "yivz qklg jsbv ttso");
                        smtp1.EnableSsl = mailDetailsNT.SMTP_ESSL.ToString() == "true" ? true : false;

                        if (lp_attachment != null)
                            foreach (var attach in lp_attachment)
                            {
                                mail1.Attachments.Add(new Attachment(attach));
                            }

                        smtp1.Send(mail1);
                    }
                    foreach (Attachment attachment in mail1.Attachments)
                    {
                        attachment.Dispose();
                    }

                }

                strerror = "Sent Email";
                return strerror;

                /*MailMessage mail = new MailMessage();


                foreach (var to_address in sp_to.Replace(",", ";").Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    // mail.To.Add(new MailAddress(to_address));
                    mail.To.Add(new MailAddress("narendrakumar.soni@powersoft.in"));
                    //mail.To.Add("ashish.tripathi@powersoft.in");
                    //mail.CC.Add("brijesh.tiwari@powersoft.in");
                }
                if (sp_cc != null)
                    foreach (var cc_address in sp_cc.Replace(",", ";").Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mail.CC.Add(new MailAddress(cc_address));
                        // mail.CC.Add("brijesh.tiwari@powersoft.in");
                    }
                if (sp_bcc != null)
                    foreach (var bcc_address in sp_bcc.Replace(",", ";").Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mail.Bcc.Add(new MailAddress(bcc_address));
                    }

                mail.Subject = sp_subject;
                //mail.From = new System.Net.Mail.MailAddress(mailDetailsNT.MAIL_FROM, sp_display_name);//, sp_display_name == "" ? dt.Rows[0]["MAIL_DISPLAY_NAME"].ToString() : sp_display_name
                mail.From = new System.Net.Mail.MailAddress("autosupport@powersoft.in");//, sp_display_name == "" ? dt.Rows[0]["MAIL_DISPLAY_NAME"].ToString() : sp_display_name
                SmtpClient smtp = new SmtpClient();
                smtp.Timeout = Convert.ToInt32(mailDetailsNT.SMTP_TIMEOUT);
                smtp.Port = Convert.ToInt32(mailDetailsNT.SMTP_PORT);
                smtp.UseDefaultCredentials = true;
                smtp.Host = mailDetailsNT.SMTP_HOST.ToString();
//                sc.Credentials = basicAuthenticationInfo;
                smtp.Credentials = new NetworkCredential("autosupport@powersoft.in", "yivz qklg jsbv ttso");
                smtp.EnableSsl = mailDetailsNT.SMTP_ESSL.ToString() == "true" ? true : false;
                mail.IsBodyHtml = true;
                mail.Body = sp_body;
                if (lp_attachment != null)
                    foreach (var attach in lp_attachment)
                    {
                        mail.Attachments.Add(new Attachment(attach));
                    }
                smtp.Send(mail);*/


            }
            catch (Exception ex)
            {
               // string FileName = string.Empty;
                //string strFolder = string.Empty;

                //strFolder = _fileSettings.FilePath; // "D:\\Application\\TaskDeployment" + "\\ErrorFolder";
                //if (!Directory.Exists(strFolder))
                //{
                //    Directory.CreateDirectory(strFolder);
                //}

                //if (File.Exists(strFolder + "\\ErrorLog.txt") == false)
                //{
                //    using (System.IO.StreamWriter sw = File.CreateText(strFolder + "\\ErrorLog.txt"))
                //    {
                //        sw.Write("\n");
                //        sw.WriteLine("--------------------------------------------------------------" + "\n");
                //        sw.WriteLine(System.DateTime.Now);
                //        sw.WriteLine(FileName + "--> " + ex.Message.ToString() + "\n");
                //        sw.WriteLine("--------------------------------------------------------------" + "\n");
                //    }
                //}
                //else
                //{
                //    using (System.IO.StreamWriter sw = File.AppendText(strFolder + "\\ErrorLog.txt"))
                //    {
                //        sw.Write("\n");
                //        sw.WriteLine("--------------------------------------------------------------" + "\n");
                //        sw.WriteLine(System.DateTime.Now);
                //        sw.WriteLine(FileName + "--> " + ex.Message.ToString() + "\n");
                //        sw.WriteLine("--------------------------------------------------------------" + "\n");
                //    }
                //}

                strerror = "Error Sending Email : " + ex.Message;
                return strerror;
            }
        }

        public async Task<string> InsertBankUserDetailsListAsync(List<bankUserDetails_HDR> userList)
        {
            if (userList == null || userList.Count == 0)
                return "No records to insert.";

            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"))) // assuming _connection is SqlConnection
                {
                    //if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();
                    foreach (var user in userList)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@LegalEntityId", user.LegalEntityId);
                        parameters.Add("@LegalEntityName", user.LegalEntityName);
                        parameters.Add("@ProjectId", user.ProjectId);
                        parameters.Add("@ProjectName", user.ProjectName);
                        parameters.Add("@BuildingId", user.BuildingId);
                        parameters.Add("@BuildingName", user.BuildingName);
                        parameters.Add("@AccountId", user.AccountId);
                        parameters.Add("@AccountName", user.AccountName);
                        parameters.Add("@AccountNo", user.AccountNo);
                        parameters.Add("@IFSCCode", user.IFSCCode);
                        parameters.Add("@BankId", user.BankId);
                        parameters.Add("@BankName", user.BankName);
                        parameters.Add("@BranchName", user.BranchName);
                        parameters.Add("@AccountType", user.AccountType);
                        parameters.Add("@TagType", user.TagType);
                        parameters.Add("@CustId", user.CustId);
                        parameters.Add("@LastBalance", user.LastBalance);
                        parameters.Add("@unclearFunds", user.unclearFunds);
                        parameters.Add("@netBalance", user.netBalance);
                        parameters.Add("@balAvailable", user.balAvailable);
                        parameters.Add("@holdAmount", user.holdAmount);
                        parameters.Add("@overdraft", user.overdraft);
                        parameters.Add("@customerName", user.customerName);
                        parameters.Add("@Contact_Person", user.Contact_Person);
                        parameters.Add("@Contact_No", user.Contact_No);
                        parameters.Add("@GST_Number", user.GST_Number);
                        parameters.Add("@CREATED_BY", user.CREATED_BY);
                        parameters.Add("@CREATION_DATE", user.CREATION_DATE);
                        parameters.Add("@DELETE_FLAG", user.DELETE_FLAG);
                        parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        await connection.ExecuteAsync("dbo.uspInsertBankUserDetails", parameters, commandType: CommandType.StoredProcedure);
                        string response = parameters.Get<string>("@responseMessage");
                        Console.WriteLine($"✅ Inserted: {response}");
                    }
                    await connection.CloseAsync();
                }
                return "All records inserted successfully.";
            }
            catch (Exception ex)
            {
                return $"❌ Error inserting data: {ex.Message}";
            }
        }
        public async Task<string> InsertBankAccountDetailsAsync(List<BankAccountDetails> accountList)
        {
            if (accountList == null || accountList.Count == 0)
                return "No records to insert.";
            try
            {
                using (var connection = _connection) // Assuming _connection is SqlConnection
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    foreach (var acc in accountList)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@acctNumber", acc.acctNumber);
                        parameters.Add("@branchCode", acc.branchCode);
                        parameters.Add("@AuthorizationVal", acc.AuthorizationVal);
                        parameters.Add("@customerID", acc.customerID);
                        parameters.Add("@keyVal", acc.keyVal);
                        parameters.Add("@CompanyName", acc.CompanyName);
                        parameters.Add("@CompanyId", acc.CompanyId);
                        parameters.Add("@ProjectName", acc.ProjectName);
                        parameters.Add("@ProjectId", acc.ProjectId);
                        parameters.Add("@BuildingName", acc.BuildingName);
                        parameters.Add("@BuildingId", acc.BuildingId);
                        parameters.Add("@ATTRIBUTE1", acc.ATTRIBUTE1);
                        parameters.Add("@ATTRIBUTE2", acc.ATTRIBUTE2);
                        parameters.Add("@ATTRIBUTE3", acc.ATTRIBUTE3);
                        parameters.Add("@ATTRIBUTE4", acc.ATTRIBUTE4);
                        parameters.Add("@ATTRIBUTE5", acc.ATTRIBUTE5);
                        parameters.Add("@CREATED_BY", acc.CREATED_BY);
                        parameters.Add("@CREATED_BY_Name", acc.CREATED_BY_Name);
                        parameters.Add("@CREATION_DATE", acc.CREATION_DATE);
                        parameters.Add("@LAST_UPDATED_BY", acc.LAST_UPDATED_BY);
                        parameters.Add("@LAST_UPDATED_BY_Name", acc.LAST_UPDATED_BY_Name);
                        parameters.Add("@LAST_UPDATE_DATE", acc.LAST_UPDATE_DATE);
                        parameters.Add("@DELETE_FLAG", acc.DELETE_FLAG);
                        parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        await connection.ExecuteAsync("dbo.uspInsertBankAccountDetails", parameters, commandType: CommandType.StoredProcedure);
                        string response = parameters.Get<string>("@responseMessage");
                        Console.WriteLine($"✅ Inserted: {response}");
                    }
                    await connection.CloseAsync();
                    return "All records inserted successfully.";
                }
            }
            catch (Exception ex)
            {
                return $"❌ Error inserting data: {ex.Message}";
            }
        }
        #endregion

        #region
        //// History Insert Method 

        //public async Task<string> Insertbank_Details_Hdr_HistoryAsync(Bank_Details_Hdr_H details)
        //{
        //    if (details == null)
        //        return "No records to insert.";

        //    try
        //    {
        //        using (var connection = _connection)
        //        {
        //            if (connection.State == ConnectionState.Closed)
        //                connection.Open();

        //            var parameters = new DynamicParameters();
        //            parameters.Add("@HISTSEQ_NO", details.HISTSEQ_NO);
        //            parameters.Add("@HIST_DATE", DateTime.UtcNow);
        //            parameters.Add("@Mkey", details.Mkey);
        //            parameters.Add("@EntryDateTime", details.EntryDateTime);
        //            parameters.Add("@LegalEntityId", details.LegalEntityId);
        //            parameters.Add("@LegalEntityName", details.LegalEntityName);
        //            parameters.Add("@ProjectId", details.ProjectId);
        //            parameters.Add("@ProjectName", details.ProjectName);
        //            parameters.Add("@BuildingId", details.BuildingId);
        //            parameters.Add("@BuildingName", details.BuildingName);
        //            parameters.Add("@AccountId", details.AccountId);
        //            parameters.Add("@AccountName", details.AccountName);
        //            parameters.Add("@AccountNo", details.AccountNo);
        //            parameters.Add("@IFSCCode", details.IFSCCode);
        //            parameters.Add("@BankId", details.BankId);
        //            parameters.Add("@BankName", details.BankName);
        //            parameters.Add("@BranchName", details.BranchName);
        //            parameters.Add("@AccountType", details.AccountType);
        //            parameters.Add("@TagType", details.TagType);
        //            parameters.Add("@CustId", details.CustId);
        //            parameters.Add("@LastBalance", details.LastBalance);
        //            parameters.Add("@unclearFunds", details.unclearFunds);
        //            parameters.Add("@netBalance", details.netBalance);
        //            parameters.Add("@balAvailable", details.balAvailable);
        //            parameters.Add("@holdAmount", details.holdAmount);
        //            parameters.Add("@overdraft", details.overdraft);
        //            parameters.Add("@customerName", details.customerName);
        //            parameters.Add("@LastTransactionDatetime", details.LastTransactionDatetime);
        //            parameters.Add("@ActiveFlag", details.ActiveFlag);
        //            parameters.Add("@Status", details.Status);
        //            parameters.Add("@Process_Flag", details.Process_Flag);
        //            parameters.Add("@Contact_Person", details.Contact_Person);
        //            parameters.Add("@Contact_No", details.Contact_No);
        //            parameters.Add("@GST_Number", details.GST_Number);
        //            parameters.Add("@ATTRIBUTE1", details.ATTRIBUTE1);
        //            parameters.Add("@ATTRIBUTE2", details.ATTRIBUTE2);
        //            parameters.Add("@ATTRIBUTE3", details.ATTRIBUTE3);
        //            parameters.Add("@ATTRIBUTE4", details.ATTRIBUTE4);
        //            parameters.Add("@ATTRIBUTE5", details.ATTRIBUTE5);
        //            parameters.Add("@CREATED_BY", details.CREATED_BY);
        //            parameters.Add("@CREATION_DATE", details.CREATION_DATE);
        //            parameters.Add("@LAST_UPDATED_BY", details.LAST_UPDATED_BY);
        //            parameters.Add("@LAST_UPDATE_DATE", details.LAST_UPDATE_DATE);
        //            parameters.Add("@DELETE_FLAG", details.DELETE_FLAG);
        //            parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            await connection.ExecuteAsync("dbo.usp_Insertbank_Details_Hdr_H", parameters, commandType: CommandType.StoredProcedure);
        //            string response = parameters.Get<string>("@responseMessage");
        //            return response;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"❌ Error inserting data: {ex.Message}";
        //    }
        //}

        //public async Task<string> InsertBankAccountDetails_HistoryAsync(Bank_Acc_Details_H details)
        //{
        //    if (details == null)
        //        return "No record to insert.";

        //    try
        //    {
        //        using (var connection = _connection) // Assuming _connection is a SqlConnection
        //        {
        //            if (connection.State == ConnectionState.Closed)
        //                connection.Open();

        //            var parameters = new DynamicParameters();
        //            parameters.Add("@HISTSEQ_NO", details.HISTSEQ_NO);
        //            parameters.Add("@HIST_DATE", DateTime.UtcNow);
        //            parameters.Add("@Mkey", details.Mkey);
        //            parameters.Add("@acctNumber", details.acctNumber);
        //            parameters.Add("@branchCode", details.branchCode);
        //            parameters.Add("@AuthorizationVal", details.AuthorizationVal);
        //            parameters.Add("@customerID", details.customerID);
        //            parameters.Add("@keyVal", details.keyVal);
        //            parameters.Add("@CompanyName", details.CompanyName);
        //            parameters.Add("@CompanyId", details.CompanyId);
        //            parameters.Add("@ProjectName", details.ProjectName);
        //            parameters.Add("@ProjectId", details.ProjectId);
        //            parameters.Add("@BuildingName", details.BuildingName);
        //            parameters.Add("@BuildingId", details.BuildingId);
        //            parameters.Add("@ATTRIBUTE1", details.ATTRIBUTE1);
        //            parameters.Add("@ATTRIBUTE2", details.ATTRIBUTE2);
        //            parameters.Add("@ATTRIBUTE3", details.ATTRIBUTE3);
        //            parameters.Add("@ATTRIBUTE4", details.ATTRIBUTE4);
        //            parameters.Add("@ATTRIBUTE5", details.ATTRIBUTE5);
        //            parameters.Add("@CREATED_BY", details.CREATED_BY);
        //            parameters.Add("@CREATED_BY_Name", details.CREATED_BY_Name);
        //            parameters.Add("@CREATION_DATE", details.CREATION_DATE);
        //            parameters.Add("@LAST_UPDATED_BY", details.LAST_UPDATED_BY);
        //            parameters.Add("@LAST_UPDATED_BY_Name", details.LAST_UPDATED_BY_Name);
        //            parameters.Add("@LAST_UPDATE_DATE", details.LAST_UPDATE_DATE);
        //            parameters.Add("@DELETE_FLAG", details.DELETE_FLAG);

        //            parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

        //            await connection.ExecuteAsync("dbo.uspInsertBank_Acc_Details_H", parameters, commandType: CommandType.StoredProcedure);

        //            string response = parameters.Get<string>("@responseMessage");
        //            await connection.CloseAsync();

        //            return response;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"❌ Error inserting data: {ex.Message}";
        //    }
        //}

        //// End History Method 
        #endregion
        //public async Task<string> InsertILogResponse(LogResponseObject logResponseObject)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(logResponseObject.DELETE_FLAG.ToString()) || logResponseObject.DELETE_FLAG == '\0')
        //        {
        //            logResponseObject.DELETE_FLAG = 'N';
        //        }
        //        string storedProcedureName;
        //        storedProcedureName = "[dbo].[UspAddPaymentlogDetails]";
        //        // Create DynamicParameters and add the properties of LogResponseObject
        //        var parameters = new DynamicParameters();
        //        //parameters.Add("@Mkey", logResponseObject.Mkey);  // logResponseObject.Mkey
        //        parameters.Add("@Status", logResponseObject.Status);
        //        parameters.Add("@Message ", logResponseObject.Message);
        //        parameters.Add("@ActionName", logResponseObject.Action_Name);
        //        parameters.Add("@MethodName ", logResponseObject.Method_Name);
        //        parameters.Add("@ATTRIBUTE1 ", logResponseObject.ATTRIBUTE1);
        //        parameters.Add("@ATTRIBUTE2  ", logResponseObject.ATTRIBUTE2);
        //        parameters.Add("@ATTRIBUTE3  ", logResponseObject.ATTRIBUTE3);
        //        parameters.Add("@ATTRIBUTE4  ", logResponseObject.ATTRIBUTE4);
        //        parameters.Add("@ATTRIBUTE5  ", logResponseObject.ATTRIBUT5);
        //        parameters.Add("@CREATION_DATE", logResponseObject.Created_Date);
        //        parameters.Add("@CREATED_BY", logResponseObject.CREATED_BY);
        //        parameters.Add("@LAST_UPDATED_BY", logResponseObject.LAST_UPDATED_BY);
        //        parameters.Add("@LAST_UPDATE_DATE", logResponseObject.LAST_UPDATE_DATE);
        //        parameters.Add("@DELETE_FLAG", logResponseObject.DELETE_FLAG);
        //        parameters.Add("@Payment_Status", logResponseObject.Payment_Status);
        //        parameters.Add("@CF_Payment_ID", logResponseObject.Cf_Payment_Id);
        //        parameters.Add("@Bank_Reference", logResponseObject.Bank_Reference);
        //        parameters.Add("@Entity", logResponseObject.Entity);
        //        parameters.Add("@Is_Captured", logResponseObject.Is_Captured);
        //        parameters.Add("@Order_Amount", logResponseObject.Order_Amount);
        //        parameters.Add("@Order_ID", logResponseObject.Order_Id);
        //        parameters.Add("@Payment_Completion_Time", logResponseObject.Payment_Completion_Time);
        //        parameters.Add("@Payment_Currency", logResponseObject.Payment_Currency);
        //        parameters.Add("@Payment_Message", logResponseObject.Payment_Message);
        //        parameters.Add("@Payment_Method", logResponseObject.Payment_Method);
        //        parameters.Add("@UPI_Channel", logResponseObject.UPI_Channel);
        //        //parameters.Add("@Channel", logResponseObject.Channel);
        //        parameters.Add("@UPI_ID", logResponseObject.Upi_Id);
        //        parameters.Add("@Payment_Time", logResponseObject.Payment_Time);
        //        parameters.Add("@CF_Order_ID", logResponseObject.Cf_Order_Id);
        //        parameters.Add("@Order_Currency", logResponseObject.Order_Currency);
        //        parameters.Add("@Order_Status", logResponseObject.Order_Status);
        //        parameters.Add("@Payment_Session_ID", logResponseObject.Payment_Session_Id);
        //        parameters.Add("@Order_Expiry_Time", logResponseObject.Order_Expiry_Time);
        //        parameters.Add("@Order_Note", logResponseObject.Order_Note);
        //        parameters.Add("@Created_At", logResponseObject.Created_At);
        //        parameters.Add("@Order_Splits", logResponseObject.Order_Splits);
        //        //parameters.Add("@Customer_Details", logResponseObject.Customer_Details);
        //        parameters.Add("@Customer_ID", logResponseObject.Customer_Id);
        //        parameters.Add("@Customer_Email", logResponseObject.Customer_Email);
        //        parameters.Add("@Customer_Phone", logResponseObject.Customer_Phone);
        //        parameters.Add("@Customer_Name", logResponseObject.Customer_Name);
        //        parameters.Add("@Customer_Bank_Account_Number", logResponseObject.Customer_Bank_Account_Number);
        //        parameters.Add("@Customer_Bank_IFSC", logResponseObject.Customer_Bank_Ifsc);
        //        parameters.Add("@Customer_Bank_Code", logResponseObject.Customer_Bank_Code);
        //        parameters.Add("@Customer_UID", logResponseObject.Customer_Uid);
        //        parameters.Add("@Order_Meta", logResponseObject.Order_Meta);
        //        parameters.Add("@Return_URL ", logResponseObject.Return_Url);
        //        parameters.Add("@Notify_URL", logResponseObject.Notify_Url);
        //        parameters.Add("@Payment_Methods", logResponseObject.Payment_Methods);
        //        parameters.Add("@Order_Tags", logResponseObject.Order_Tags);

        //        // Add new settlement fields here as well
        //        parameters.Add("@Settlement_ID", logResponseObject.Settlement_Id);
        //        parameters.Add("@Payment_ID", logResponseObject.Payment_Id);
        //        parameters.Add("@Amount_Settled", logResponseObject.Settlement_Amount);
        //        parameters.Add("@Service_Charge", logResponseObject.Service_Charge);
        //        //parameters.Add("@Payment_Time_Settlement", logResponseObject.Payment_Time_Settlement);
        //        parameters.Add("@Payment_UTR", logResponseObject.Payment_Utr);
        //        parameters.Add("@Remarks ", logResponseObject.Remarks);
        //        //parameters.Add("@Remarks_Settlement", logResponseObject.Remarks_Settlement);
        //        parameters.Add("@Adjustment", logResponseObject.Adjustment);
        //        parameters.Add("@CF_Settlement_ID", logResponseObject.Cf_Settlement_Id);
        //        parameters.Add("@Closed_in_Favor_Of", logResponseObject.Closed_In_Favor_Of);
        //        parameters.Add("@Dispute_Category", logResponseObject.Dispute_Category);
        //        parameters.Add("@Dispute_Note", logResponseObject.Dispute_Note);
        //        parameters.Add("@Dispute_Resolved_On", logResponseObject.Dispute_Resolved_On);
        //        parameters.Add("@Event_Amount", logResponseObject.Event_Amount);
        //        parameters.Add("@Event_Currency", logResponseObject.Event_Currency);
        //        parameters.Add("@Event_Id", logResponseObject.Event_Id);
        //        parameters.Add("@Event_Settlement_Amount", logResponseObject.Event_Settlement_Amount);
        //        parameters.Add("@Event_Status", logResponseObject.Event_Status);
        //        parameters.Add("@Event_Time", logResponseObject.Event_Time);
        //        parameters.Add("@Event_Type", logResponseObject.Event_Type);
        //        parameters.Add("@Payment_Amount", logResponseObject.Payment_Amount);
        //        parameters.Add("@Payment_From", logResponseObject.Payment_From);
        //        parameters.Add("@Payment_Group ", logResponseObject.Payment_Group);
        //        parameters.Add("@Payment_Service_Charge", logResponseObject.Payment_Service_Charge);
        //        parameters.Add("@Payment_Service_Tax", logResponseObject.Payment_Service_Tax);
        //        parameters.Add("@Payment_Till", logResponseObject.Payment_Till);
        //        parameters.Add("@Reason", logResponseObject.Reason);
        //        parameters.Add("@Refund_ARN ", logResponseObject.Refund_Arn);
        //        parameters.Add("@Refund_ID", logResponseObject.Refund_Id);
        //        parameters.Add("@Refund_Note", logResponseObject.Refund_Note);
        //        parameters.Add("@Refund_Processed_At", logResponseObject.Refund_Processed_At);
        //        parameters.Add("@Resolved_On", logResponseObject.Resolved_On);
        //        parameters.Add("@Sale_Type", logResponseObject.Sale_Type);
        //        parameters.Add("@Service_Tax ", logResponseObject.Service_Tax);
        //        parameters.Add("@Settlement_Charge", logResponseObject.Settlement_Charge);
        //        parameters.Add("@Settlement_Date", logResponseObject.Settlement_Date);
        //        parameters.Add("@Settlement_Initiated_On", logResponseObject.Settlement_Initiated_On);
        //        parameters.Add("@Settlement_Tax", logResponseObject.Settlement_Tax);
        //        parameters.Add("@Settlement_Type", logResponseObject.Settlement_Type);
        //        parameters.Add("@Settlement_UTR", logResponseObject.Settlement_Utr);
        //        parameters.Add("@Split_Service_Charge", logResponseObject.Split_Service_Charge);
        //        parameters.Add("@Split_Service_Tax ", logResponseObject.Split_Service_Tax);
        //        //parameters.Add("@Status_Settlement", logResponseObject.Status_Settlement);
        //        parameters.Add("@Vendor_Commission", logResponseObject.Vendor_Commission);
        //        parameters.Add("@Adjustment_Remarks ", logResponseObject.Adjustment_Remarks);
        //        parameters.Add("@ResponseMessage ", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        // Execute the stored procedure (Insert query using Dapper)
        //        var result = await _connection.ExecuteAsync(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);
        //        string responseMessage = parameters.Get<string>("@ResponseMessage ");

        //        // Adjust response message based on MKey value
        //        if (responseMessage.Contains("Success"))
        //        {
        //            // If MKey is 0 (Add), return "Success"
        //            return responseMessage.Contains("Success") ? "Success" : "No rows added";
        //        }
        //        else
        //        {
        //            // If MKey is not 0 (Update), return "Update Success"
        //            return responseMessage.Contains("Success") ? "Update Success" : "No rows updated";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log exception (for your reference or debugging)
        //        return $"Error: {ex.Message}";
        //    }
        //}

        #region
        //public async  Task<string> HashPasswordSHA256(string password)
        //{
        //    using (SHA256 sha256Hash = SHA256.Create())
        //    {
        //        // Compute the hash from the password string
        //        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

        //        // Convert the byte array to a hexadecimal string
        //        StringBuilder builder = new StringBuilder();
        //        foreach (var byteValue in bytes)
        //        {
        //            builder.Append(byteValue.ToString("x2"));
        //        }
        //        return builder.ToString();  // Return the hashed password as a string
        //    }
        //}

        // Encrypted Password 
        //public string EncryptPassword(string encryptedPassword, string keyString)
        //{
        //    // Ensure the key is 32 bytes (AES-256) by trimming or padding the key
        //    byte[] key = GetKey(keyString, 32); // AES-256 requires a 32-byte key

        //    using (Aes aesAlg = Aes.Create())
        //    {
        //        aesAlg.Key = key; // Set the AES key
        //        aesAlg.IV = new byte[16]; // Initialization Vector, set to 0 for simplicity (NOT recommended for production)

        //        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        //        using (MemoryStream msEncrypt = new MemoryStream())
        //        {
        //            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        //                {
        //                    swEncrypt.Write(encryptedPassword); // Write the password to be encrypted
        //                }
        //            }

        //            return Convert.ToBase64String(msEncrypt.ToArray()); // Return the encrypted password as a Base64 string
        //        }
        //    }
        //}



        // Encrypted User And Password into One Single Response 


        //public string UserDecryptedResponse(string base64EncryptedData, string keyString)
        //{
        //    try
        //    {
        //        // Decode the Base64 encoded string into a byte array
        //        byte[] encryptedData = Convert.FromBase64String(base64EncryptedData);

        //        // Extract the IV (first 16 bytes) from the encrypted data
        //        byte[] iv = new byte[16];
        //        Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);

        //        // The remaining data is the encrypted content
        //        byte[] cipherText = new byte[encryptedData.Length - iv.Length];
        //        Buffer.BlockCopy(encryptedData, iv.Length, cipherText, 0, cipherText.Length);

        //        // Get the 32-byte key from the key string (Ensure GetKey returns a 32-byte key)
        //        byte[] key = GetKey(keyString, 32);

        //        using (Aes aesAlg = Aes.Create())
        //        {
        //            aesAlg.Key = key;
        //            aesAlg.IV = iv;
        //            aesAlg.Mode = CipherMode.CBC;
        //            aesAlg.Padding = PaddingMode.PKCS7;

        //            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        //            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
        //            {
        //                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //                {
        //                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        //                    {
        //                        return srDecrypt.ReadToEnd(); // Return the decrypted result as a string
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new InvalidOperationException("An error occurred while decrypting the user data.", ex);
        //    }
        //}

        #endregion




    }
}
