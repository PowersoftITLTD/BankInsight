using BankPortalAPI.Model;
using BankPortalAPI.Repository.Iservices;
using BankPortalAPI.Repository.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.Data.SqlClient;

namespace BankPortalAPI.Controllers
{

    //[Route("api/[controller]")]
    //[ApiController]
    //[Authorize]
   public class UserMasterAuthController : ControllerBase
    {
        private readonly IUserMasterAuthServices _authMasterService;
        private readonly IConfiguration _configuration;
        private readonly string _baseFolder;

        //[HttpGet]
        //public IActionResult Index()
        //{
        //    return View();
        //}
        public UserMasterAuthController(IUserMasterAuthServices authMasterService, SqlConnection sqlConnection, IConfiguration configuration , IOptions<ExcelFileSettings> options)
        {
            _authMasterService = authMasterService;
            _configuration = configuration;
            _baseFolder = Path.Combine(Directory.GetCurrentDirectory(), options.Value.BaseFolder);
        }
        

        [HttpPost("User-login")]
        public async Task<IActionResult> Login([FromBody] UserModel userModel)
        {
            var keyString = _configuration["EncryptionKey"];
            var responseObject = new ResponseObject();

            try
            {
                if (userModel == null || string.IsNullOrEmpty(userModel.Username) || string.IsNullOrEmpty(userModel.Password))
                {
                    responseObject.Status = "Error";
                    responseObject.Message = "Please enter a valid username and password.";
                    return Ok(responseObject);
                    //return Ok(new { message = "Please Entry Valide User & Password " });

                }

                // Validate The User Using Mobile Number , UserGmail , UserNo
                bool userMst = await _authMasterService.ValidateUserByUserInput(userModel.Username);
                if (userMst)
                {
                    var token = await _authMasterService.Authenticate(userModel.Username, userModel.Password);
                    var UserEncrypted = _authMasterService.UserEncryptedReponsone(userModel, keyString);
                    if (token == null || token == "Invalid login name or password")
                    {
                        responseObject.Status = "Error";
                        responseObject.Message = "Invalid username or password.";
                        return Unauthorized(responseObject);
                    }
                    if (UserEncrypted == null)
                    {
                        responseObject.Status = "Error";
                        responseObject.Message = "Invalid user details.";
                        return Ok(responseObject);
                    }
                    responseObject.Status = "Ok";
                    responseObject.Message = "Token generated successfully.";
                    responseObject.Data = new { Token = token, UserEncryptedDetails = UserEncrypted };
                    return Ok(responseObject);
                }
                else
                {
                    responseObject.Status = "Error";
                    responseObject.Message = "Username is not valid."; // ✅ Updated this line
                    return Ok(responseObject);
                }
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = ex.Message;
                return Ok(responseObject);
            }
        }

        [HttpGet("Validate_AuthenticationBy-UserInput")]
        public async Task<IActionResult> GetValidateUserbyUserInput(string userInput)
        {
            var keyString = _configuration["EncryptionKey"];
            var responseObject = new ResponseObject();
            var userModel = new UserModel();
            try
            {
                if (string.IsNullOrEmpty(userInput))
                {
                    responseObject.Status = "Error";
                    responseObject.Message = "Please enter a valid username";
                    return Ok(responseObject);
                    //return Ok(new { message = "Please Entry Valide User & Password " });

                }

                // Validate The User Using Mobile Number , UserGmail , UserNo
                bool userMst = await _authMasterService.ValidateUserByUserInput(userInput);
                if (userMst)
                {
                    var Passwordinbypte = await _authMasterService.GetUserMasterTokenByUserInputAsync(userInput);
                    if (!string.IsNullOrEmpty(Passwordinbypte))
                    {
                        responseObject.Status = "Ok";
                        responseObject.Message = "Token generated successfully.";
                        responseObject.Data = new { Token = Passwordinbypte };
                    }
                    else
                    {
                        responseObject.Status = "Error";
                        responseObject.Message = "Login_Password is Not Found.";
                    }
                }
                else
                {
                    responseObject.Status = "Error";
                    responseObject.Message = "Username is not valid.";
                   // return Ok(responseObject);
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = ex.Message;
                return Ok(responseObject);
            }
        }


        [HttpGet("SingleSignOn-Login")]
        public async Task<IActionResult> SingleSignOnUserInputLogin(string userEmail)
        {
            var keyString = _configuration["EncryptionKey"];
            var responseObject = new ResponseObject();
            var userModel = new UserModel();
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    responseObject.Status = "Error";
                    responseObject.Message = "Please enter a valid username";
                    return Ok(responseObject);
                    //return Ok(new { message = "Please Entry Valide User & Password " });

                }
                // Validate The User Using Mobile Number , UserGmail , UserNo
                bool userMst = await _authMasterService.GetSingleSignOnLogin(userEmail);
                if (userMst)
                {
                    var userModels = await _authMasterService.GetUserMasterDetails(userEmail);
                    var userMasterLogin = new UserMasterLoginModel
                    {
                        UserId = Convert.ToString(userModels.MKEY),
                        UserName = userModels.USER_FULL_NAME
                    };
                    var userMasterEncryptData =  _authMasterService.UserMasterEncryptedReponsone(userMasterLogin, keyString);
                    var Passwordinbypte = await _authMasterService.GetUserMasterTokenByUserInputAsync(userEmail);
                    if (!string.IsNullOrEmpty(Passwordinbypte))
                    {
                        responseObject.Status = "Ok";
                        responseObject.Message = "Token generated successfully.";
                        responseObject.Data = new { Token = Passwordinbypte  , UserEncryptedDetails = userMasterEncryptData};
                    }
                    else
                    {
                        responseObject.Status = "Error";
                        responseObject.Message = "Login_Password is Not Found.";
                    }
                }
                else
                {
                    responseObject.Status = "Error";
                    responseObject.Message = "Username is not valid.";
                    // return Ok(responseObject);
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = ex.Message;
                return Ok(responseObject);
            }
        }

        [Authorize]
        [HttpGet("UserMasterDecryptedPasswordVerifying")]
        public async Task<IActionResult> UserMasterDecryptedPasswordVerifying(string Password)
        {
            var responseObject = new ResponseObject();
            var userMasterModel = new UserMasterLoginModel();
            try
            {
                //var Passwordhash = Convert.ToByte(Password);
                var keyString = _configuration["EncryptionKey"];
                var PassworsHash = _authMasterService.DecryptPassword(Password, keyString);
                if (PassworsHash != null)
                {
                    string[] strDatat = PassworsHash.Split(':');
                    if (strDatat.Length >= 0)
                    {
                        userMasterModel = new UserMasterLoginModel
                        {
                            UserId = strDatat[0],
                            UserName = strDatat[1]
                        };
                    }
                }
                if(userMasterModel.UserName != null)
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "User successfully Decrypted logged Credential";
                    responseObject.Data = userMasterModel;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No UserName is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {

                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during login/registration.";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
                //throw;
                // return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("GetUserInfo-SingalSignOnDecryptUserDetail")]
        public async Task<IActionResult> GetUserInfo(string EncryptedUserDetails)
        {
            var responseObject = new ResponseObject();
            var userMasterModel = new UserMasterLoginModel();
            try
            {
                var keyString = _configuration["EncryptionKey"];
                var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (PassworsHash != null)
                {
                    string[] strDatat = PassworsHash.Split(':');
                    if (strDatat.Length >= 0)
                    {
                        userMasterModel = new UserMasterLoginModel
                        {
                            UserId = strDatat[0],
                            UserName = strDatat[1]
                        };
                    }
                }
                if (userMasterModel.UserName != null)
                {
                    var userMsModel = await _authMasterService.GetUserMasterDetails(userMasterModel.UserName);
                    responseObject.Status = "Ok";
                    responseObject.Message = "User successfully Decrypted logged Credential";
                    responseObject.Data = userMsModel;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No UserName is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch(Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during GetUserInfo";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [Authorize]
        [HttpGet("GetBuildingDetails-Filter")]
        public async Task<IActionResult> GetBuildingDetails()
        {
            var responseObject = new ResponseObject();

            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _authMasterService.GetUserIdbyUserName(Name);
                int businessGroupId = 1;
                //var keyString = _configuration["EncryptionKey"];
                var parameters = new CommonSpParameters
                {
                    UserId = Convert.ToInt32(UserId),
                    BusinessGroupId = businessGroupId,
                    Attribute1 = null,
                    Attribute2 = null,
                    Attribute3 = null,
                    Attribute4 = null
                };

                var result = await _authMasterService.GetDataFromSpAsync<BuildingDetails>("Sp_GetDistinctBuildingDetails", parameters);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Building Details Fetch successfully";
                    responseObject.Data = result;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Building Details is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during Building Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [Authorize]
        [HttpGet("GetProjectDetails-Filter")]
        public async Task<IActionResult> GetProjectDetails()
        {
            var responseObject = new ResponseObject();

            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _authMasterService.GetUserIdbyUserName(Name);
                int businessGroupId = 1;
                //var keyString = _configuration["EncryptionKey"];
                var parameters = new CommonSpParameters
                {
                    UserId = Convert.ToInt32(UserId),
                    BusinessGroupId = businessGroupId,
                    Attribute1 = null,
                    Attribute2 = null,
                    Attribute3 = null,
                    Attribute4 = null
                };

                var result = await _authMasterService.GetDataFromSpAsync<ProjectDetails>("Sp_GetDistinctProjectDetails", parameters);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Project Details Fetch successfully";
                    responseObject.Data = result;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Project Details is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during project Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [Authorize]
        [HttpGet("GetLegalEntityDetails-Filter")]
        public async Task<IActionResult> GetLegalEntityDetails()
        {
            var responseObject = new ResponseObject();

            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _authMasterService.GetUserIdbyUserName(Name);
                int businessGroupId = 1;
                //var keyString = _configuration["EncryptionKey"];
                var parameters = new CommonSpParameters
                {
                    UserId = Convert.ToInt32(UserId),
                    BusinessGroupId = businessGroupId,
                    Attribute1 = null,
                    Attribute2 = null,
                    Attribute3 = null,
                    Attribute4 = null
                };

                var result = await _authMasterService.GetDataFromSpAsync<LegalEntityDetails>("Sp_GetDistinctLegalEntityDetails", parameters);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Legal Entity Details Fetch successfully";
                    responseObject.Data = result;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Legal Entity Details is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during Legal Entity Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [Authorize]
        [HttpGet("GetBankDetails-Filter")]
        public async Task<IActionResult> GetBankDetails()
        {
            var responseObject = new ResponseObject();

            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _authMasterService.GetUserIdbyUserName(Name);
                int businessGroupId = 1;
                //var keyString = _configuration["EncryptionKey"];
                var parameters = new CommonSpParameters
                {
                    UserId = Convert.ToInt32(UserId),
                    BusinessGroupId = businessGroupId,
                    Attribute1 = null,
                    Attribute2 = null,
                    Attribute3 = null,
                    Attribute4 = null
                };

                var result = await _authMasterService.GetDataFromSpAsync<BankDetails>("Sp_GetDistinctBankDetails", parameters);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Building Details Fetch successfully";
                    responseObject.Data = result;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Building Details is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during Building Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [Authorize]
        [HttpGet("GetAccountDetails-Filter")]
        public async Task<IActionResult> GetAccountDetails()
        {
            var responseObject = new ResponseObject();

            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _authMasterService.GetUserIdbyUserName(Name);
                int businessGroupId = 1;
                //var keyString = _configuration["EncryptionKey"];
                var parameters = new CommonSpParameters
                {
                    UserId = Convert.ToInt32(UserId),
                    BusinessGroupId = businessGroupId,
                    Attribute1 = null,
                    Attribute2 = null,
                    Attribute3 = null,
                    Attribute4 = null
                };

                var result = await _authMasterService.GetDataFromSpAsync<AccountDetails>("Sp_GetDistinctAccountDetails", parameters);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Account Details Fetch successfully";
                    responseObject.Data = result;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Account Details is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during Account Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [Authorize]
        [HttpGet("GetAccountStatusDetails-Filter")]
        public async Task<IActionResult> GetAccountStatusDetails()
        {
            var responseObject = new ResponseObject();

            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _authMasterService.GetUserIdbyUserName(Name);
                int businessGroupId = 1;
                //var keyString = _configuration["EncryptionKey"];
                var parameters = new CommonSpParameters
                {
                    UserId = Convert.ToInt32(UserId),
                    BusinessGroupId = businessGroupId,
                    Attribute1 = null,
                    Attribute2 = null,
                    Attribute3 = null,
                    Attribute4 = null
                };

                var result = await _authMasterService.GetDataFromSpAsync<AccountStatus>("Sp_GetDistinctAccountStatus", parameters);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Account Status Details Fetch successfully";
                    responseObject.Data = result;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Account Status Details is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during Account Status Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [HttpPost("Task-Management/Change_Password_NT")]
        [Authorize]
        public async Task<ActionResult<PutChangePasswordOutPutNT>> ChangePasswordNT([FromBody] ChangePasswordInputNT changePasswordInput)
        {
            try
            {
                if (changePasswordInput.LoginName == null)
                {
                    var responseTaskAction = new PutChangePasswordOutPut_List
                    {
                        Status = "Error",
                        Message = "Error Occurd LoginName",
                        Data = null
                    };
                    return Ok(responseTaskAction);
                }
                string Name = User.Identity.Name.ToString();
                decimal UserId =  await _authMasterService.GetUserIdbyUserName(Name);
                int Session_userId = Convert.ToInt32(UserId);
                changePasswordInput.Session_User_ID = Session_userId > 0 ? Session_userId : 0;
                changePasswordInput.Business_Group_ID = changePasswordInput.Business_Group_ID > 0 ? changePasswordInput.Business_Group_ID : 0;
                var ChangePass = await _authMasterService.PostChangePasswordAsync(changePasswordInput);
                return Ok(ChangePass);
            }
            catch (Exception ex)
            {
                var response = new PutChangePasswordOutPut_List
                {
                    Status = "Error",
                    Message = ex.Message,
                    Data = null
                };
                return Ok(response);
            }
        }

        [HttpPost("Task-Management/Forgot_Password")]
        public async Task<ActionResult<IEnumerable<ResetPasswordOutPut_List>>> ForgotPassword([FromBody] ForgotPasswordInput forgotPasswordInput)
        {
            try
            {
                var ForgotPass = await _authMasterService.GetForgotPasswordAsync(forgotPasswordInput.LoginName);
                if (ForgotPass == null)
                {
                    var responseTaskAction =  new  ForgotPasswordOutPut_List                 //new ApiResponse<EmployeeCompanyMST>
                    {
                        Status = "Error",
                        Message = "Error Occurd",
                        Data = null
                    };
                    return Ok(responseTaskAction);
                }
                if (forgotPasswordInput.LoginName == null)
                {
                    var responseTaskAction = new ForgotPasswordOutPut_List
                    {
                        Status = "Error",
                        Message = "Error Occurd LoginName",
                        Data = null
                    };
                    return Ok(responseTaskAction);
                }
                foreach (var Response in ForgotPass)
                {
                    if (Response.Status != "Ok")
                    {
                        var response = new ResetPasswordOutPut_List
                        {
                            Status = "Error",
                            Message = Response.Message,
                            Data = null
                        };
                        return Ok(response);
                    }
                }
                string TempararyPass = string.Empty;
                foreach (var TempPaass in ForgotPass)
                {
                    TempararyPass = TempPaass.Data.Select(x => x.MessageText.ToString()).First().ToString();
                }

                try
                {
                    var ResetPass = await _authMasterService.GetResetPasswordAsync(TempararyPass, forgotPasswordInput.LoginName);
                    return Ok(ResetPass);
                }
                catch(Exception ex)
                {
                    var response = new ResetPasswordOutPut_List
                    {
                        Status = "Error",
                        Message = ex.Message,
                        Data = null
                    };
                    return Ok(response);
                }
                

                //if (ResetPass == null)
                //{
                //    var responseTaskAction = new ApiResponse<EmployeeCompanyMST>
                //    {
                //        Status = "Error",
                //        Message = "Error Occurd",
                //        Data = null
                //    };
                //    return Ok(responseTaskAction);
                //}

                //return Ok(ResetPass);
            }
            catch (Exception ex)
            {
                var response = new ResetPasswordOutPut_List
                {
                    Status = "Error",
                    Message = ex.Message,
                    Data = null
                };
                return Ok(response);
            }
        }

        [HttpPost("Upload_ExcelFile")]
        [Authorize]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            var responseObject = new ResponseObject();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // 1️⃣ File Validation
                if (file == null || file.Length == 0)
                    return BadRequest(new { Status = "Error", Message = "No file uploaded." });

                if (!file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsx"))
                    return BadRequest(new { Status = "Error", Message = "Invalid file type. Only .xls or .xlsx allowed." });

                string Name = User.Identity.Name.ToString();
                decimal UserId = await _authMasterService.GetUserIdbyUserName(Name);
                int Session_userId = Convert.ToInt32(UserId);
                var bankDetailsList = new List<bankUserDetails_HDR>();
                var accountDetailsList = new List<BankAccountDetails>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        if (package.Workbook.Worksheets.Count == 0)
                            return BadRequest(new { Status = "Error", Message = "No worksheet found in file." });

                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet.Dimension == null)
                            return BadRequest(new { Status = "Error", Message = "Worksheet is empty." });

                        int rowCount = worksheet.Dimension.Rows;
                        if (rowCount < 2)
                            return BadRequest(new { Status = "Error", Message = "Excel file contains no data." });

                        // 2️⃣ Read Data
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                // Reading Excel values
                                string legalEntityName = worksheet.Cells[row, 1].Text.Trim();
                                string projectName = worksheet.Cells[row, 2].Text.Trim();
                                string buildingName = worksheet.Cells[row, 3].Text.Trim();
                                string accountNo = worksheet.Cells[row, 4].Text.Trim();
                                string ifscCode = worksheet.Cells[row, 5].Text.Trim();
                                string bankName = worksheet.Cells[row, 6].Text.Trim();

                                string lastBalanceText = worksheet.Cells[row, 7].Text.Trim();
                                string unclearFundsText = worksheet.Cells[row, 8].Text.Trim();
                                string netBalanceText = worksheet.Cells[row, 9].Text.Trim();
                                string balAvailableText = worksheet.Cells[row, 10].Text.Trim();
                                string holdAmountText = worksheet.Cells[row, 11].Text.Trim();
                                string overdraftText = worksheet.Cells[row, 12].Text.Trim();

                                string contactPerson = worksheet.Cells[row, 13].Text.Trim();
                                string contactNo = worksheet.Cells[row, 14].Text.Trim();
                                string gstNumber = worksheet.Cells[row, 15].Text.Trim();
                                string balance_As_Of_Date = worksheet.Cells[row, 16].Text.Trim();

                                // Skip empty rows
                                if (string.IsNullOrWhiteSpace(accountNo))
                                    continue;


                                if (string.IsNullOrWhiteSpace(legalEntityName))
                                    throw new Exception($"LegalEntityName is required in row {row}");

                                if (string.IsNullOrWhiteSpace(projectName))
                                    throw new Exception($"ProjectName is required in row {row}");

                                if (string.IsNullOrWhiteSpace(buildingName))
                                    throw new Exception($"BuildingName is required in row {row}");

                                if (string.IsNullOrWhiteSpace(accountNo))
                                    throw new Exception($"AccountNo is required in row {row}");

                                if (string.IsNullOrWhiteSpace(ifscCode))
                                    throw new Exception($"IFSCCode is required in row {row}");
                                //if (string.IsNullOrWhiteSpace(BankName))
                                //    throw new Exception($"BankName is required in row {row}");
                                //if (string.IsNullOrWhiteSpace(LastBalance))
                                //    throw new Exception($"LastBalance is required in row {row}");
                                //if (string.IsNullOrWhiteSpace(unclearFunds))
                                //    throw new Exception($"unclearFunds is required in row {row}");
                                //if (string.IsNullOrWhiteSpace(netBalance))
                                //    throw new Exception($"netBalance is required in row {row}");
                                //if (string.IsNullOrWhiteSpace(balAvailable))
                                //    throw new Exception($"balAvailable is required in row {row}");
                                //if (string.IsNullOrWhiteSpace(holdAmount))
                                //    throw new Exception($"holdAmount is required in row {row}");
                                //if (string.IsNullOrWhiteSpace(overdraft))
                                //    throw new Exception($"overdraft is required in row {row}");

                                //if (string.IsNullOrWhiteSpace(Contact_Person))
                                //    throw new Exception($"Contact_Person is required in row {row}");
                                //if (string.IsNullOrWhiteSpace(Contact_No))
                                //    throw new Exception($"Contact_No is required in row {row}");
                                //if (string.IsNullOrWhiteSpace(GST_Number))
                                //    throw new Exception($"GST_Number is required in row {row}");

                                //if (string.IsNullOrWhiteSpace(Balance_As_Of_Date))
                                //    throw new Exception($"Balance_As_Of_Date is required in row {row}");

                                // 🧩 Create bank_UserDetails model
                                var userDetail = new bankUserDetails_HDR
                                {
                                    //EntryDateTime = DateTime.UtcNow,
                                    LegalEntityName = string.IsNullOrEmpty(legalEntityName) ? null : legalEntityName,
                                    ProjectName = string.IsNullOrEmpty(projectName) ? null : projectName,
                                    BuildingName = string.IsNullOrEmpty(buildingName) ? null : buildingName,
                                    AccountNo = string.IsNullOrEmpty(accountNo) ? null : accountNo,
                                    IFSCCode = string.IsNullOrEmpty(ifscCode) ? null : ifscCode,
                                    BankName = string.IsNullOrEmpty(bankName) ? null : bankName,
                                    LastBalance = ParseDecimalOrDefault(lastBalanceText),
                                    unclearFunds = ParseDecimalOrDefault(unclearFundsText),
                                    netBalance = ParseDecimalOrDefault(netBalanceText),
                                    balAvailable = ParseDecimalOrDefault(balAvailableText),
                                    holdAmount = ParseDecimalOrDefault(holdAmountText),
                                    overdraft = ParseDecimalOrDefault(overdraftText),
                                    Contact_Person = string.IsNullOrEmpty(contactPerson) ? null : contactPerson,
                                    Contact_No = string.IsNullOrEmpty(contactNo) ? null : contactNo,
                                    GST_Number = string.IsNullOrEmpty(gstNumber) ? null : gstNumber,
                                    DELETE_FLAG = "N",
                                    CREATED_BY = Session_userId,
                                    CREATION_DATE = DateTime.UtcNow

                                    //Balance_As_Of_Date = string.IsNullOrEmpty(balance_As_Of_Date) ? null : balance_As_Of_Date
                                };

                                bankDetailsList.Add(userDetail);

                                // 🧩 Create BankAccountDetails model (example mapping)
                                var accDetail = new BankAccountDetails
                                {
                                    acctNumber = accountNo,
                                    branchCode = ifscCode,
                                    CompanyName = legalEntityName,
                                    ProjectName = projectName,
                                    BuildingName = buildingName,
                                   // AuthorizationVal = bankName,
                                    CREATED_BY = Session_userId, // Example default value
                                    CREATED_BY_Name = Name,
                                    CREATION_DATE = DateTime.UtcNow,
                                    DELETE_FLAG = "N"
                                };

                                accountDetailsList.Add(accDetail);
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine($"Row {row} skipped due to error: {innerEx.Message}");
                            }
                        }
                    }
                }

                if (bankDetailsList.Count == 0)
                    return BadRequest(new { Status = "Error", Message = "No valid data found in Excel." });

                // 3️⃣ INSERT INTO DATABASE — both methods
                string userInsertResult = await _authMasterService.InsertBankUserDetailsListAsync(bankDetailsList);
                string accountInsertResult = await _authMasterService.InsertBankAccountDetailsAsync(accountDetailsList);

                // 4️⃣ Success Response
                responseObject.Status = "Success";
                responseObject.Message = $"Excel data processed and inserted successfully.\n" +
                                         $"Bank_Details_Hdr Insert: {userInsertResult}, Bank_Acc_Details Insert: {accountInsertResult}";
                responseObject.Data = new
                {
                    TotalRows = bankDetailsList.Count,
                    UserRecords = bankDetailsList.Count,
                    AccountRecords = accountDetailsList.Count
                };

                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An unexpected error occurred while processing the Excel file.";
                responseObject.Data = new { error = ex.Message };
                Console.WriteLine($"[UploadExcel] Exception: {ex}");
                return StatusCode(500, responseObject);
            }
        }

        private decimal ParseDecimalOrDefault(string input)
        {
            return decimal.TryParse(input, out decimal value) ? value : 0m;
        }

        //[HttpGet("DownloadExcelFile")]
        //public IActionResult DownloadExcelFile(string fileName)
        //{
        //    if (string.IsNullOrWhiteSpace(fileName))
        //        return BadRequest("Filename must be provided.");

        //    string fileNameWithExt = fileName.EndsWith(".xlsx") ? fileName : fileName + ".xlsx";
        //    var filePath = Path.Combine(_baseFolder, fileNameWithExt);
        //    //var filePath = Path.Combine(_baseFolder, fileName + ".xlsx");
        //    //Console.WriteLine($"Looking for file at: {filePath}");
        //    if (!System.IO.File.Exists(filePath))
        //    {
        //        return NotFound(new { message = "File not found." });
        //    }

        //    var fileBytes = System.IO.File.ReadAllBytes(filePath);
        //    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        //    return File(fileBytes, contentType, fileName);
        //}


        #region

        //[HttpPost("Upload_ExcelFile")]
        ////[Authorize]
        //public async Task<IActionResult> UploadExcel(IFormFile file)
        //{
        //    var responseObject = new ResponseObject();
        //    try
        //    {
        //        // Set EPPlus license for non-commercial use (EPPlus 8+)
        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //        // 1️⃣ File Validation
        //        if (file == null || file.Length == 0)
        //        {
        //            responseObject.Status = "Error";
        //            responseObject.Message = "No file uploaded. Please upload a valid Excel file.";
        //            return BadRequest(responseObject);
        //        }

        //        if (!file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsx"))
        //        {
        //            responseObject.Status = "Error";
        //            responseObject.Message = "Invalid file type. Please upload an Excel file (.xls or .xlsx).";
        //            return BadRequest(responseObject);
        //        }

        //        var bankdetails = new List<bank_UserDetails>();

        //        using (var stream = new MemoryStream())
        //        {
        //            await file.CopyToAsync(stream);
        //            using (var package = new ExcelPackage(stream))
        //            {
        //                if (package.Workbook.Worksheets.Count == 0)
        //                {
        //                    responseObject.Status = "Error";
        //                    responseObject.Message = "No worksheet found in the Excel file.";
        //                    return BadRequest(responseObject);
        //                }
        //                ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // First sheet
        //                if (worksheet.Dimension == null)
        //                {
        //                    responseObject.Status = "Error";
        //                    responseObject.Message = "Worksheet is empty.";
        //                    return BadRequest(responseObject);
        //                }
        //                int rowCount = worksheet.Dimension.Rows;
        //                if (rowCount < 2)
        //                {
        //                    responseObject.Status = "Error";
        //                    responseObject.Message = "No data found in Excel file (only headers present).";
        //                    return BadRequest(responseObject);
        //                }
        //                // 2️⃣ Read and Validate Data Rows
        //                for (int row = 2; row <= rowCount; row++)
        //                {
        //                    try
        //                    {
        //                        string legalEntityName = worksheet.Cells[row, 1].Text.Trim();
        //                        string projectName = worksheet.Cells[row, 2].Text.Trim();
        //                        string buildingName = worksheet.Cells[row, 3].Text.Trim();
        //                        string accountNo = worksheet.Cells[row, 4].Text.Trim();
        //                        string ifscCode = worksheet.Cells[row, 5].Text.Trim();
        //                        string bankName = worksheet.Cells[row, 6].Text.Trim();

        //                        string lastBalanceText = worksheet.Cells[row, 7].Text.Trim();
        //                        string unclearFundsText = worksheet.Cells[row, 8].Text.Trim();
        //                        string netBalanceText = worksheet.Cells[row, 9].Text.Trim();
        //                        string balAvailableText = worksheet.Cells[row, 10].Text.Trim();
        //                        string holdAmountText = worksheet.Cells[row, 11].Text.Trim();
        //                        string overdraftText = worksheet.Cells[row, 12].Text.Trim();

        //                        string contactPerson = worksheet.Cells[row, 13].Text.Trim();
        //                        string contactNo = worksheet.Cells[row, 14].Text.Trim();
        //                        string gstNumber = worksheet.Cells[row, 15].Text.Trim();
        //                        string balance_As_Of_Date = worksheet.Cells[row, 16].Text.Trim();

        //                        // Optional: skip if required fields are missing (example, AccountNo)
        //                        if (string.IsNullOrWhiteSpace(accountNo))
        //                            continue; // skip this row
        //                        //if (string.IsNullOrWhiteSpace(LegalEntityName))
        //                        //    throw new Exception($"LegalEntityName is required in row {row}");

        //                        //if (string.IsNullOrWhiteSpace(ProjectName))
        //                        //    throw new Exception($"ProjectName is required in row {row}");

        //                        //if (string.IsNullOrWhiteSpace(BuildingName))
        //                        //    throw new Exception($"BuildingName is required in row {row}");

        //                        //if (string.IsNullOrWhiteSpace(AccountNo))
        //                        //    throw new Exception($"AccountNo is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(IFSCCode))
        //                        //    throw new Exception($"IFSCCode is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(BankName))
        //                        //    throw new Exception($"BankName is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(LastBalance))
        //                        //    throw new Exception($"LastBalance is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(unclearFunds))
        //                        //    throw new Exception($"unclearFunds is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(netBalance))
        //                        //    throw new Exception($"netBalance is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(balAvailable))
        //                        //    throw new Exception($"balAvailable is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(holdAmount))
        //                        //    throw new Exception($"holdAmount is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(overdraft))
        //                        //    throw new Exception($"overdraft is required in row {row}");

        //                        //if (string.IsNullOrWhiteSpace(Contact_Person))
        //                        //    throw new Exception($"Contact_Person is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(Contact_No))
        //                        //    throw new Exception($"Contact_No is required in row {row}");
        //                        //if (string.IsNullOrWhiteSpace(GST_Number))
        //                        //    throw new Exception($"GST_Number is required in row {row}");

        //                        //if (string.IsNullOrWhiteSpace(Balance_As_Of_Date))
        //                        //    throw new Exception($"Balance_As_Of_Date is required in row {row}");


        //                        var emp = new bank_UserDetails
        //                        {
        //                            LegalEntityName = string.IsNullOrEmpty(legalEntityName) ? null : legalEntityName,
        //                            ProjectName = string.IsNullOrEmpty(projectName) ? null : projectName,
        //                            BuildingName = string.IsNullOrEmpty(buildingName) ? null : buildingName,
        //                            IFSCCode = string.IsNullOrEmpty(ifscCode) ? null : ifscCode,
        //                            BankName = string.IsNullOrEmpty(bankName) ? null : bankName,

        //                            // For decimals, parse and assign 0 if empty or invalid
        //                            LastBalance = ParseDecimalOrDefault(lastBalanceText),
        //                            unclearFunds = ParseDecimalOrDefault(unclearFundsText),
        //                            netBalance = ParseDecimalOrDefault(netBalanceText),
        //                            balAvailable = ParseDecimalOrDefault(balAvailableText),
        //                            holdAmount = ParseDecimalOrDefault(holdAmountText),
        //                            overdraft = ParseDecimalOrDefault(overdraftText),

        //                            Contact_Person = string.IsNullOrEmpty(contactPerson) ? null : contactPerson,
        //                            Contact_No = string.IsNullOrEmpty(contactNo) ? null : contactNo,
        //                            GST_Number = string.IsNullOrEmpty(gstNumber) ? null : gstNumber,
        //                            Balance_As_Of_Date = string.IsNullOrEmpty(balance_As_Of_Date) ? null : balance_As_Of_Date
        //                        };


        //                        bankdetails.Add(emp);
        //                    }
        //                    catch (Exception innerEx)
        //                    {
        //                        // Log and skip problematic row
        //                        Console.WriteLine($"Row {row} skipped due to error: {innerEx.Message}");
        //                    }
        //                }
        //            }
        //        }

        //        if (bankdetails.Count == 0)
        //        {
        //            responseObject.Status = "Error";
        //            responseObject.Message = "No valid employee data found in Excel file.";
        //            return BadRequest(responseObject);
        //        }

        //        // 3️⃣ Success Response
        //        responseObject.Status = "Success";
        //        responseObject.Message = "Excel data fetched successfully.";
        //        responseObject.Data = bankdetails;

        //        return Ok(responseObject);
        //    }
        //    catch (Exception ex)
        //    {
        //        // 4️⃣ General Exception Handling
        //        responseObject.Status = "Error";
        //        responseObject.Message = "An unexpected error occurred while processing the Excel file.";
        //        responseObject.Data = new { error = ex.Message };

        //        // Log detailed exception for developers
        //        Console.WriteLine($"[UploadExcel] Exception: {ex}");

        //        return StatusCode(500, responseObject);
        //    }
        //}

        ////[HttpGet("DownloadExcelFile")]
        ////public IActionResult DownloadExcelFile(string fileName)
        ////{
        ////    if (string.IsNullOrWhiteSpace(fileName))
        ////        return BadRequest("Filename must be provided.");

        ////    string fileNameWithExt = fileName.EndsWith(".xlsx") ? fileName : fileName + ".xlsx";
        ////    var filePath = Path.Combine(_baseFolder, fileNameWithExt);
        ////    //var filePath = Path.Combine(_baseFolder, fileName + ".xlsx");
        ////    //Console.WriteLine($"Looking for file at: {filePath}");
        ////    if (!System.IO.File.Exists(filePath))
        ////    {
        ////        return NotFound(new { message = "File not found." });
        ////    }

        ////    var fileBytes = System.IO.File.ReadAllBytes(filePath);
        ////    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        ////    return File(fileBytes, contentType, fileName);
        ////}

        //private decimal ParseDecimalOrDefault(string input)
        //{
        //    if (decimal.TryParse(input, out decimal value))
        //        return value;
        //    return 0m; // default to zero if null, empty, or invalid decimal
        //}

        #endregion

    }


}
