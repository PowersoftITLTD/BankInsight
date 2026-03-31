using BankPortalAPI.Model;
using BankPortalAPI.Repository.Iservices;
using BankPortalAPI.Repository.Services;
using Jose;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestSharp;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static Dapper.SqlMapper;

namespace BankPortalAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BankPortalController : ControllerBase
    {

        private readonly IBankPortalServices _bankPortalService;
        private readonly IAuthService _auth;
        private readonly IConfiguration _configuration;
        private readonly ICommonServices _commonService;
        private readonly string _encryptionKey;
        private readonly string _evn;
        private readonly string _fileName;
        private readonly IRefreshAll_Bank_Acc_DetailsServices _refreshBAD;

        public BankPortalController(IBankPortalServices bankPortalService,IAuthService authService, IConfiguration configuration, ICommonServices commonService, IRefreshAll_Bank_Acc_DetailsServices refreshBAD)
        {
            _bankPortalService = bankPortalService;
            _auth = authService;
            _configuration = configuration;
            _commonService = commonService;
            _encryptionKey = _configuration["EncryptionKey"];
            _refreshBAD = refreshBAD;
            _evn= _configuration["Environment_key"];
            _fileName= _configuration["FileName"];
        }

        [HttpGet("BankDetails")]
        // Get All BankDetailList Method
        public async Task<IActionResult> GetBankDetails( int? businessGroupId,  string? entity, string? project, string? building, string? bank, string? account, string? balanceRange)
        {

            try
            {
                //int UserId = 2;
                int? BusinessGroupId = businessGroupId > 0 ? businessGroupId : 1;
                var responseObj = new object();
                var commonListParameter = new CommonListParameters();
                string Name = User.Identity.Name.ToString();
                int UserId = await _auth.GetUserIdbyUserName(Name);
                commonListParameter = new CommonListParameters
                {
                    UserId = UserId,
                    BusinessGroupId = BusinessGroupId,
                    Entity = entity?.Replace(" ", ""),
                    Project = project?.Replace(" ", ""),
                    Building = building?.Replace(" ", ""),
                    Bank = bank?.Replace(" ", ""),
                    Account = account?.Replace(" ", ""),
                    BalanceRange = balanceRange?.Replace(" ", "")
                };
                var responseList = await _bankPortalService.GetAllBankDetails(commonListParameter);    //UserId , BusinessGroupId
                if (responseList != null || responseList.Any())
                {
                    responseObj = new { Status = "Success", Message = "ProjectList Data Retriving SuccessFully ", Data = responseList };
                }
                else
                {
                    responseObj = new { Status = "Error", Message = "ProjectList Data Failed Our Empty" };
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");

            }
        }

        [HttpPost("BankDetails_PS")]
        public async Task<IActionResult> GetBankDetails_PS([FromBody] jsonEncryptModel jsonEncrypt)  //[FromBody] BankDetails_Model details_Model      int? businessGroupId, string? entity, string? project, string? building, string? bank, string? account, string? balanceRange
        {

            try
            {
                //var EncrypteduserModel = _commonService.EncryptionObje<BankDetails_Model>(details_Model, _encryptionKey);
                var details_Model = _commonService.DecryptObject<BankDetails_Model>(jsonEncrypt.jsonEncrypt, _encryptionKey);
                //int UserId = 2;
                int? BusinessGroupId = details_Model.businessGroupId > 0 ? details_Model.businessGroupId : 1;
                var responseObj = new object();
                var commonListParameter = new CommonListParameters();
                string Name = User.Identity.Name.ToString();
                int UserId = await _auth.GetUserIdbyUserName(Name);
                commonListParameter = new CommonListParameters
                {
                    UserId = UserId,
                    BusinessGroupId = BusinessGroupId,
                    Entity = details_Model.entity?.Replace(" ", ""),
                    Project = details_Model.project?.Replace(" ", ""),
                    Building = details_Model.building?.Replace(" ", ""),
                    Bank = details_Model.bank?.Replace(" ", ""),
                    Account = details_Model.account?.Replace(" ", ""),
                    BalanceRange = details_Model.balanceRange?.Replace(" ", "")
                };
                var responseList = await _bankPortalService.GetAllBankDetails_Ps(commonListParameter);
                
                //UserId , BusinessGroupId
                List<BankPortal_model> bankportalList = responseList.Status == "Success" && responseList.Data is List<BankPortal_model> data ? data : new List<BankPortal_model>();
                //var bankportalList= responseList.Data;
                var encryptresponseList = _commonService.EncryptionObje<List<BankPortal_model>>(bankportalList, _encryptionKey);
                var decryptedResponselisyt = _commonService.DecryptObject<List<BankPortal_model>>(encryptresponseList, _encryptionKey);
                if (responseList != null || responseList.Status.Contains("Success"))
                {
                    responseObj = new { Status = "Success", Message = "ProjectList Data Retriving SuccessFully ", Data = encryptresponseList };
                }
                else
                {
                    responseObj = new { Status = "Error", Message = "ProjectList Data Failed Our Empty" };
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");

            }
        }

        [HttpGet("GetBankTopEntitiesbyBalance")]
        public async Task<IActionResult> GetBankTopEntitiesbyBalance()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var bankAccountSummaryList = await _bankPortalService.GetEntityBankAccountBalanceAsync(UserId, businessGroupId);
                if (bankAccountSummaryList != null)
                {
                    var EncryptBankAccountSummary= 
                    responseObj.Status = "Success";
                    responseObj.Message = $"Account  Top Entity Balance List Fetch";
                    responseObj.Data = bankAccountSummaryList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = bankAccountSummaryList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpPost("GetBankTopEntitiesbyBalance_PS")]
        public async Task<IActionResult> GetBankTopEntitiesbyBalance_Ps([FromBody] jsonEncryptModel jsonEncrypt)   //[FromBody] jsonEncryptModel jsonEncrypt
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if(DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
              
            try
            {
                var bankAccountSummaryList = await _bankPortalService.GetEntityBankAccountBalanceAsync(UserId, businessGroupId);
                var EncryptentityBalance_model= _commonService.EncryptionObje<List<EntityBalance_Model>>(bankAccountSummaryList, _encryptionKey);
                var DecryptentityBalance_model = _commonService.DecryptObject<List<EntityBalance_Model>>(EncryptentityBalance_model, _encryptionKey);
                if (DecryptentityBalance_model != null)
                {
                    var EncryptBankAccountSummary =
                    responseObj.Status = "Success";
                    responseObj.Message = $"Account  Top Entity Balance List Fetch";
                    responseObj.Data = EncryptentityBalance_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptentityBalance_model;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpGet("GetBankTopProjectbyBalance")]
        public async Task<IActionResult> GetBankTopProjectbyBalance()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var bankAccountSummaryList = await _bankPortalService.GetBankTopbyProjectBalanceAsync(UserId, businessGroupId);
                if (bankAccountSummaryList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Account  Top Project Balance List Fetch";
                    responseObj.Data = bankAccountSummaryList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = bankAccountSummaryList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpPost("GetBankTopProjectbyBalance_PS")]
        public async Task<IActionResult> GetBankTopProjectbyBalance_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                var bankAccountSummaryList = await _bankPortalService.GetBankTopbyProjectBalanceAsync(UserId, businessGroupId);
                var EncryptTopProjectbyBalance_model = _commonService.EncryptionObje<List<BankTopProjectbyBalance>>(bankAccountSummaryList, _encryptionKey);
                var DecryptTopProjectbyBalance_model = _commonService.DecryptObject<List<BankTopProjectbyBalance>>(EncryptTopProjectbyBalance_model, _encryptionKey);

                if (DecryptTopProjectbyBalance_model != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Account  Top Project Balance List Fetch";
                    responseObj.Data = EncryptTopProjectbyBalance_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptTopProjectbyBalance_model;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpGet("AccountType-Balance")]

        public async Task<IActionResult> GetAccountTypeBalance()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var bankAccountSummaryList = await _bankPortalService.GetAccountTypeBalanceAsync(UserId, businessGroupId);
                if (bankAccountSummaryList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Account Type Balance List Fetch";
                    responseObj.Data = bankAccountSummaryList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = bankAccountSummaryList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }


        [HttpPost("AccountType-Balance_PS")]
        public async Task<IActionResult> GetAccountTypeBalance_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                var bankAccountTypeBalanceList = await _bankPortalService.GetAccountTypeBalanceAsync(UserId, businessGroupId);
                var EncryptbankAccountTypeBalanceList_model = _commonService.EncryptionObje<List<AccountType_BalanceModel>>(bankAccountTypeBalanceList, _encryptionKey);
                var DecryptbankAccountTypeBalanceList_model = _commonService.DecryptObject<List<AccountType_BalanceModel>>(EncryptbankAccountTypeBalanceList_model, _encryptionKey);

                if (DecryptbankAccountTypeBalanceList_model != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Account Type Balance List Fetch";
                    responseObj.Data = EncryptbankAccountTypeBalanceList_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptbankAccountTypeBalanceList_model;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpGet("BankAccount-Details")]

        public async Task<IActionResult> GetBankAccount_Details()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var bankAccountSummaryList = await _bankPortalService.GetBankAccountDetailsAsync(UserId, businessGroupId);
                if (bankAccountSummaryList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Entity List Fetch Successfully";
                    responseObj.Data = bankAccountSummaryList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = bankAccountSummaryList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpPost("BankAccount-Details_PS")]

        public async Task<IActionResult> GetBankAccount_Details_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                var bankAccountDetailsModelList = await _bankPortalService.GetBankAccountDetailsAsync(UserId, businessGroupId);
                var EncryptbankAccountDetailsModel_model = _commonService.EncryptionObje<List<BankAccountDetailsModel>>(bankAccountDetailsModelList, _encryptionKey);
                var DecryptbankAccountDetailsModel_model = _commonService.DecryptObject<List<BankAccountDetailsModel>>(EncryptbankAccountDetailsModel_model, _encryptionKey);

                if (DecryptbankAccountDetailsModel_model != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Entity List Fetch Successfully";
                    responseObj.Data = EncryptbankAccountDetailsModel_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptbankAccountDetailsModel_model;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpGet("Entity-List")]
        public async Task<IActionResult> GetEntityList(string? entity ,string? project , string? building, string? bank, string? account, string? balanceRange)
        {
            var responseObj = new ResponseObject();
            var commonListParameter = new CommonListParameters();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                commonListParameter = new CommonListParameters
                {
                    UserId= UserId,
                    BusinessGroupId= businessGroupId,
                    Entity = entity?.Replace(" ", ""),
                    Project = project?.Replace(" ", ""),
                    Building = building?.Replace(" ", ""),
                    Bank = bank?.Replace(" ", ""),
                    Account = account?.Replace(" ", ""),
                    BalanceRange = balanceRange?.Replace(" ", "")
                };
                var bankAccountSummaryList = await _bankPortalService.GetEntityListAsync(commonListParameter);    //UserId, businessGroupId
                if (bankAccountSummaryList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Entity List Fetch";
                    responseObj.Data = bankAccountSummaryList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = bankAccountSummaryList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpPost("Entity-List_PS")]
        public async Task<IActionResult> GetEntityList_PS([FromBody] jsonEncryptModel jsonEncrypt)    //[FromBody]BankDetails_Model bankDetails_
        {
            var responseObj = new ResponseObject();
            var commonListParameter = new CommonListParameters();
            int? UserId = 0;
            int? businessGroupId = 0;
            //var EncrypteduserModel = _commonService.EncryptionObje<BankDetails_Model>(bankDetails_, _encryptionKey);
            var details_Model = _commonService.DecryptObject<BankDetails_Model>(jsonEncrypt.jsonEncrypt, _encryptionKey);

            if(details_Model.UserId > 0)
            {
                UserId= details_Model.UserId;
                businessGroupId = details_Model.businessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                 UserId = await _auth.GetUserIdbyUserName(Name);
                 businessGroupId = 1;
            }
            try
            {
                commonListParameter = new CommonListParameters
                {
                    UserId = UserId,
                    BusinessGroupId = businessGroupId,
                    Entity = details_Model.entity?.Replace(" ", ""),
                    Project = details_Model.project?.Replace(" ", ""),
                    Building = details_Model.building?.Replace(" ", ""),
                    Bank = details_Model.bank?.Replace(" ", ""),
                    Account = details_Model.account?.Replace(" ", ""),
                    BalanceRange = details_Model.balanceRange?.Replace(" ", "")
                };
                var bankAccountSummaryList = await _bankPortalService.GetEntityListAsync(commonListParameter);
                //UserId, businessGroupId
                //List<BankPortal_model> bankportalList = bankAccountSummaryList.S == "Success" && bankAccountSummaryList.Data is List<BankPortal_model> data ? data : new List<BankPortal_model>();
                //var bankportalList= responseList.Data;
                var encryptresponseList = _commonService.EncryptionObje<List<EntityModel>>(bankAccountSummaryList, _encryptionKey);
                var decryptedResponselisyt = _commonService.DecryptObject<List<EntityModel>>(encryptresponseList, _encryptionKey);
                if (decryptedResponselisyt != null && decryptedResponselisyt.Any()!= null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Entity List Fetch";
                    responseObj.Data = encryptresponseList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = encryptresponseList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpGet("Project-List")]
        public async Task<IActionResult> GetProjectList(string? entity, string? project, string? building, string? bank, string? account, string? balanceRange)
        {
            var responseObj = new ResponseObject();
            var commonListParameter = new CommonListParameters();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                commonListParameter = new CommonListParameters
                {
                    UserId = UserId,
                    BusinessGroupId = businessGroupId,
                    Entity = entity?.Replace(" ", ""),
                    Project = project?.Replace(" ", ""),
                    Building = building?.Replace(" ", ""),
                    Bank = bank?.Replace(" ", ""),
                    Account = account?.Replace(" ", ""),
                    BalanceRange = balanceRange?.Replace(" ", "")
                };
                var bankAccountSummaryList = await _bankPortalService.GetProjectListAsync(commonListParameter);
                if (bankAccountSummaryList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $" Project List Fetch Successfully";
                    responseObj.Data = bankAccountSummaryList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = bankAccountSummaryList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpPost("Project-List_PS")]
        public async Task<IActionResult> GetProjectList_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            var commonListParameter = new CommonListParameters();
            int? UserId = 0;
            int? businessGroupId = 0;
            //var EncrypteduserModel = _commonService.EncryptionObje<BankDetails_Model>(bankDetails_, _encryptionKey);
            var details_Model = _commonService.DecryptObject<BankDetails_Model>(jsonEncrypt.jsonEncrypt, _encryptionKey);

            if (details_Model.UserId > 0)
            {
                UserId = details_Model.UserId;
                businessGroupId = details_Model.businessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                commonListParameter = new CommonListParameters
                {
                    UserId = UserId,
                    BusinessGroupId = businessGroupId,
                    Entity = details_Model.entity?.Replace(" ", ""),
                    Project = details_Model.project?.Replace(" ", ""),
                    Building = details_Model.building?.Replace(" ", ""),
                    Bank = details_Model.bank?.Replace(" ", ""),
                    Account = details_Model.account?.Replace(" ", ""),
                    BalanceRange = details_Model.balanceRange?.Replace(" ", "")
                };
                var projectModelList = await _bankPortalService.GetProjectListAsync(commonListParameter);
                var encryptresponseList = _commonService.EncryptionObje<List<ProjectModel>>(projectModelList, _encryptionKey);
                var decryptedResponselisyt = _commonService.DecryptObject<List<ProjectModel>>(encryptresponseList, _encryptionKey);
                if (decryptedResponselisyt != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $" Project List Fetch Successfully";
                    responseObj.Data = encryptresponseList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = encryptresponseList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }
        
        [HttpGet("ApprovalStatus")]
        public async Task<IActionResult> GetApprovalStatusList([FromQuery] string activeFlag)
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                string flag = activeFlag.ToString().ToUpper();
                char activeflag = Convert.ToChar(flag);
                var approvalStatusList = await _bankPortalService.GetBankApprovalHdrList(activeflag, UserId, businessGroupId);
                if (approvalStatusList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Approval Status List Fetch";
                    responseObj.Data = approvalStatusList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = approvalStatusList;
                }
                return Ok(responseObj);
                }catch(Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpPost("ApprovalStatus_PS")]
        public async Task<IActionResult> GetApprovalStatusList_PS([FromQuery]  jsonEncryptModel jsonEncrypt) // string activeFlag   CommonInputAprovalStatus inputAprovalStatus
        {
            var responseObj = new ResponseObject();
            var commonListParameter = new CommonListParameters();
            int? UserId = 0;
            int? businessGroupId = 0;
            //var EncrypteduserModel = _commonService.EncryptionObje<CommonInputAprovalStatus>(inputAprovalStatus, _encryptionKey);
            var details_Model = _commonService.DecryptObject<CommonInputAprovalStatus>(jsonEncrypt.jsonEncrypt, _encryptionKey);  // jsonEncrypt.jsonEncrypt

            if (details_Model.UserId > 0)
            {
                UserId = details_Model.UserId;
                businessGroupId = details_Model.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                string flag = details_Model.activeFlag.ToString().ToUpper();
                char activeflag = Convert.ToChar(flag);
                var approvalStatusList = await _bankPortalService.GetBankApprovalHdrList(activeflag, UserId, businessGroupId);
                var encryptresponseList = _commonService.EncryptionObje<List<Bank_Approval_Hdr_Model>>(approvalStatusList, _encryptionKey);
                var decryptedResponselisyt = _commonService.DecryptObject<List<Bank_Approval_Hdr_Model>>(encryptresponseList, _encryptionKey);
                if (decryptedResponselisyt != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Approval Status List Fetch";
                    responseObj.Data = encryptresponseList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = encryptresponseList;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpGet("BankApprovalCurrentBalance")]
        public async Task<IActionResult> GetBankApprovalCurrentAmount()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var approvalStatusList = await _bankPortalService.GetBankApprovalQueryListAsync(UserId, businessGroupId);
                if (approvalStatusList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Approval Status List Fetch";
                    responseObj.Data = approvalStatusList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = approvalStatusList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpPost("BankApprovalCurrentBalance_PS")]
        public async Task<IActionResult> GetBankApprovalCurrentAmount_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                var approvalStatusList = await _bankPortalService.GetBankApprovalQueryListAsync(UserId, businessGroupId);
                var EncryptapprovalStatusList_model = _commonService.EncryptionObje<List<BankApprovalQuery>>(approvalStatusList, _encryptionKey);
                var DecryptapprovalStatusList_model = _commonService.DecryptObject<List<BankApprovalQuery>>(EncryptapprovalStatusList_model, _encryptionKey);

                if (DecryptapprovalStatusList_model != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Approval Status List Fetch";
                    responseObj.Data = EncryptapprovalStatusList_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptapprovalStatusList_model;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpGet("GetBankDetail_Card")]
        public async Task<IActionResult> GetBankDetailsCard()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var bankAccountSummaryList = await _bankPortalService.GetBankDetailsByCardAsync(UserId, businessGroupId);
                if (bankAccountSummaryList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"All Bank Details List Fetch Successfully";
                    responseObj.Data = bankAccountSummaryList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = bankAccountSummaryList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpPost("GetBankDetail_Card_PS")]
        public async Task<IActionResult> GetBankDetailsCard_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                var bankAccountSummaryList = await _bankPortalService.GetBankDetailsByCardAsync(UserId, businessGroupId);
                var EncryptBankDetailsbyCardList_model = _commonService.EncryptionObje<List<BankDetailByCard>>(bankAccountSummaryList, _encryptionKey);
                var DecryptBankDetailsbyCardList_model = _commonService.DecryptObject<List<BankDetailByCard>>(EncryptBankDetailsbyCardList_model, _encryptionKey);
                if (DecryptBankDetailsbyCardList_model != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"All Bank Details List Fetch Successfully";
                    responseObj.Data = EncryptBankDetailsbyCardList_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptBankDetailsbyCardList_model;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpGet("GetViewAllBankDetailBy_BankId")]
        public async Task<IActionResult> GetViewAllBankDetailBy_BankId(int BankId ,string BankName)
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var bankAccountSummaryList = await _bankPortalService.GetALLBankDetailsByCardAsync(UserId, businessGroupId ,BankId ,BankName);
                if (bankAccountSummaryList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Account  Top Entity Balance List Fetch";
                    responseObj.Data = bankAccountSummaryList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = bankAccountSummaryList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        [HttpPost("GetViewAllBankDetailBy_BankId_PS")]
        public async Task<IActionResult> GetViewAllBankDetailBy_BankId_PS([FromBody] jsonEncryptModel jsonEncrypt)  //int BankId, string BankName
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<ViewAllBankAccountDetails>(allBankAccountDetails, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<ViewAllBankAccountDetails>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                var AllbankAccountSummaryList = await _bankPortalService.GetALLBankDetailsByCardAsync(UserId, businessGroupId, DeCryptCommonInputResponse.BankId, DeCryptCommonInputResponse.BankName);
                var EncryptAllbankAccountSummaryList = _commonService.EncryptionObje<List<BankPortal_model>>(AllbankAccountSummaryList, _encryptionKey);
                var DecryptAllbankAccountSummaryList = _commonService.DecryptObject<List<BankPortal_model>>(EncryptAllbankAccountSummaryList, _encryptionKey);


                if (DecryptAllbankAccountSummaryList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Account  Top Entity Balance List Fetch";
                    responseObj.Data = EncryptAllbankAccountSummaryList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptAllbankAccountSummaryList;
                }
                return Ok(responseObj);

            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);

            }
        }

        #region
        [HttpGet("UserDecryptedPasswordVerifying")]

        public async Task<IActionResult> UserDecryptedPasswordVerifying(string Password)
        {
            var responseObject = new ResponseObject();
            // var userModel = new UserModel();
            try
            {
                //var Passwordhash = Convert.ToByte(Password);
                var keyString = _configuration["EncryptionKey"];
                var PassworsHash = _auth.DecryptPassword(Password, keyString);
                if (PassworsHash != null)
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "User successfully Decrypted logged in Credential";
                    responseObject.Data = PassworsHash;
                    //return Ok(new { Message = userModel, Status = "Ok" });
                    return Ok(responseObject);

                }
                else
                {
                    responseObject.Status = "Error";
                    responseObject.Message = "User Decrypted logged Failed";
                    responseObject.Data = PassworsHash;
                    //return Ok(new { Message = userModel, Status = "Ok" });
                    return Ok(responseObject);
                }

                //return Ok(new { message = result, Status = "Ok" });
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

        [HttpGet("EncryptedUserbyUserInput")]

        public async Task<IActionResult> UserEncryptedbyUserInput([FromQuery] string userInput)
        {
            var responseObject = new ResponseObject();
            try
            {
                var keyString = _configuration["EncryptionKey"];
                var encryptedUser = _auth.EncryptedInputbuUser(userInput, keyString);
                if (encryptedUser != null)
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "User successfully Encrypted logged in Credential";
                    responseObject.Data = encryptedUser;
                    //return Ok(new { Message = userModel, Status = "Ok" });
                    return Ok(responseObject);
                }
                else
                {
                    responseObject.Status = "Error";
                    responseObject.Message = "User Encrypted logged Failed";
                    responseObject.Data = encryptedUser;
                    //return Ok(new { Message = userModel, Status = "Ok" });
                    return Ok(responseObject);
                }
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during login/registration.";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }


        [HttpGet("BankApproval_Hdr-Approved")]
        public async Task<IActionResult> GetApprovedBankApproval_Hdr()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var approvedList = await _bankPortalService.GetApprovedBankApproval_HdrList(UserId, businessGroupId);
                if (approvedList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Approved Status List Fetch";
                    responseObj.Data = approvedList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = approvedList;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpGet("BankApproval_Hdr-Approved_PS")]
        public async Task<IActionResult> GetApprovedBankApproval_Hdr_PS([FromBody] jsonEncryptModel jsonEncrypt) //CommoninputResponse commoninput
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                var approvedList = await _bankPortalService.GetApprovedBankApproval_HdrList(UserId, businessGroupId);
                var EncryptapprovedList_model = _commonService.EncryptionObje<List<CommonTransactionsModel>>(approvedList, _encryptionKey);
                var DecryptapprovedList_model = _commonService.DecryptObject<List<CommonTransactionsModel>>(EncryptapprovedList_model, _encryptionKey);

                if (DecryptapprovedList_model != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Approved Status List Fetch";
                    responseObj.Data = EncryptapprovedList_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptapprovedList_model;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpGet("BankApproval_Hdr-Pending")]
        public async Task<IActionResult> GetPendingBankApproval_Hdr()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var pendingList = await _bankPortalService.GetPendingBankApproval_HdrList(UserId, businessGroupId);
                if (pendingList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Pending Status List Fetch";
                    responseObj.Data = pendingList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = pendingList;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpGet("BankApproval_Hdr-Pending_PS")]
        public async Task<IActionResult> GetPendingBankApproval_Hdr_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                var pendingList = await _bankPortalService.GetPendingBankApproval_HdrList(UserId, businessGroupId);
                var EncryptpendingList_model = _commonService.EncryptionObje<List<CommonTransactionsModel>>(pendingList, _encryptionKey);
                var DecryptpendingList_model = _commonService.DecryptObject<List<CommonTransactionsModel>>(EncryptpendingList_model, _encryptionKey);
                if (DecryptpendingList_model != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Pending Status List Fetch";
                    responseObj.Data = EncryptpendingList_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptpendingList_model;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpGet("BankApproval_Hdr-Rejected")]
        public async Task<IActionResult> GetRejectBankApproval_Hdr()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var rejectList = await _bankPortalService.GetRejectedBankApproval_HdrList(UserId, businessGroupId);
                if (rejectList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Rejected Status List Fetch";
                    responseObj.Data = rejectList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = rejectList;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpGet("BankApproval_Hdr-Completed")]
        public async Task<IActionResult> GetCompletedBankApproval_Hdr()
        {
            var responseObj = new ResponseObject();
            string Name = User.Identity.Name?.ToString();
            int UserId = await _auth.GetUserIdbyUserName(Name);
            int businessGroupId = 1;
            try
            {
                var completedList = await _bankPortalService.GetCompletedBankApproval_HdrList(UserId, businessGroupId);
                if (completedList != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Rejected Status List Fetch";
                    responseObj.Data = completedList;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = completedList;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpGet("BankApproval_Hdr-Rejected_PS")]
        public async Task<IActionResult> GetRejectBankApproval_Hdr_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                var rejectList = await _bankPortalService.GetRejectedBankApproval_HdrList(UserId, businessGroupId);
                var EncryptrejectList_model = _commonService.EncryptionObje<List<CommonTransactionsModel>>(rejectList, _encryptionKey);
                var DecryptpendingList_model = _commonService.DecryptObject<List<CommonTransactionsModel>>(EncryptrejectList_model, _encryptionKey);
                if (DecryptpendingList_model != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Rejected Status List Fetch";
                    responseObj.Data = EncryptrejectList_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptrejectList_model;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpGet("BankApproval_Hdr-Completed_PS")]
        public async Task<IActionResult> GetCompletedBankApproval_Hdr_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            var DeCryptCommonInputResponse = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (DeCryptCommonInputResponse.UserId > 0)
            {
                UserId = DeCryptCommonInputResponse.UserId;
                businessGroupId = DeCryptCommonInputResponse.BusinessGroupId;
            }
            else
            {
                string Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                var completedList = await _bankPortalService.GetCompletedBankApproval_HdrList(UserId, businessGroupId);
                var EncryptcompletedList_model = _commonService.EncryptionObje<List<CommonTransactionsModel>>(completedList, _encryptionKey);
                var DecryptpendingList_model = _commonService.DecryptObject<List<CommonTransactionsModel>>(EncryptcompletedList_model, _encryptionKey);
                if (DecryptpendingList_model != null)
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"Rejected Status List Fetch";
                    responseObj.Data = EncryptcompletedList_model;

                }
                else
                {
                    responseObj.Status = "Success";
                    responseObj.Message = $"No Data Available ";
                    responseObj.Data = EncryptcompletedList_model;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        // Encryption Done in Above All Method

        [HttpPost("InsertBank_Approval_Hdr")]
        public async Task<IActionResult> InsertBank_Approval_Hdr([FromBody] Bank_Approval_Hdr_Model bank_Approval_Hdr_Model)
        {
            var responseObj = new ResponseObject();
            try
            {
                string Name = User.Identity.Name?.ToString();
                int UserId = await _auth.GetUserIdbyUserName(Name);
                bank_Approval_Hdr_Model.UserId = UserId;
                bank_Approval_Hdr_Model.BusinessGroupId = 1;
                bank_Approval_Hdr_Model.CREATION_DATE = DateTime.UtcNow;
                bank_Approval_Hdr_Model.CREATED_BY = UserId;
                bank_Approval_Hdr_Model.CREATED_BY_Name = Name;
                var Encryptrespons = _commonService.EncryptionObje<Bank_Approval_Hdr_Model>(bank_Approval_Hdr_Model, _encryptionKey);
                var decryptrespons = _commonService.DecryptObject<Bank_Approval_Hdr_Model>(Encryptrespons, _encryptionKey);
                var responseResult = await _bankPortalService.InsertOrUpdateBankApprovalAsync(bank_Approval_Hdr_Model);
                
               
                if (responseResult.Contains("Success"))
                {
                    responseObj.Status = "Success";
                    responseObj.Message = "Insert Bank Approval Successfully";
                    responseObj.Data = Encryptrespons;
                }
                else
                {
                    responseObj.Status = "Error";
                    responseObj.Message = "Insert Failed in Bank Approval";
                    responseObj.Data = null;
                }

                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during OTP generation.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }


        [HttpPost("InsertBank_Approval_Hdr_PS")]

        public async Task<IActionResult> InsertBank_Approval_Hdr_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<Bank_Approval_Hdr_Model>(bank_Approval_Hdr_Model, _encryptionKey);
            var DeCryptBankApprovalHdrModel = _commonService.DecryptObject<Bank_Approval_Hdr_Model>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            try
            {
                string Name = User.Identity.Name?.ToString();
                int UserId = await _auth.GetUserIdbyUserName(Name);
                DeCryptBankApprovalHdrModel.UserId = UserId;
                DeCryptBankApprovalHdrModel.BusinessGroupId = 1;
                DeCryptBankApprovalHdrModel.CREATION_DATE = DateTime.UtcNow;
                DeCryptBankApprovalHdrModel.CREATED_BY = UserId;
                DeCryptBankApprovalHdrModel.CREATED_BY_Name = Name;
                DeCryptBankApprovalHdrModel.LAST_UPDATED_BY_Name = Name;
                DeCryptBankApprovalHdrModel.LAST_UPDATE_DATE = DateTime.UtcNow;
                var responseResult = await _bankPortalService.InsertOrUpdateBankApprovalAsync(DeCryptBankApprovalHdrModel);
                if (responseResult.Contains("Success"))
                {
                    responseObj.Status = "Success";
                    responseObj.Message = "Insert Bank Approval Successfully";
                    responseObj.Data = jsonEncrypt.jsonEncrypt;
                }
                else
                {
                    responseObj.Status = "Error";
                    responseObj.Message = "Insert Failed in Bank Approval";
                    responseObj.Data = null;
                }
                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during OTP generation.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpPost("Update_Bank_Approval_Hdr")]
        public async Task<IActionResult> Update_Bank_Approval_Hdr(Update_BnakPortalApproval update_BnakPortal)
        {
            var responseObj = new ResponseObject();
            try
            {
                string Name = User.Identity.Name?.ToString();
                int UserId = await _auth.GetUserIdbyUserName(Name);
                int businessGroupId = 1;
                var encryptUpdate = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
                var bankportalApproval = await _bankPortalService.GetBankApprovalByUserId(update_BnakPortal.Mkey, UserId, businessGroupId);
                if (bankportalApproval != null)
                {

                    var bankApprovallatestupdate = new Bank_Approval_Hdr_Model
                    {
                        Mkey = update_BnakPortal.Mkey,
                        EntryDateTime = bankportalApproval.EntryDateTime,
                        LegalEntityId = bankportalApproval.NS_Internal_ID,
                        LegalEntityName = bankportalApproval.LegalEntityName,
                        ProjectId = bankportalApproval.ProjectId,
                        ProjectName = bankportalApproval.ProjectName,
                        BuildingId = bankportalApproval.BuildingId,
                        BuildingName = bankportalApproval.BuildingName,
                        TransactionType = bankportalApproval.TransactionType,
                        LastTransactionDatetime = DateTime.UtcNow,
                        Amount = update_BnakPortal.Amount,
                        DisplayText = bankportalApproval.DisplayText,
                        NS_Internal_ID = bankportalApproval.NS_Internal_ID,
                        NS_User_ID = bankportalApproval.NS_User_ID,
                        NS_User_Name = bankportalApproval.NS_User_Name,
                        AppoverID = bankportalApproval.AppoverID,
                        ApproverName = bankportalApproval.ApproverName,
                        RequestedBy = bankportalApproval.RequestedBy,
                        RequestedByName = bankportalApproval.RequestedByName,
                        ActionCode = update_BnakPortal.ActionCode,
                        ActionTime = update_BnakPortal.ActionTime,
                        ActiveFlag = bankportalApproval.ActiveFlag,
                        Status = bankportalApproval.Status,
                        Process_Flag = bankportalApproval.Process_Flag,
                        ATTRIBUTE1 = bankportalApproval.ATTRIBUTE1,
                        ATTRIBUTE2 = bankportalApproval.ATTRIBUTE2,
                        ATTRIBUTE3 = bankportalApproval.ATTRIBUTE3,
                        ATTRIBUTE4 = bankportalApproval.ATTRIBUTE4,
                        ATTRIBUTE5 = bankportalApproval.ATTRIBUTE5,
                        CREATED_BY = bankportalApproval.CREATED_BY,
                        CREATION_DATE = bankportalApproval.CREATION_DATE,
                        CREATED_BY_Name = bankportalApproval.CREATED_BY_Name,
                        LAST_UPDATED_BY = UserId,
                        LAST_UPDATED_BY_Name = Name,
                        LAST_UPDATE_DATE = DateTime.UtcNow,
                        DELETE_FLAG = bankportalApproval.DELETE_FLAG.ToString(),
                        UserId = UserId,
                        BusinessGroupId = businessGroupId
                    };


                    var responseResult = await _bankPortalService.InsertOrUpdateBankApprovalAsync(bankApprovallatestupdate);
                    if (responseResult.Contains("Update Success"))
                    {
                        responseObj.Status = "Success";
                        responseObj.Message = $"Update Success & Mkey:{update_BnakPortal.Mkey}";
                        responseObj.Data = update_BnakPortal;
                    }
                    else
                    {
                        responseObj.Status = "Error";
                        responseObj.Message = $"Update Failed & Mkey:{update_BnakPortal.Mkey}";
                        responseObj.Data = update_BnakPortal;
                    }
                }
                else
                {
                    responseObj.Status = "Sucess";
                    responseObj.Message = $"No data available against Mkey: {update_BnakPortal.Mkey}";
                    responseObj.Data = null;
                }

                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        [HttpPost("Update_Bank_Approval_Hdr_PS")]
        public async Task<IActionResult> Update_Bank_Approval_Hdr_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObj = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            string Name = string.Empty;
            //var jsonEncrypt = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
            var update_BnakPortal = _commonService.DecryptObject<Update_BnakPortalApproval>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (update_BnakPortal.UserId > 0)
            {
                UserId = update_BnakPortal.UserId;
                businessGroupId = update_BnakPortal.BusinessGroupId;
            }
            else
            {
                Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                var bankportalApproval = await _bankPortalService.GetBankApprovalByUserId(update_BnakPortal.Mkey, UserId, businessGroupId);

                if (bankportalApproval != null)
                {

                    var bankApprovallatestupdate = new Bank_Approval_Hdr_Model
                    {
                        Mkey = update_BnakPortal.Mkey,
                        EntryDateTime = bankportalApproval.EntryDateTime,
                        LegalEntityId = bankportalApproval.NS_Internal_ID,
                        LegalEntityName = bankportalApproval.LegalEntityName,
                        ProjectId = bankportalApproval.ProjectId,
                        ProjectName = bankportalApproval.ProjectName,
                        BuildingId = bankportalApproval.BuildingId,
                        BuildingName = bankportalApproval.BuildingName,
                        TransactionType = bankportalApproval.TransactionType,
                        LastTransactionDatetime = DateTime.UtcNow,
                        Amount = update_BnakPortal.Amount,
                        DisplayText = bankportalApproval.DisplayText,
                        NS_Internal_ID = bankportalApproval.NS_Internal_ID,
                        NS_User_ID = bankportalApproval.NS_User_ID,
                        NS_User_Name = bankportalApproval.NS_User_Name,
                        AppoverID = bankportalApproval.AppoverID,
                        ApproverName = bankportalApproval.ApproverName,
                        RequestedBy = bankportalApproval.RequestedBy,
                        RequestedByName = bankportalApproval.RequestedByName,
                        ActionCode = update_BnakPortal.ActionCode,
                        ActionTime = update_BnakPortal.ActionTime,
                        ActiveFlag = bankportalApproval.ActiveFlag,
                        Status = bankportalApproval.Status,
                        Process_Flag = bankportalApproval.Process_Flag,
                        ATTRIBUTE1 = bankportalApproval.ATTRIBUTE1,
                        ATTRIBUTE2 = bankportalApproval.ATTRIBUTE2,
                        ATTRIBUTE3 = bankportalApproval.ATTRIBUTE3,
                        ATTRIBUTE4 = bankportalApproval.ATTRIBUTE4,
                        ATTRIBUTE5 = bankportalApproval.ATTRIBUTE5,
                        CREATED_BY = bankportalApproval.CREATED_BY,
                        CREATION_DATE = bankportalApproval.CREATION_DATE,
                        CREATED_BY_Name = bankportalApproval.CREATED_BY_Name,
                        LAST_UPDATED_BY = UserId,
                        LAST_UPDATED_BY_Name = Name,
                        LAST_UPDATE_DATE = DateTime.UtcNow,
                        DELETE_FLAG = bankportalApproval.DELETE_FLAG.ToString(),
                        UserId = UserId,
                        BusinessGroupId = businessGroupId
                    };


                    var responseResult = await _bankPortalService.InsertOrUpdateBankApprovalAsync(bankApprovallatestupdate);
                    var Encryptrespons = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
                    var decryptrespons = _commonService.DecryptObject<Update_BnakPortalApproval>(Encryptrespons, _encryptionKey);

                    if (responseResult.Contains("Update Success"))
                    {
                        responseObj.Status = "Success";
                        responseObj.Message = $"Update Success & Mkey:{update_BnakPortal.Mkey}";
                        responseObj.Data = Encryptrespons;
                    }
                    else
                    {
                        responseObj.Status = "Error";
                        responseObj.Message = $"Update Failed & Mkey:{update_BnakPortal.Mkey}";
                        responseObj.Data = Encryptrespons;
                    }
                }
                else
                {
                    responseObj.Status = "Sucess";
                    responseObj.Message = $"No data available against Mkey: {update_BnakPortal.Mkey}";
                    responseObj.Data = null;
                }

                return Ok(responseObj);
            }
            catch (Exception ex)
            {
                responseObj.Status = "Error";
                responseObj.Message = "An error occurred during Bank Approval.";
                responseObj.Data = new { error = ex.Message };
                return StatusCode(500, responseObj);
            }
        }

        #endregion

        #region
        // Filter All Get Method 
        [Authorize]
        [HttpGet("GetBuildingDetails-Filter")]
        public async Task<IActionResult> GetBuildingDetails()
        {
            var responseObject = new ResponseObject();

            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _auth.GetUserIdbyUserName(Name);
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

                var result = await _auth.GetDataFromSpAsync<BuildingDetails>("Sp_GetDistinctBuildingDetails", parameters);
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
        [HttpPost("GetBuildingDetails-Filter_PS")]
        public async Task<IActionResult> GetBuildingDetails_PS([FromBody]jsonEncryptModel jsonEncrypt)
        {
            var responseObject = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            string Name = string.Empty;
            //var jsonEncrypt = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
            var update_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (update_BnakPortal.UserId > 0)
            {
                UserId = update_BnakPortal.UserId;
                businessGroupId = update_BnakPortal.BusinessGroupId;
            }
            else
            {
                Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                
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

                var result = await _auth.GetDataFromSpAsync<BuildingDetails>("Sp_GetDistinctBuildingDetails", parameters);
                var encryptresult = _commonService.EncryptionObje<List<BuildingDetails>>(result.ToList(), _encryptionKey);
                var Decryptresult = _commonService.DecryptObject<List<BuildingDetails>>(encryptresult, _encryptionKey);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Building Details Fetch successfully";
                    responseObject.Data = encryptresult;
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
                decimal UserId = await _auth.GetUserIdbyUserName(Name);
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

                var result = await _auth.GetDataFromSpAsync<ProjectDetails>("Sp_GetDistinctProjectDetails", parameters);
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
        [HttpPost("GetProjectDetails-Filter_PS")]
        public async Task<IActionResult> GetProjectDetails_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObject = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            string Name = string.Empty;
            //var jsonEncrypt = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
            var update_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (update_BnakPortal.UserId > 0)
            {
                UserId = update_BnakPortal.UserId;
                businessGroupId = update_BnakPortal.BusinessGroupId;
            }
            else
            {
                Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                //string Name = User.Identity.Name?.ToString();
                //decimal UserId = await _auth.GetUserIdbyUserName(Name);
                //int businessGroupId = 1;
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

                var result = await _auth.GetDataFromSpAsync<ProjectDetails>("Sp_GetDistinctProjectDetails", parameters);
                var encryptresult = _commonService.EncryptionObje<List<ProjectDetails>>(result.ToList(), _encryptionKey);
                var Decryptresult = _commonService.DecryptObject<List<ProjectDetails>>(encryptresult, _encryptionKey);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Project Details Fetch successfully";
                    responseObject.Data = encryptresult;
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
                decimal UserId = await _auth.GetUserIdbyUserName(Name);
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

                var result = await _auth.GetDataFromSpAsync<LegalEntityDetails>("Sp_GetDistinctLegalEntityDetails", parameters);
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
        [HttpPost("GetLegalEntityDetails-Filter_PS")]
        public async Task<IActionResult> GetLegalEntityDetails_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObject = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            string Name = string.Empty;
            //var jsonEncrypt = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
            var update_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (update_BnakPortal.UserId > 0)
            {
                UserId = update_BnakPortal.UserId;
                businessGroupId = update_BnakPortal.BusinessGroupId;
            }
            else
            {
                Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                //string Name = User.Identity.Name?.ToString();
                //decimal UserId = await _auth.GetUserIdbyUserName(Name);
                //int businessGroupId = 1;
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

                var result = await _auth.GetDataFromSpAsync<LegalEntityDetails>("Sp_GetDistinctLegalEntityDetails", parameters);
                var encryptresult = _commonService.EncryptionObje<List<LegalEntityDetails>>(result.ToList(), _encryptionKey);
                var Decryptresult = _commonService.DecryptObject<List<LegalEntityDetails>>(encryptresult, _encryptionKey);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Legal Entity Details Fetch successfully";
                    responseObject.Data = encryptresult;
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
        public async Task<IActionResult> GetBankDetailsFilter()
        {
            var responseObject = new ResponseObject();

            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _auth.GetUserIdbyUserName(Name);
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

                var result = await _auth.GetDataFromSpAsync<BankDetails>("Sp_GetDistinctBankDetails", parameters);
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
        [HttpPost("GetBankDetails-Filter_PS")]
        public async Task<IActionResult> GetBankDetailsFilter_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObject = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            string Name = string.Empty;
            //var jsonEncrypt = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
            var update_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (update_BnakPortal.UserId > 0)
            {
                UserId = update_BnakPortal.UserId;
                businessGroupId = update_BnakPortal.BusinessGroupId;
            }
            else
            {
                Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                //string Name = User.Identity.Name?.ToString();
                //decimal UserId = await _auth.GetUserIdbyUserName(Name);
                //int businessGroupId = 1;
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

                var result = await _auth.GetDataFromSpAsync<BankDetails>("Sp_GetDistinctBankDetails", parameters);
                var encryptresult = _commonService.EncryptionObje<List<BankDetails>>(result.ToList(), _encryptionKey);
                var Decryptresult = _commonService.DecryptObject<List<BankDetails>>(encryptresult, _encryptionKey);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Building Details Fetch successfully";
                    responseObject.Data = encryptresult;
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
                decimal UserId = await _auth.GetUserIdbyUserName(Name);
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

                var result = await _auth.GetDataFromSpAsync<AccountDetails>("Sp_GetDistinctAccountDetails", parameters);
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
        [HttpPost("GetAccountDetails-Filter_PS")]
        public async Task<IActionResult> GetAccountDetails_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObject = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            string Name = string.Empty;
            //var jsonEncrypt = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
            var update_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (update_BnakPortal.UserId > 0)
            {
                UserId = update_BnakPortal.UserId;
                businessGroupId = update_BnakPortal.BusinessGroupId;
            }
            else
            {
                Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                //string Name = User.Identity.Name?.ToString();
                //decimal UserId = await _auth.GetUserIdbyUserName(Name);
                //int businessGroupId = 1;
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

                var result = await _auth.GetDataFromSpAsync<AccountDetails>("Sp_GetDistinctAccountDetails", parameters);
                var encryptresult = _commonService.EncryptionObje<List<AccountDetails>>(result.ToList(), _encryptionKey);
                var Decryptresult = _commonService.DecryptObject<List<AccountDetails>>(encryptresult, _encryptionKey);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Account Details Fetch successfully";
                    responseObject.Data = encryptresult;
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
                decimal UserId = await _auth.GetUserIdbyUserName(Name);
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

                var result = await _auth.GetDataFromSpAsync<AccountStatus>("Sp_GetDistinctAccountStatus", parameters);
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

        [Authorize]
        [HttpPost("GetAccountStatusDetails-Filter_PS")]
        public async Task<IActionResult> GetAccountStatusDetails_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObject = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            string Name = string.Empty;
            //var jsonEncrypt = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
            var update_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (update_BnakPortal.UserId > 0)
            {
                UserId = update_BnakPortal.UserId;
                businessGroupId = update_BnakPortal.BusinessGroupId;
            }
            else
            {
                Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                //string Name = User.Identity.Name?.ToString();
                //decimal UserId = await _auth.GetUserIdbyUserName(Name);
                //int businessGroupId = 1;
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

                var result = await _auth.GetDataFromSpAsync<AccountStatus>("Sp_GetDistinctAccountStatus", parameters);
                var encryptresult = _commonService.EncryptionObje<List<AccountStatus>>(result.ToList(), _encryptionKey);
                var Decryptresult = _commonService.DecryptObject<List<AccountStatus>>(encryptresult, _encryptionKey);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = " Account Status Details Fetch successfully";
                    responseObject.Data = encryptresult;
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

        [Authorize]
        [HttpGet("GetBankAccountInfo")]
        public async Task<IActionResult> GetBankAccountInfo()
        {
            var responseObject = new ResponseObject();

            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _auth.GetUserIdbyUserName(Name);
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

                var result = await _auth.GetDataFromSpAsync<BankAccountInfo>("Sp_getBankAccountInfo", parameters);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "Bank & Account Details Fetch successfully";
                    responseObject.Data = result;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Bank & Account Details is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during  Bank Account Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [Authorize]
        [HttpPost("GetBankAccountInfo_PS")]
        public async Task<IActionResult> GetBankAccountInfo_PS([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObject = new ResponseObject();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            string Name = string.Empty;
            //var jsonEncrypt = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
            var update_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (update_BnakPortal.UserId > 0)
            {
                UserId = update_BnakPortal.UserId;
                businessGroupId = update_BnakPortal.BusinessGroupId;
            }
            else
            {
                Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }

            try
            {
                //string Name = User.Identity.Name?.ToString();
                //decimal UserId = await _auth.GetUserIdbyUserName(Name);
                //int businessGroupId = 1;
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

                var result = await _auth.GetDataFromSpAsync<BankAccountInfo>("Sp_getBankAccountInfo", parameters);
                var encryptresult = _commonService.EncryptionObje<List<BankAccountInfo>>(result.ToList(), _encryptionKey);
                var Decryptresult = _commonService.DecryptObject<List<BankAccountInfo>>(encryptresult, _encryptionKey);
                //var PassworsHash = _authMasterService.DecryptPassword(EncryptedUserDetails, keyString);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "Bank & Account Details Fetch successfully";
                    responseObject.Data = encryptresult;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Bank & Account Details is Available";
                    //responseObject.Data = new { error = ex.Message };
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during  Bank Account Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [Authorize]
        [HttpGet("GetAdditionalInformation")]
        public async Task<IActionResult> GetAdditionalInformation(string? accountNo)
        {
            var responseObject = new ResponseObject();
            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _auth.GetUserIdbyUserName(Name);
                int businessGroupId = 1;
                //var keyString = _configuration["EncryptionKey"];
                var parameters = new CommonSpParameters
                {
                    UserId = Convert.ToInt32(UserId),
                    BusinessGroupId = businessGroupId,
                    Attribute1 = string.IsNullOrEmpty(accountNo) ? null:accountNo,
                    Attribute2 = null,
                    Attribute3 = null,
                    Attribute4 = null
                };
                var result = await _bankPortalService.GetAdditionalInformationAsync(parameters);
                if(result!=null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "Additional Information Details Fetch successfully";
                    responseObject.Data = result;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Additional Information Details is Available";
                }
                return Ok(responseObject);
            }
            catch(Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during Account Status Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [Authorize]
        [HttpPost("GetAdditionalInformation_NT")]

        public async Task<IActionResult> GetAdditionalInformation_NT([FromBody] jsonEncryptModel jsonEncrypt)   //  string? accountNo  
        {
            var responseObject = new ResponseObject();
            try
            {
                string Name = User.Identity.Name?.ToString();
                decimal UserId = await _auth.GetUserIdbyUserName(Name);
                //var loginecrypt = _commonService.EncryptionObje<string>(accountNo, _encryptionKey);
                var accountNo = _commonService.DecryptObject<string>(jsonEncrypt.jsonEncrypt, _encryptionKey);
                int businessGroupId = 1;
                //var keyString = _configuration["EncryptionKey"];
                var parameters = new CommonSpParameters
                {
                    UserId = Convert.ToInt32(UserId),
                    BusinessGroupId = businessGroupId,
                    Attribute1 = string.IsNullOrEmpty(accountNo) ? null : accountNo,
                    Attribute2 = null,
                    Attribute3 = null,
                    Attribute4 = null
                };
                var result = await _bankPortalService.GetAdditionalInformationAsync(parameters);
                var EncryptcompletedList = _commonService.EncryptionObje<List<BankPortal_model>>(result.ToList(), _encryptionKey);
                var DeCryptcompletedList = _commonService.DecryptObject<List<BankPortal_model>>(EncryptcompletedList, _encryptionKey);
                if (result != null && result.Any())
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "Additional Information Details Fetch successfully";
                    responseObject.Data = EncryptcompletedList;    //result;
                }
                else
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Additional Information Details is Available";
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

        #endregion

        #region
        // Bank Account Summary Portal Dashboard
        [HttpGet("BankPortal_Dashboard")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBank_Acc_SubSidairy_Dashboard()
        {
            var responseObject = new ResponseObject();
            var bankAccSummaryResponseList = new List<BankAccountSummary_ResponseModel>();

            try
            {
                var activeSubs =
                    await _bankPortalService.GetBankAccountSummaryActiveAsync();

                foreach (var item in activeSubs)
                {
                    if (item.subid <= 0)
                        continue;

                    var bkSumm_Map =
                        await _bankPortalService
                            .GetMapBankAccountSummary_IntoBankAccountSummary_BySubId(item);

                    // ✅ MUST initialize
                    bkSumm_Map.bankAccountSummary_Ns ??=
                        new List<BankAccountSummary_Ns_Model>();

                    var bankAccSummary_NSList =
                        new List<BankAccountSummary_Ns_Model>();

                    var summaryBySubId =
                        await _bankPortalService
                            .GetBankAccountSummaryBySubIdAsync(item.subid);

                    foreach (var subItem in summaryBySubId)
                    {
                        if (subItem.projectid > 0 || subItem.projectid== 0)
                        {
                            var details =
                                await _bankPortalService
                                    .GetBankAccountSummaryBySubId_ProjectIdAsync(
                                        subItem.subid,
                                        subItem.projectid
                                    );

                            subItem.bank_Acc_Details_NS_ =
                                details?.ToList() ?? new List<Bank_Acc_Details_NS_Model>();
                        }

                        bankAccSummary_NSList.Add(subItem);
                    }

                    // ✅ Safe AddRange
                    bkSumm_Map.bankAccountSummary_Ns.AddRange(bankAccSummary_NSList);

                    bankAccSummaryResponseList.Add(bkSumm_Map);
                }

                responseObject.Status = "Ok";
                responseObject.Message =
                    bankAccSummaryResponseList.Any()
                        ? "Bank Account Summary Dashboard Fetch successfully"
                        : "No Bank Account Summary Dashboard is Available";

                responseObject.Data = bankAccSummaryResponseList;

                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message =
                    "An error occurred during Account Status Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [HttpPost("BankPortal_Dashboard_PS")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBank_Acc_SubSidairy_Dashboard([FromBody] jsonEncryptModel jsonEncrypt)
        {
            var responseObject = new ResponseObject();
            var bankAccSummaryResponseList = new List<BankAccountSummary_ResponseModel>();
            //var jsonEncrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, _encryptionKey);
            int? UserId = 0;
            int? businessGroupId = 0;
            string Name = string.Empty;
            //var jsonEncrypt = _commonService.EncryptionObje<Update_BnakPortalApproval>(update_BnakPortal, _encryptionKey);
            var dashboard_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
            if (dashboard_BnakPortal.UserId > 0)
            {
                UserId = dashboard_BnakPortal.UserId;
                businessGroupId = dashboard_BnakPortal.BusinessGroupId;
            }
            else
            {
                Name = User.Identity.Name?.ToString();
                UserId = await _auth.GetUserIdbyUserName(Name);
                businessGroupId = 1;
            }
            try
            {
                var activeSubs =
                    await _bankPortalService.GetBankAccountSummaryActiveAsync();

                foreach (var item in activeSubs)
                {
                    if (item.subid <= 0)
                        continue;

                    var bkSumm_Map =
                        await _bankPortalService
                            .GetMapBankAccountSummary_IntoBankAccountSummary_BySubId(item);

                    // ✅ MUST initialize
                    bkSumm_Map.bankAccountSummary_Ns ??=
                        new List<BankAccountSummary_Ns_Model>();

                    var bankAccSummary_NSList =
                        new List<BankAccountSummary_Ns_Model>();

                    var summaryBySubId =
                        await _bankPortalService
                            .GetBankAccountSummaryBySubIdAsync(item.subid);

                    foreach (var subItem in summaryBySubId)
                    {
                        if (subItem.projectid > 0 || subItem.projectid == 0)
                        {
                            var details =
                                await _bankPortalService
                                    .GetBankAccountSummaryBySubId_ProjectIdAsync(
                                        subItem.subid,
                                        subItem.projectid
                                    );

                            subItem.bank_Acc_Details_NS_ =
                                details?.ToList() ?? new List<Bank_Acc_Details_NS_Model>();
                        }

                        bankAccSummary_NSList.Add(subItem);
                    }

                    // ✅ Safe AddRange
                    bkSumm_Map.bankAccountSummary_Ns.AddRange(bankAccSummary_NSList);

                    bankAccSummaryResponseList.Add(bkSumm_Map);
                }

                // Encryption List of Data 
                var encryptresult = _commonService.EncryptionObje<List<BankAccountSummary_ResponseModel>>(bankAccSummaryResponseList.ToList(), _encryptionKey);
                var Decryptresult = _commonService.DecryptObject<List<BankAccountSummary_ResponseModel>>(encryptresult, _encryptionKey);


                responseObject.Status = "Ok";
                responseObject.Message =
                    bankAccSummaryResponseList.Any()
                        ? "Bank Account Summary Dashboard Fetch successfully"
                        : "No Bank Account Summary Dashboard is Available";

                responseObject.Data = encryptresult;

                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message =
                    "An error occurred during Account Status Details Fetching";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [HttpPost("AllRefresh_Bank_Acc_Details")]
        public async Task<IActionResult> GetAllRefresh_Bank_Acc_Details([FromBody] jsonEncryptModel jsonEncrypt)  // jsonEncryptModel jsonEncrypt
        {
            var responseObject = new ResponseObject();
            var Bank_Acc_Summ_NS = new List<Bank_Acc_Summ_NS>();
            var bankAccSummaryResponse = new List<Bank_Acc_Subsidiary_Summ_NS>();
            var BankDetails_BySubsidiary_NS = new List<BankDetails_By_Subsidiary>();
            try
            {
                string Name = User.Identity.Name?.ToString();
                //decimal UserId = await _auth.GetUserIdbyUserName(Name);
                decimal UserId = 0;
                string userid = string.Empty;
               // string Name = string.Empty;
                int? businessGroupId = 0;
                //var encrypt_BankPortal = _commonService.EncryptionObje<CommoninputResponse>(new CommoninputResponse { UserId = Convert.ToInt32(UserId), BusinessGroupId = 1 }, _encryptionKey);
                var dashboard_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, _encryptionKey);  //jsonEncrypt.jsonEncrypt
                if (dashboard_BnakPortal.UserId > 0)
                {
                    UserId = Convert.ToDecimal(dashboard_BnakPortal.UserId);
                    userid = Convert.ToString(UserId);
                    businessGroupId = dashboard_BnakPortal.BusinessGroupId;
                }
                else
                {
                    //Name = User.Identity.Name?.ToString();
                    UserId = await _auth.GetUserIdbyUserName(Name);
                    userid = Convert.ToString(UserId);
                    businessGroupId = 1;
                }

                var BankAccsubsidiaryresponse = await _refreshBAD.TriggerBank_Acc_Subsidiary_Summ_NSAsync_L1();
                bankAccSummaryResponse = BankAccsubsidiaryresponse.Data as List<Bank_Acc_Subsidiary_Summ_NS>;
                Console.WriteLine("inserted Previouse Data in History Table successfully.");
                var makeHistroryStatus = await _refreshBAD.ProcessBankAccSubsidiarySummaryMakeHistoryAsync_L2(UserId, businessGroupId);

                foreach (var bnksubitem in bankAccSummaryResponse)
                {
                    bnksubitem.Created_By = Convert.ToDecimal(UserId);
                    bnksubitem.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin": Name;
                    // bnksubitem.Mkey = resultresponse.Result.Mkey;
                    //bnksubitem.subid = item.SubId;
                    Console.WriteLine($"Subsidiary: {bnksubitem.subsidiary}, account_bal: {bnksubitem.account_bal},closing_balance_as_per_bank_statement: {bnksubitem.closing_balance_as_per_bank_statement}, current_account_balance_as_per_bank_book: {bnksubitem.current_account_balance_as_per_bank_book}");
                    var resultbnkSubresponses = await _refreshBAD.AddBank_Acc_Subsidiary_Summ_NSSummary_L1(bnksubitem , userid);
                    var bank_Acc_Subsidiary_Log_Model = await _refreshBAD.MapBank_Acc_Subsidiary_Summ_NS_ToLogModel(bnksubitem);
                    if (resultbnkSubresponses.Message.Contains("Success"))
                    {
                        Console.WriteLine("Add Bank Acc Summary Method End");
                        Console.WriteLine("L3 Method And Logic Execution Started");
                        bank_Acc_Subsidiary_Log_Model.Status = "Success";
                        bank_Acc_Subsidiary_Log_Model.Message = " Bank Account Subsidiary Record inserted successfully.";
                        bank_Acc_Subsidiary_Log_Model.ActionName = "AddBank_Acc_Summ_Async_L1";
                        bank_Acc_Subsidiary_Log_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                        //bank_Acc_Subsidiary_Log_Model.CREATED_BY_Name = Name;
                        try
                        {

                            var mapbankAccLog = await _refreshBAD.MapBank_Acc_Log_NS_Model(bank_Acc_Subsidiary_Log_Model);
                            mapbankAccLog.CREATED_BY_Name = Name;
                            mapbankAccLog.CREATED_BY= UserId;
                            var bankdetailsLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog ,userid);

                            // This Line of Code is for Testing Purpose to check the L3 Logic when L1 is Success                                                                                                                                                                                                                
                            //var bankdetailsLog = "Success";      //await CommonService.BankPortalService.AddBank_Acc_SubSidiary_summ_LogAsyncL1(bank_Acc_Subsidiary_Log_Model);
                            // End

                            var LoginStatus = bankdetailsLog.Contains("Success") ? "Success" : "Failure";
                            if (LoginStatus.Contains("Success"))
                            {

                                var subsidiaryresponse = await _refreshBAD.TriggerBankDetail_By_SusidiaryidAsync_L3(bnksubitem.subid);
                                BankDetails_BySubsidiary_NS = subsidiaryresponse.Data as List<BankDetails_By_Subsidiary>;
                                foreach (var subitem in BankDetails_BySubsidiary_NS)
                                {
                                    subitem.Created_By = Convert.ToDecimal(UserId);
                                    subitem.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin" :Name ;
                                    subitem.Mkey = resultbnkSubresponses.Mkey;
                                    subitem.SubId = bnksubitem.subid;
                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {bnksubitem.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                    Console.WriteLine("Add Sub SubdiaryBank Details Execution Started");
                                    var resultSubresponses = await _refreshBAD.AddSubSidiaryBankDetailsSummary_L3(subitem ,userid);
                                    string subsidiaryLog = resultSubresponses.Message.Contains("Success") ? "Success" : "Failure";
                                    subitem.SrNo = resultSubresponses.SrNo;
                                    var bankDetailsAccountLog_Model = await _refreshBAD.MapBank_Details_By_Subsidiary_ToLogModel(subitem);
                                    Console.WriteLine("Add Sub SubdiaryBank Details Method End");
                                    if (resultSubresponses.Message.Contains("Success"))
                                    {
                                        Console.WriteLine("Add Bank Acc Summary Method End");
                                        Console.WriteLine("L3 Method And Logic Execution Started");
                                        bankDetailsAccountLog_Model.Status = "Success";
                                        bankDetailsAccountLog_Model.Message = " Bank Details Subsidiary Account Summary Record inserted successfully.";
                                        bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                        bankDetailsAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                                        try
                                        {
                                            //var bankDetailsLogresponse = await CommonService.BankPortalService.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model);
                                            var mapbankAccLog_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                            mapbankAccLog.CREATED_BY= UserId;
                                            mapbankAccLog.CREATED_BY_Name= string.IsNullOrEmpty(Name) ? "Admin": Name;
                                            var LogBankDetailsSubsidairyStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_L2 ,userid);

                                            // This Line of Code is for Testing Purpose to check the L3 Logic when L2 is Success
                                            // var LogBankDetailsSubsidairyStatus_L3 = "Success";       // bankDetailsLogresponse.Contains("Success") ? "Success" : "Failure";
                                            // End
                                            if (LogBankDetailsSubsidairyStatus_L3.Contains("Success"))
                                            {
                                                Console.WriteLine("Bank Details SubSidiary Acc Summary L3 inserted successfully.");
                                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                Console.WriteLine("Record inserted successfully.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                bankDetailsAccountLog_Model.Status = "Error";
                                                bankDetailsAccountLog_Model.Message = "Bank Details SubSidiary Acc Summary L3.Insert Failed";
                                                bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                                bankDetailsAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                                                var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                                mapbankAccLog_Fail_L2.CREATED_BY= UserId;
                                                mapbankAccLog_Fail_L2.CREATED_BY_Name= string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                                var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2 , userid);
                                                Console.WriteLine("Failed to insert record.");
                                            }
                                            // Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                            // Console.WriteLine("Record inserted successfully.");
                                        }
                                        catch (Exception ex)
                                        {
                                            string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {subitem.Subsidiary}, Project: {subitem.Project}. Error: {ex.Message}";
                                            Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, Project: {subitem.Project},closing_balance_as_per_bank_statement: {subitem.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {subitem.Current_Account_Balance_As_Per_Bank_Book}");
                                            Console.WriteLine("Failed to insert record.");
                                            bankDetailsAccountLog_Model.Status = "Error";
                                            bankDetailsAccountLog_Model.Message = errorMessage;
                                            bankDetailsAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                            bankDetailsAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                                            var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(bnksubitem.Created_By, 1);
                                            var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                            mapbankAccLog_Fail_L2.CREATED_BY= UserId;
                                            mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                            var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);

                                            //var ExceptiombankdetailsLog = await CommonService.BankPortalService.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model);
                                            // Console.WriteLine("Exception Occurred While Mapping Bank Details SubSidiary To Log Model: " + ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                        var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(bnksubitem.Created_By, 1);

                                        Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                        bankDetailsAccountLog_Model.Status = "Error";
                                        bankDetailsAccountLog_Model.Message = "Bank Details SubSidiary Acc Summary L3.Insert Failed";
                                        bankDetailsAccountLog_Model.ActionName = "Add Bank Details Subsidairy _Acc_Summ_Async_L3";
                                        bankDetailsAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                                        var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                        mapbankAccLog_Fail_L2.CREATED_BY= UserId;
                                        mapbankAccLog_Fail_L2.CREATED_BY_Name= string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                        var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);
                                        Console.WriteLine("Failed to insert record.");
                                    }
                                }
                                //Console.WriteLine($"Subsidiary: {item.Subsidiary}, Account Number: {item.custrecord_htl_bank_account_number}, Description: {item.description}, Account Balance: {item.account_bal}");
                                //Console.WriteLine("Record inserted successfully.");

                                Console.WriteLine("Bank Acc Summary Log inserted successfully.");
                                Console.WriteLine($"Subsidiary: {bnksubitem.subsidiary}, account_bal: {bnksubitem.account_bal},closing_balance_as_per_bank_statement: {bnksubitem.closing_balance_as_per_bank_statement}, current_account_balance_as_per_bank_book: {bnksubitem.current_account_balance_as_per_bank_book}");
                                Console.WriteLine("Record inserted successfully.");
                            }
                            else
                            {
                                Console.WriteLine("Bank Acc Subsidiary Summary L1 Log. Insert Failed");
                                Console.WriteLine($"Subsidiary: {bnksubitem.subsidiary}, account_bal: {bnksubitem.account_bal},closing_balance_as_per_bank_statement: {bnksubitem.closing_balance_as_per_bank_statement}, custrecord_htl_bank_account_number: {bnksubitem.custrecord_htl_bank_account_number}");
                                var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(bnksubitem.Created_By, 1);
                                mapbankAccLog.Status = "Error";
                                mapbankAccLog.Message = "Bank Acc Subsidiary Summary L1 Log. Insert Failed";
                                mapbankAccLog.ActionName = "Add Bank Details Subsidairy _Acc_Summ_Async_L3";
                                mapbankAccLog.MethodName = "GetAllRefresh_Bank_Acc_Details";
                                mapbankAccLog.CREATED_BY= UserId;
                                mapbankAccLog.CREATED_BY_Name= string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                // var mapbankAccLog_Fail_L2 = await CommonService.BankPortalService.MapBank_Acc_Log_NS_Model_L3(mapbankAccLog);
                                var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog, userid);


                                Console.WriteLine("Failed to insert record.");
                            }
                        }
                        catch (Exception ex)
                        {
                            bank_Acc_Subsidiary_Log_Model.Status = "Error";
                            bank_Acc_Subsidiary_Log_Model.Message = ex.Message;
                            bank_Acc_Subsidiary_Log_Model.ActionName = "AddBank_Acc_Summ_Async_L1";
                            bank_Acc_Subsidiary_Log_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                            var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(bnksubitem.Created_By, 1);
                            // var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_SubSidiary_summ_LogAsyncL1(bank_Acc_Subsidiary_Log_Model);
                            var mapbankAccLog_Fail_L1 = await _refreshBAD.MapBank_Acc_Log_NS_Model(bank_Acc_Subsidiary_Log_Model);
                            mapbankAccLog_Fail_L1.CREATED_BY= UserId;
                            mapbankAccLog_Fail_L1.CREATED_BY_Name= string.IsNullOrEmpty(Name) ? "Admin" : Name;
                            var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L1, userid);

                        }
                    }
                    else
                    {
                        Console.WriteLine($"Subsidiary: {bnksubitem.subsidiary}, account_bal: {bnksubitem.account_bal},closing_balance_as_per_bank_statement: {bnksubitem.closing_balance_as_per_bank_statement}, custrecord_htl_bank_account_number: {bnksubitem.custrecord_htl_bank_account_number}");
                        Console.WriteLine("Failed to insert record.");
                        bank_Acc_Subsidiary_Log_Model.Status = "Error";
                        bank_Acc_Subsidiary_Log_Model.Message = "Failed to insert record.";
                        bank_Acc_Subsidiary_Log_Model.ActionName = "AddBank_Acc_Summ_Async_L1";
                        bank_Acc_Subsidiary_Log_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                        //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_SubSidiary_summ_LogAsyncL1(bank_Acc_Subsidiary_Log_Model);
                        var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(bnksubitem.Created_By, 1);
                        var mapbankAccLog = await _refreshBAD.MapBank_Acc_Log_NS_Model(bank_Acc_Subsidiary_Log_Model);
                        mapbankAccLog.CREATED_BY= UserId;
                        mapbankAccLog.CREATED_BY_Name= string.IsNullOrEmpty(Name) ? "Admin" : Name;
                        var bankdetailsLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog, userid);
                        WriteErrorToTextFile("Failed to insert Bank Acc Subsidiary Summary L1 record for Subsidiary: " + bnksubitem.subsidiary);
                    }
                }


                string storeprocedure = "[dbo].[sp_GetWhatsAppTemplate_Details]";  // Example stored procedure name to get the String Query but Currently not used.
                Console.WriteLine("L2 Method And Logic Execution Started");
                var response = _refreshBAD.TriggerBankSummary_BY_NetSuiteQlAsync_L2();
                Bank_Acc_Summ_NS = (List<Bank_Acc_Summ_NS>)response.Result.Data;
                //Console.WriteLine(strQuery.);

                string templateName = string.Empty;
                foreach (var item in Bank_Acc_Summ_NS)
                {
                    item.CreatedBy = Convert.ToDecimal(UserId);
                    item.CreatedByName = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                    Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                    Console.WriteLine("Add Bank Acc SummaryMethod Execution Started");
                    var resultresponse = await _refreshBAD.AddBank_Acc_Summ_Async_L2(item, userid);
                    string resultstatus = resultresponse.Message.Contains("Success") ? "Success" : "Failure";
                    var bankAccountLog_Model = await _refreshBAD.MapBank_Acc_Summ_NS_ToLogModel(item);
                    if (resultstatus.Contains("Success"))
                    {
                        try
                        {
                            Console.WriteLine("Add Bank Acc Summary Method End");
                            Console.WriteLine("L3 Method And Logic Execution Started");
                            bankAccountLog_Model.Status = "Success";
                            bankAccountLog_Model.Message = "Record inserted successfully.";
                            bankAccountLog_Model.ActionName = "AddBank_Acc_Summ_Async_L2";
                            bankAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";

                            //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                            var mapbankAccLog = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                            mapbankAccLog.CREATED_BY= UserId;
                            mapbankAccLog.CREATED_BY_Name= string.IsNullOrEmpty(Name) ? "Admin" : Name;
                            var bankdetailsLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog, userid);



                            var LoginStatus = bankdetailsLog.Contains("Success") ? "Success" : "Failure";
                            if (LoginStatus.Contains("Success"))
                            {
                                Console.WriteLine("Bank Acc Summary Log inserted successfully.");
                                var subsidiaryresponse = await _refreshBAD.TriggerBankDetail_By_SusidiaryidAsync_L3(item.SubId);
                                BankDetails_BySubsidiary_NS = subsidiaryresponse.Data as List<BankDetails_By_Subsidiary>;
                                foreach (var subitem in BankDetails_BySubsidiary_NS)
                                {
                                    subitem.Created_By = Convert.ToDecimal(UserId);
                                    subitem.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                    subitem.Mkey = resultresponse.Mkey;
                                    subitem.SubId = item.SubId;
                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                    Console.WriteLine("Add Sub SubdiaryBank Details Execution Started");
                                    //var resultSubresponses = await CommonService.BankPortalService.AddSubSidiaryBankDetailsSummary_L3(subitem);
                                    var resultSubresponses = "Failure";
                                    //string subsidiaryLog = resultSubresponses.Message.Contains("Failure") ? "Success" : "Failure";
                                    //subitem.SrNo = resultSubresponses.SrNo;
                                    var bankDetailsAccountLog_Model = await _refreshBAD.MapBank_Details_By_Subsidiary_ToLogModel(subitem);
                                    Console.WriteLine("Add Sub SubdiaryBank Details Method End");
                                    if (resultSubresponses.Contains("Success"))
                                    {
                                        try
                                        {
                                            Console.WriteLine("Add Bank Acc Summary Method End");
                                            Console.WriteLine("L3 Method And Logic Execution Started");
                                            bankDetailsAccountLog_Model.Status = "Success";
                                            bankDetailsAccountLog_Model.Message = " Bank Details Subsidiary Account Summary Record inserted successfully.";
                                            bankDetailsAccountLog_Model.ActionName = "Add Bank Details Subsidairy _Acc_Summ_Async_L3";
                                            bankDetailsAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                                            //bankDetailsAccountLog_Model.CREATED_BY= UserId;
                                            var bankDetailsLogresponse = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                            var LogBankDetailsSubsidairyStatus_L3 = bankDetailsLogresponse.Contains("Success") ? "Success" : "Failure";
                                            if (LogBankDetailsSubsidairyStatus_L3.Contains("Success"))
                                            {
                                                Console.WriteLine("Bank Details SubSidiary Acc Summary L3 inserted successfully.");
                                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                Console.WriteLine("Record inserted successfully.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                Console.WriteLine("Failed to insert record.");
                                            }
                                            Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                            Console.WriteLine("Record inserted successfully.");
                                        }
                                        catch (Exception ex)
                                        {
                                            string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                            Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                            Console.WriteLine("Failed to insert record.");
                                            bankDetailsAccountLog_Model.Status = "Error";
                                            bankDetailsAccountLog_Model.Message = errorMessage;
                                            bankDetailsAccountLog_Model.ActionName = "AddBank_Acc_Summ_Async_L2";
                                            bankDetailsAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                                            var ExceptiombankdetailsLog = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                            // Console.WriteLine("Exception Occurred While Mapping Bank Details SubSidiary To Log Model: " + ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                        Console.WriteLine("Record inserted successfully.");

                                        //var makeHistroryStatus = CommonService.BankPortalService.ProcessBankAccSubsidiarySummaryMakeHistoryAsync_L2(subitem.Created_By, 1);
                                    }
                                }
                                Console.WriteLine($"Subsidiary: {item.Subsidiary}, Account Number: {item.custrecord_htl_bank_account_number}, Description: {item.description}, Account Balance: {item.account_bal}");
                                Console.WriteLine("Record inserted successfully.");
                            }
                            else
                            {
                                var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                bankAccountLog_Model.Status = "Error";
                                bankAccountLog_Model.Message = "Failed to insert Bank Acc Summary Log.";
                                bankAccountLog_Model.ActionName = "AddBank_Acc_Summ_Async_L2";
                                bankAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                                var mapbankAcc_FailedLog = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                mapbankAcc_FailedLog.CREATED_BY= UserId;
                                mapbankAcc_FailedLog.CREATED_BY_Name= string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                var bankdetailsFailedLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAcc_FailedLog, userid);
                                Console.WriteLine("Failed to insert Bank Acc Summary Log.");

                            }

                        }
                        catch (Exception ex)
                        {
                            string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                            Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                            Console.WriteLine("Failed to insert record.");
                            bankAccountLog_Model.Status = "Error";
                            bankAccountLog_Model.Message = errorMessage;
                            bankAccountLog_Model.ActionName = "AddBank_Acc_Summ_Async_L2";
                            bankAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";
                            //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                            var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                            var mapbankAccLog = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                            var bankdetailsLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog , userid);
                            Console.WriteLine("Exception Occurred While Mapping Bank Acc Summary To Log Model: " + ex.Message);
                        }

                    }
                    else
                    {
                        string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                        Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                        Console.WriteLine("Failed to insert record.");
                        bankAccountLog_Model.Status = "Error";
                        bankAccountLog_Model.Message = errorMessage;
                        bankAccountLog_Model.ActionName = "AddBank_Acc_Summ_Async_L2";
                        bankAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";

                        // var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                        var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                        var mapbankAccLog = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                        mapbankAccLog.CREATED_BY= UserId;
                        mapbankAccLog.CREATED_BY_Name= string.IsNullOrEmpty(Name) ? "Admin" : Name;
                        var bankdetailsLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog, userid);
                        WriteErrorToTextFile(errorMessage);
                    }
                }
                //Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                Console.WriteLine("All records marked with DELETE_FLAG = 'Y' have been deleted successfully.");
                var delete_FlagStatus = _refreshBAD.ProcessBankAccSubsidiarySummaryDeleteDetailsAsync(UserId, businessGroupId);




                //var parameters = new CommonSpParameters
                //{
                //    UserId = Convert.ToInt32(UserId),
                //    BusinessGroupId = businessGroupId,
                //    Attribute1 = null,
                //    Attribute2 = null,
                //    Attribute3 = null,
                //    Attribute4 = null
                //};
                //var result = await _bankPortalService.Refresh_All_Bank_Acc_Details(parameters);
                responseObject.Status = "Ok";
                responseObject.Message = "Bank Account Details Refreshed Successfully";
                responseObject.Data = null;
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during Refreshing Bank Account Details";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        [HttpPost("Refresh_Bank_Acc_Details_ById")]
        public async Task<IActionResult> Refresh_Bank_Acc_Details_ById([FromBody] RefreshBankAccDetailsByIdRequest request)
        {
            var responseObject = new ResponseObject();
            var checkresponseObject = new ResponseObject();
            var Bank_Acc_Summ_NS = new List<Bank_Acc_Summ_NS>();
            var Bank_Acc_Summ_NS_Linq = new List<Bank_Acc_Summ_NS>();
            var bankAccSummaryResponse = new List<Bank_Acc_Subsidiary_Summ_NS>();
            var bank_Acc_Subsidiarydetails = new Bank_Acc_Subsidiary_Summ_NS();
            var bank_Acc_details = new BankDetails_By_Subsidiary();
            var bank_Acc_sum_ns = new Bank_Acc_Summ_NS();
            var BankDetails_BySubsidiary_NS = new List<BankDetails_By_Subsidiary>();
            try
            {

                string Name = User.Identity.Name?.ToString();
                //decimal UserId = await _auth.GetUserIdbyUserName(Name);
                decimal UserId = 0;
                string userid = string.Empty;
                // string Name = string.Empty;
                int? businessGroupId = 0;
                var encrypt_BankPortal = _commonService.EncryptionObje<RefreshBankAccDetailsByIdRequest>(request, _encryptionKey);
                var dashboard_BnakPortal = _commonService.DecryptObject<RefreshBankAccDetailsByIdRequest>(encrypt_BankPortal, _encryptionKey);  //jsonEncrypt.jsonEncrypt
                if (dashboard_BnakPortal.UserId > 0)
                {
                    UserId = Convert.ToDecimal(dashboard_BnakPortal.UserId);
                    userid = Convert.ToString(UserId);
                    businessGroupId = dashboard_BnakPortal.BusinessGroupId;
                }
                else
                {
                    //Name = User.Identity.Name?.ToString();
                    UserId = await _auth.GetUserIdbyUserName(Name);
                    userid = Convert.ToString(UserId);
                    businessGroupId = 1;
                }
                bool HasSubId = request.SubId > 0;
                bool HasProjectId_Null = request.ProjectId == null ? true: false;
                bool HasProjectId = request.ProjectId > 0 ? true: false;
                bool HasProjectId_Zero = request.ProjectId ==0 ? true: false;
                bool HasAccountNumber = !string.IsNullOrWhiteSpace(request.BankAccountNumber);
                
                if (HasSubId && HasProjectId_Null && !HasAccountNumber)
                {
                    var BankAccsubsidiaryresponse = await _refreshBAD.TriggerBank_Acc_Subsidiary_Summ_NSAsync_L1();
                    bankAccSummaryResponse = BankAccsubsidiaryresponse.Data as List<Bank_Acc_Subsidiary_Summ_NS>;
                    Console.WriteLine("inserted Previouse Data in History Table successfully.");
                    bank_Acc_Subsidiarydetails = bankAccSummaryResponse?.FirstOrDefault(x => x.subid == request.SubId);
                    var bank_Acc_Subsidiary_Log_Model = await _refreshBAD.MapBank_Acc_Subsidiary_Summ_NS_ToLogModel(bank_Acc_Subsidiarydetails);
                    bank_Acc_Subsidiary_Log_Model.Status = "Success";
                    bank_Acc_Subsidiary_Log_Model.Message = " Bank Account Subsidiary Record inserted successfully.";
                    bank_Acc_Subsidiary_Log_Model.ActionName = "UpdateBank_Acc_Summ_Async_L1";
                    bank_Acc_Subsidiary_Log_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                    var mapbankAccLog = await _refreshBAD.MapBank_Acc_Log_NS_Model(bank_Acc_Subsidiary_Log_Model);
                    mapbankAccLog.CREATED_BY_Name = Name;
                    mapbankAccLog.CREATED_BY = UserId;
                    var bankdetailsLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog, userid);


                    var LoginStatus = bankdetailsLog.Contains("Success") ? "Success" : "Failure";
                    if (LoginStatus.Contains("Success"))
                    {
                        
                        
                        bank_Acc_Subsidiarydetails.Last_Updated_By_Name = Name;
                        var bank_acc_subsidiaryStatus = await _refreshBAD.UpdateBank_Acc_Subsidiary_Summ_NsSummary_L1(bank_Acc_Subsidiarydetails!, userid ,Name);
                        var bnkStatus = bank_acc_subsidiaryStatus.Message.Contains("Update Success with History") ? "Success" : "Failure";
                        if (bnkStatus.Contains("Failure"))
                        {
                            bnkStatus = bank_acc_subsidiaryStatus.Message.Contains("Insert Success") ? "Success" : "Failure";
                        }
                        
                        if (bank_Acc_Subsidiarydetails.subid > 0 && bnkStatus.Contains("Success"))
                        {
                            var subsidiaryresponse = await _refreshBAD.TriggerBankDetail_By_SusidiaryidAsync_L3(bank_Acc_Subsidiarydetails.subid);
                            BankDetails_BySubsidiary_NS = subsidiaryresponse.Data as List<BankDetails_By_Subsidiary>;
                            foreach (var subitem in BankDetails_BySubsidiary_NS)
                            {
                                subitem.Created_By = Convert.ToDecimal(UserId);
                                subitem.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                subitem.Mkey = bank_acc_subsidiaryStatus.Mkey;
                                subitem.SubId = bank_Acc_Subsidiarydetails.subid;
                                //Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {bnksubitem.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                //Console.WriteLine("Add Sub SubdiaryBank Details Execution Started");
                                var resultSubresponses = await _refreshBAD.Update_SubSidiaryBankDetailsSummary_L3(subitem, userid ,Name);
                                string subsidiaryLog = resultSubresponses.Message.Contains("Update Success") ? "Success" : "Failure";
                                if (subsidiaryLog.Contains("Failure"))
                                {
                                    subsidiaryLog = resultSubresponses.Message.Contains("Insert Success") ? "Success" : "Failure";
                                }
                                subitem.SrNo = resultSubresponses.SrNo;
                                var bankDetailsAccountLog_Model = await _refreshBAD.MapBank_Details_By_Subsidiary_ToLogModel(subitem);
                                Console.WriteLine("Add Sub SubdiaryBank Details Method End");
                                if (resultSubresponses.Message.Contains("Success"))
                                {
                                    Console.WriteLine("Add Bank Acc Summary Method End");
                                    Console.WriteLine("L3 Method And Logic Execution Started");
                                    bankDetailsAccountLog_Model.Status = "Success";
                                    bankDetailsAccountLog_Model.Message = " Bank Details Subsidiary Account Summary Record inserted successfully.";
                                    bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                    bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                    try
                                    {
                                        //var bankDetailsLogresponse = await CommonService.BankPortalService.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model);
                                        var mapbankAccLog_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                        mapbankAccLog_L2.CREATED_BY = UserId;
                                        mapbankAccLog_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                        var LogBankDetailsSubsidairyStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_L2, userid);

                                        // This Line of Code is for Testing Purpose to check the L3 Logic when L2 is Success
                                        // var LogBankDetailsSubsidairyStatus_L3 = "Success";       // bankDetailsLogresponse.Contains("Success") ? "Success" : "Failure";
                                        // End
                                        if (LogBankDetailsSubsidairyStatus_L3.Contains("Success"))
                                        {
                                            Console.WriteLine("Bank Details SubSidiary Acc Summary L3 inserted successfully.");
                                            Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                            Console.WriteLine("Record inserted successfully.");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                            Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                            bankDetailsAccountLog_Model.Status = "Error";
                                            bankDetailsAccountLog_Model.Message = "Bank Details SubSidiary Acc Summary L3.Insert Failed";
                                            bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                            bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                            var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                            mapbankAccLog_Fail_L2.CREATED_BY = UserId;
                                            mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                            var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);
                                            Console.WriteLine("Failed to insert record.");
                                        }
                                        // Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                        // Console.WriteLine("Record inserted successfully.");
                                    }
                                    catch (Exception ex)
                                    {
                                        string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {subitem.Subsidiary}, Project: {subitem.Project}. Error: {ex.Message}";
                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, Project: {subitem.Project},closing_balance_as_per_bank_statement: {subitem.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {subitem.Current_Account_Balance_As_Per_Bank_Book}");
                                        Console.WriteLine("Failed to insert record.");
                                        bankDetailsAccountLog_Model.Status = "Error";
                                        bankDetailsAccountLog_Model.Message = errorMessage;
                                        bankDetailsAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                        bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                        var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(bank_Acc_Subsidiarydetails.Created_By, 1);
                                        var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                        mapbankAccLog_Fail_L2.CREATED_BY = UserId;
                                        mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                        var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);

                                        //var ExceptiombankdetailsLog = await CommonService.BankPortalService.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model);
                                        // Console.WriteLine("Exception Occurred While Mapping Bank Details SubSidiary To Log Model: " + ex.Message);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                    var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(request.UserId, 1);

                                    Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                    bankDetailsAccountLog_Model.Status = "Error";
                                    bankDetailsAccountLog_Model.Message = "Bank Details SubSidiary Acc Summary L3.Insert Failed";
                                    bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                    bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                    var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                    mapbankAccLog_Fail_L2.CREATED_BY = UserId;
                                    mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                    var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);
                                    Console.WriteLine("Failed to insert record.");
                                }
                            }
                            //Console.WriteLine($"Subsidiary: {item.Subsidiary}, Account Number: {item.custrecord_htl_bank_account_number}, Description: {item.description}, Account Balance: {item.account_bal}");
                            //Console.WriteLine("Record inserted successfully.");

                            Console.WriteLine("Bank Acc Summary Log inserted successfully.");
                            Console.WriteLine($"Subsidiary: {bank_Acc_Subsidiarydetails.subsidiary}, account_bal: {bank_Acc_Subsidiarydetails.account_bal},closing_balance_as_per_bank_statement: {bank_Acc_Subsidiarydetails.closing_balance_as_per_bank_statement}, current_account_balance_as_per_bank_book: {bank_Acc_Subsidiarydetails.current_account_balance_as_per_bank_book}");
                            Console.WriteLine("Record inserted successfully.");
                        }
                        else
                        {
                            bank_Acc_Subsidiary_Log_Model.Status = "Error";
                            bank_Acc_Subsidiary_Log_Model.Message = " Bank Account Subsidiary Record inserted successfully.";
                            bank_Acc_Subsidiary_Log_Model.ActionName = "UpdateBank_Acc_Summ_Async_L1";
                            bank_Acc_Subsidiary_Log_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                            var mapbankAccLogs = await _refreshBAD.MapBank_Acc_Log_NS_Model(bank_Acc_Subsidiary_Log_Model);
                            mapbankAccLogs.CREATED_BY_Name = Name;
                            mapbankAccLogs.CREATED_BY = UserId;
                            var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLogs, userid);
                        }

                    }
                    else
                    {
                        bank_Acc_Subsidiary_Log_Model.Status = "Error";
                        bank_Acc_Subsidiary_Log_Model.Message = " Bank Account Subsidiary Record inserted successfully.";
                        bank_Acc_Subsidiary_Log_Model.ActionName = "UpdateBank_Acc_Summ_Async_L1";
                        bank_Acc_Subsidiary_Log_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                        var mapbankAccLogs = await _refreshBAD.MapBank_Acc_Log_NS_Model(bank_Acc_Subsidiary_Log_Model);
                        mapbankAccLogs.CREATED_BY_Name = Name;
                        mapbankAccLogs.CREATED_BY = UserId;
                        var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLogs, userid);
                    }

                    // The Level2 Method call Here 

                    string storeprocedure = "[dbo].[sp_GetWhatsAppTemplate_Details]";  // Example stored procedure name to get the String Query but Currently not used.
                    Console.WriteLine("L2 Method And Logic Execution Started");
                    var response = _refreshBAD.TriggerBankSummary_BY_NetSuiteQlAsync_L2();
                    Bank_Acc_Summ_NS = (List<Bank_Acc_Summ_NS>)response.Result.Data;
                    Bank_Acc_Summ_NS = Bank_Acc_Summ_NS.Where(x => x.SubId == request.SubId).ToList();
                    //Console.WriteLine(strQuery.);

                    string templateName = string.Empty;
                    foreach (var item in Bank_Acc_Summ_NS)
                    {
                        item.CreatedBy = Convert.ToDecimal(UserId);
                        item.CreatedByName = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                        Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                        Console.WriteLine("Add Bank Acc SummaryMethod Execution Started");
                        var resultresponse = await _refreshBAD.Update_Bank_Acc_Summ_Async_L2(item, userid ,Name);
                        string resultstatus = resultresponse.Message.Contains("Update Success") ? "Success" : "Failure";
                        if (resultstatus.Contains("Failure"))
                        {
                            resultstatus = resultresponse.Message.Contains("Insert Success") ? "Success" : "Failure";
                        }
                         
                        var bankAccountLog_Model = await _refreshBAD.MapBank_Acc_Summ_NS_ToLogModel(item);
                        if (resultstatus.Contains("Success"))
                        {
                            try
                            {
                                Console.WriteLine("Add Bank Acc Summary Method End");
                                Console.WriteLine("L3 Method And Logic Execution Started");
                                bankAccountLog_Model.Status = "Success";
                                bankAccountLog_Model.Message = "Record inserted successfully.";
                                bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";

                                //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                                var mapbankAccLogs = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                mapbankAccLogs.CREATED_BY = UserId;
                                mapbankAccLogs.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLogs, userid);
                                var LoginStatusL2 = bankdetailsLogs.Contains("Success") ? "Success" : "Failure";
                                if (LoginStatusL2.Contains("Success"))
                                {
                                    Console.WriteLine("Bank Acc Summary Log inserted successfully.");
                                    var subsidiaryresponse = await _refreshBAD.TriggerBankDetail_By_SusidiaryidAsync_L3(item.SubId);
                                    BankDetails_BySubsidiary_NS = subsidiaryresponse.Data as List<BankDetails_By_Subsidiary>;
                                    foreach (var subitem in BankDetails_BySubsidiary_NS)
                                    {
                                        subitem.Created_By = Convert.ToDecimal(UserId);
                                        subitem.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                        subitem.Mkey = resultresponse.Mkey;
                                        subitem.SubId = item.SubId;
                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                        Console.WriteLine("Add Sub SubdiaryBank Details Execution Started");
                                        //var resultSubresponses = await CommonService.BankPortalService.AddSubSidiaryBankDetailsSummary_L3(subitem);
                                        var resultSubresponses = "Failure";
                                        //string subsidiaryLog = resultSubresponses.Message.Contains("Failure") ? "Success" : "Failure";
                                        //subitem.SrNo = resultSubresponses.SrNo;
                                        var bankDetailsAccountLog_Model = await _refreshBAD.MapBank_Details_By_Subsidiary_ToLogModel(subitem);
                                        Console.WriteLine("Add Sub SubdiaryBank Details Method End");
                                        if (resultSubresponses.Contains("Success"))
                                        {
                                            try
                                            {
                                                Console.WriteLine("Add Bank Acc Summary Method End");
                                                Console.WriteLine("L3 Method And Logic Execution Started");
                                                bankDetailsAccountLog_Model.Status = "Success";
                                                bankDetailsAccountLog_Model.Message = " Bank Details Subsidiary Account Summary Record inserted successfully.";
                                                bankDetailsAccountLog_Model.ActionName = "Add Bank Details Subsidairy _Acc_Summ_Async_L3";
                                                bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                                //bankDetailsAccountLog_Model.CREATED_BY= UserId;
                                                var bankDetailsLogresponse = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                                var LogBankDetailsSubsidairyStatus_L3 = bankDetailsLogresponse.Contains("Success") ? "Success" : "Failure";
                                                if (LogBankDetailsSubsidairyStatus_L3.Contains("Success"))
                                                {
                                                    Console.WriteLine("Bank Details SubSidiary Acc Summary L3 inserted successfully.");
                                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                    Console.WriteLine("Record inserted successfully.");
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                    Console.WriteLine("Failed to insert record.");
                                                }
                                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                Console.WriteLine("Record inserted successfully.");
                                            }
                                            catch (Exception ex)
                                            {
                                                string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                                Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                                Console.WriteLine("Failed to insert record.");
                                                bankDetailsAccountLog_Model.Status = "Error";
                                                bankDetailsAccountLog_Model.Message = errorMessage;
                                                bankDetailsAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                                bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                                var ExceptiombankdetailsLog = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                                // Console.WriteLine("Exception Occurred While Mapping Bank Details SubSidiary To Log Model: " + ex.Message);
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                            Console.WriteLine("Record inserted successfully.");

                                            //var makeHistroryStatus = CommonService.BankPortalService.ProcessBankAccSubsidiarySummaryMakeHistoryAsync_L2(subitem.Created_By, 1);
                                        }
                                    }
                                    Console.WriteLine($"Subsidiary: {item.Subsidiary}, Account Number: {item.custrecord_htl_bank_account_number}, Description: {item.description}, Account Balance: {item.account_bal}");
                                    Console.WriteLine("Record inserted successfully.");
                                }
                                else
                                {
                                    var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                    bankAccountLog_Model.Status = "Error";
                                    bankAccountLog_Model.Message = "Failed to insert Bank Acc Summary Log.";
                                    bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                    bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                    var mapbankAcc_FailedLog = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                    mapbankAcc_FailedLog.CREATED_BY = UserId;
                                    mapbankAcc_FailedLog.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                    var bankdetailsFailedLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAcc_FailedLog, userid);
                                    Console.WriteLine("Failed to insert Bank Acc Summary Log.");

                                }

                            }
                            catch (Exception ex)
                            {
                                string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                Console.WriteLine("Failed to insert record.");
                                bankAccountLog_Model.Status = "Error";
                                bankAccountLog_Model.Message = errorMessage;
                                bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                                var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                var mapbankAccLog_Fail_l2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_l2, userid);
                                Console.WriteLine("Exception Occurred While Mapping Bank Acc Summary To Log Model: " + ex.Message);
                            }

                        }
                        else
                        {
                            string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                            Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                            Console.WriteLine("Failed to insert record.");
                            bankAccountLog_Model.Status = "Error";
                            bankAccountLog_Model.Message = errorMessage;
                            bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                            bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";

                            // var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                            var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                            var mapbankAccLog_Fail = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                            mapbankAccLog_Fail.CREATED_BY = UserId;
                            mapbankAccLog_Fail.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                            var bankdetailsLog_fail = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail, userid);
                            WriteErrorToTextFile(errorMessage);
                        }
                    }
                    //Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                    Console.WriteLine("All records marked with DELETE_FLAG = 'Y' have been deleted successfully.");
                    var delete_FlagStatus = _refreshBAD.ProcessBankAccSubsidiarySummaryDeleteDetailsAsync(UserId, businessGroupId);


                }
                else if(HasSubId && HasProjectId && HasAccountNumber)
                {

                    var checkstatus= await _refreshBAD.CheckSubIdExistAsync(request!.SubId);
                    if(checkstatus.Message.Contains("SubId does not exist") && checkstatus.StatusCode== 0)
                    {
                        checkresponseObject =  await Insert_BANK_Acc_Details_Level_By_Level(Name,request);
                    }
                    else
                    {
                        checkresponseObject.Status = "Success";
                    }
                    if (checkresponseObject.Status.Contains("Success"))
                    {
                        var subsidiaryresponse = await _refreshBAD.TriggerBankDetail_By_SusidiaryidAsync_L3(request!.SubId);
                        BankDetails_BySubsidiary_NS = subsidiaryresponse.Data as List<BankDetails_By_Subsidiary>;
                        request.BankAccountNumber = request.BankAccountNumber.Trim();
                        bank_Acc_details = BankDetails_BySubsidiary_NS?.FirstOrDefault(x => x.SubId == request.SubId && x.projectid == request.ProjectId && x.DisplayNameWithHierarchy == request.BankAccountNumber);
                        bank_Acc_details.Created_By = Convert.ToDecimal(userid);
                        bank_Acc_details.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                        //bank_Acc_details.Mkey = request.Mkey;
                        bank_Acc_details.SubId = request.SubId;
                        //Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {bnksubitem.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                        //Console.WriteLine("Add Sub SubdiaryBank Details Execution Started");
                        var resultSubresponses = await _refreshBAD.Update_SubSidiaryBankDetailsSummary_L3(bank_Acc_details, userid, Name);
                        string subsidiaryLog = resultSubresponses.Message.Contains("Update Success") ? "Success" : "Failure";
                        if (subsidiaryLog.Contains("Failure"))
                        {
                            subsidiaryLog = resultSubresponses.Message.Contains("Insert Success") ? "Success" : "Failure";
                        }

                        bank_Acc_details.SrNo = resultSubresponses.SrNo;
                        var bankDetailsAccountLog_Model = await _refreshBAD.MapBank_Details_By_Subsidiary_ToLogModel(bank_Acc_details);

                        Console.WriteLine("Add Sub SubdiaryBank Details Method End");
                        if (subsidiaryLog.Contains("Success"))
                        {
                            Console.WriteLine("Add Bank Acc Summary Method End");
                            Console.WriteLine("L3 Method And Logic Execution Started");
                            bankDetailsAccountLog_Model.Status = "Success";
                            bankDetailsAccountLog_Model.Message = " Bank Details Subsidiary Account Summary Record inserted successfully.";
                            bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                            bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                            try
                            {
                                //var bankDetailsLogresponse = await CommonService.BankPortalService.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model);
                                var mapbankAccLog_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                mapbankAccLog_L2.CREATED_BY = UserId;
                                mapbankAccLog_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                var LogBankDetailsSubsidairyStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_L2, userid);

                                // This Line of Code is for Testing Purpose to check the L3 Logic when L2 is Success
                                // var LogBankDetailsSubsidairyStatus_L3 = "Success";       // bankDetailsLogresponse.Contains("Success") ? "Success" : "Failure";
                                // End
                                if (LogBankDetailsSubsidairyStatus_L3.Contains("Success"))
                                {
                                    Console.WriteLine("Bank Details SubSidiary Acc Summary L3 inserted successfully.");
                                    Console.WriteLine($"Subsidiary: {bank_Acc_details.Subsidiary}, account_bal: {bank_Acc_details.Account_Bal},accounttype: {bank_Acc_details.AccountType}, custrecord_htl_bank_account_number: {bank_Acc_details.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {bank_Acc_details.DisplayNameWithHierarchy}");
                                    Console.WriteLine("Record inserted successfully.");
                                }
                                else
                                {
                                    Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                    Console.WriteLine($"Subsidiary: {bank_Acc_details.Subsidiary}, account_bal: {bank_Acc_details.Account_Bal},accounttype: {bank_Acc_details.AccountType}, custrecord_htl_bank_account_number: {bank_Acc_details.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {bank_Acc_details.DisplayNameWithHierarchy}");
                                    bankDetailsAccountLog_Model.Status = "Error";
                                    bankDetailsAccountLog_Model.Message = "Bank Details SubSidiary Acc Summary L3.Insert Failed";
                                    bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                    bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                    var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                    mapbankAccLog_Fail_L2.CREATED_BY = UserId;
                                    mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                    var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);
                                    Console.WriteLine("Failed to insert record.");
                                }
                                // Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                // Console.WriteLine("Record inserted successfully.");
                            }
                            catch (Exception ex)
                            {
                                string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {bank_Acc_details.Subsidiary}, Project: {bank_Acc_details.Project}. Error: {ex.Message}";
                                Console.WriteLine($"Subsidiary: {bank_Acc_details.Subsidiary}, Project: {bank_Acc_details.Project},closing_balance_as_per_bank_statement: {bank_Acc_details.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {bank_Acc_details.Current_Account_Balance_As_Per_Bank_Book}");
                                Console.WriteLine("Failed to insert record.");
                                bankDetailsAccountLog_Model.Status = "Error";
                                bankDetailsAccountLog_Model.Message = errorMessage;
                                bankDetailsAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(bank_Acc_Subsidiarydetails.Created_By, 1);
                                var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                mapbankAccLog_Fail_L2.CREATED_BY = UserId;
                                mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);

                                //var ExceptiombankdetailsLog = await CommonService.BankPortalService.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model);
                                // Console.WriteLine("Exception Occurred While Mapping Bank Details SubSidiary To Log Model: " + ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Subsidiary: {bank_Acc_details.Subsidiary}, account_bal: {bank_Acc_details.Account_Bal},accounttype: {bank_Acc_details.AccountType}, custrecord_htl_bank_account_number: {bank_Acc_details.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {bank_Acc_details.DisplayNameWithHierarchy}");
                            var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(request.UserId, 1);

                            Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                            Console.WriteLine($"Subsidiary: {bank_Acc_details.Subsidiary}, account_bal: {bank_Acc_details.Account_Bal},accounttype: {bank_Acc_details.AccountType}, custrecord_htl_bank_account_number: {bank_Acc_details.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {bank_Acc_details.DisplayNameWithHierarchy}");
                            bankDetailsAccountLog_Model.Status = "Error";
                            bankDetailsAccountLog_Model.Message = "Bank Details SubSidiary Acc Summary L3.Insert Failed";
                            bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                            bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                            var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                            mapbankAccLog_Fail_L2.CREATED_BY = UserId;
                            mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                            var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);
                            Console.WriteLine("Failed to insert record.");
                        }

                    }
                    else
                    {
                        responseObject.Message= "Failed to insert Bank Account Details Level By Level.";
                        responseObject.Status= "Error";
                        return Ok(responseObject);
                    }
                }
                else if (HasSubId && HasProjectId && !HasAccountNumber)
                {

                    var checkstatus = await _refreshBAD.CheckSubIdExistAsync(request!.SubId);
                    if (checkstatus.Message.Contains("SubId does not exist") && checkstatus.StatusCode == 0)
                    {
                        checkresponseObject = await Insert_BANK_Acc_Details_Level_By_Level(Name, request);
                    }
                    else
                    {
                        checkresponseObject.Status = "Success";
                    }
                    if (checkresponseObject.Status.Contains("Success"))
                    {
                        var response = _refreshBAD.TriggerBankSummary_BY_NetSuiteQlAsync_L2();
                        Bank_Acc_Summ_NS = (List<Bank_Acc_Summ_NS>)response.Result.Data;
                        Bank_Acc_Summ_NS = Bank_Acc_Summ_NS.Where(x => x.SubId == request.SubId && x.projectid == request.ProjectId).ToList();
                        //Console.WriteLine(strQuery.);

                        string templateName = string.Empty;
                        foreach (var item in Bank_Acc_Summ_NS)
                        {

                            item.CreatedBy = UserId;
                            item.CreatedByName = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                            Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                            Console.WriteLine("Add Bank Acc SummaryMethod Execution Started");
                            var resultresponse = await _refreshBAD.Update_Bank_Acc_Summ_Async_L2(item, userid, Name);
                            string resultstatus = resultresponse.Message.Contains("Update Success") ? "Success" : "Failure";
                            if (resultstatus.Contains("Failure"))
                            {
                                resultstatus = resultresponse.Message.Contains("Insert Success") ? "Success" : "Failure";
                            }

                            var bankAccountLog_Model = await _refreshBAD.MapBank_Acc_Summ_NS_ToLogModel(item);
                            if (resultstatus.Contains("Success"))
                            {
                                try
                                {
                                    Console.WriteLine("Add Bank Acc Summary Method End");
                                    Console.WriteLine("L3 Method And Logic Execution Started");
                                    bankAccountLog_Model.Status = "Success";
                                    bankAccountLog_Model.Message = "Record inserted successfully.";
                                    bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                    bankAccountLog_Model.MethodName = "GetAllRefresh_Bank_Acc_Details";

                                    //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                                    var mapbankAccLogs = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                    mapbankAccLogs.CREATED_BY = UserId;
                                    mapbankAccLogs.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                    var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLogs, userid);
                                    var LoginStatusL2 = bankdetailsLogs.Contains("Success") ? "Success" : "Failure";
                                    if (LoginStatusL2.Contains("Success"))
                                    {
                                        Console.WriteLine("Bank Acc Summary Log inserted successfully.");
                                        var subsidiaryresponse = await _refreshBAD.TriggerBankDetail_By_SusidiaryidAsync_L3(item.SubId);
                                        BankDetails_BySubsidiary_NS = subsidiaryresponse.Data as List<BankDetails_By_Subsidiary>;
                                        foreach (var subitem in BankDetails_BySubsidiary_NS)
                                        {
                                            subitem.Created_By = Convert.ToDecimal(UserId);
                                            subitem.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                            subitem.Mkey = resultresponse.Mkey;
                                            subitem.SubId = item.SubId;
                                            Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                            Console.WriteLine("Add Sub SubdiaryBank Details Execution Started");
                                            //var resultSubresponses = await CommonService.BankPortalService.AddSubSidiaryBankDetailsSummary_L3(subitem);
                                            var resultSubresponses = "Failure";
                                            //string subsidiaryLog = resultSubresponses.Message.Contains("Failure") ? "Success" : "Failure";
                                            //subitem.SrNo = resultSubresponses.SrNo;
                                            var bankDetailsAccountLog_Model = await _refreshBAD.MapBank_Details_By_Subsidiary_ToLogModel(subitem);
                                            Console.WriteLine("Add Sub SubdiaryBank Details Method End");
                                            if (resultSubresponses.Contains("Success"))
                                            {
                                                try
                                                {
                                                    Console.WriteLine("Add Bank Acc Summary Method End");
                                                    Console.WriteLine("L3 Method And Logic Execution Started");
                                                    bankDetailsAccountLog_Model.Status = "Success";
                                                    bankDetailsAccountLog_Model.Message = " Bank Details Subsidiary Account Summary Record inserted successfully.";
                                                    bankDetailsAccountLog_Model.ActionName = "Add Bank Details Subsidairy _Acc_Summ_Async_L3";
                                                    bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                                    //bankDetailsAccountLog_Model.CREATED_BY= UserId;
                                                    var bankDetailsLogresponse = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                                    var LogBankDetailsSubsidairyStatus_L3 = bankDetailsLogresponse.Contains("Success") ? "Success" : "Failure";
                                                    if (LogBankDetailsSubsidairyStatus_L3.Contains("Success"))
                                                    {
                                                        Console.WriteLine("Bank Details SubSidiary Acc Summary L3 inserted successfully.");
                                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                        Console.WriteLine("Record inserted successfully.");
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                        Console.WriteLine("Failed to insert record.");
                                                    }
                                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                    Console.WriteLine("Record inserted successfully.");
                                                }
                                                catch (Exception ex)
                                                {
                                                    string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                                    Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                                    Console.WriteLine("Failed to insert record.");
                                                    bankDetailsAccountLog_Model.Status = "Error";
                                                    bankDetailsAccountLog_Model.Message = errorMessage;
                                                    bankDetailsAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                                    bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                                    var ExceptiombankdetailsLog = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                                    // Console.WriteLine("Exception Occurred While Mapping Bank Details SubSidiary To Log Model: " + ex.Message);
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                Console.WriteLine("Record inserted successfully.");

                                                //var makeHistroryStatus = CommonService.BankPortalService.ProcessBankAccSubsidiarySummaryMakeHistoryAsync_L2(subitem.Created_By, 1);
                                            }
                                        }
                                        Console.WriteLine($"Subsidiary: {item.Subsidiary}, Account Number: {item.custrecord_htl_bank_account_number}, Description: {item.description}, Account Balance: {item.account_bal}");
                                        Console.WriteLine("Record inserted successfully.");
                                    }
                                    else
                                    {
                                        var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                        bankAccountLog_Model.Status = "Error";
                                        bankAccountLog_Model.Message = "Failed to insert Bank Acc Summary Log.";
                                        bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                        bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                        var mapbankAcc_FailedLog = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                        mapbankAcc_FailedLog.CREATED_BY = UserId;
                                        mapbankAcc_FailedLog.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                        var bankdetailsFailedLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAcc_FailedLog, userid);
                                        Console.WriteLine("Failed to insert Bank Acc Summary Log.");

                                    }

                                }
                                catch (Exception ex)
                                {
                                    string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                    Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                    Console.WriteLine("Failed to insert record.");
                                    bankAccountLog_Model.Status = "Error";
                                    bankAccountLog_Model.Message = errorMessage;
                                    bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                    bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                    //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                                    var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                    var mapbankAccLog_Fail_l2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                    var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_l2, userid);
                                    Console.WriteLine("Exception Occurred While Mapping Bank Acc Summary To Log Model: " + ex.Message);
                                }

                            }
                            else
                            {
                                string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                Console.WriteLine("Failed to insert record.");
                                bankAccountLog_Model.Status = "Error";
                                bankAccountLog_Model.Message = errorMessage;
                                bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";

                                // var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                                var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                var mapbankAccLog_Fail = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                mapbankAccLog_Fail.CREATED_BY = UserId;
                                mapbankAccLog_Fail.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                var bankdetailsLog_fail = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail, userid);
                                WriteErrorToTextFile(errorMessage);
                            }


                        }
                    }
                    else
                    {
                        responseObject.Message = "Failed to insert Bank Account Details Level By Level.";
                        responseObject.Status = "Error";
                        return Ok(responseObject);
                    }
                }
                else if(HasSubId  && HasProjectId_Zero)
                {

                    var checkstatus = await _refreshBAD.CheckSubIdExistAsync(request!.SubId);
                    if (checkstatus.Message.Contains("SubId does not exist") && checkstatus.StatusCode == 0)
                    {
                        checkresponseObject = await Insert_BANK_Acc_Details_Level_By_Level(Name, request);
                    }
                    else
                    {
                        checkresponseObject.Status = "Success";
                    }
                    if (checkresponseObject.Status.Contains("Success"))
                    {
                        var response = _refreshBAD.TriggerBankSummary_BY_NetSuiteQlAsync_L2();
                        Bank_Acc_Summ_NS = (List<Bank_Acc_Summ_NS>)response.Result.Data;
                        Bank_Acc_Summ_NS = Bank_Acc_Summ_NS.Where(x => x.SubId == request.SubId && x.projectid == request.ProjectId).ToList();
                        //Console.WriteLine(strQuery.);

                        string templateName = string.Empty;
                        foreach (var item in Bank_Acc_Summ_NS)
                        {
                            item.CreatedBy = UserId;
                            item.CreatedByName = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                            Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                            Console.WriteLine("Add Bank Acc SummaryMethod Execution Started");
                            var resultresponse = await _refreshBAD.Update_Bank_Acc_Summ_Async_L2(item, userid, Name);
                            string resultstatus = resultresponse.Message.Contains("Update Success") ? "Success" : "Failure";
                            if (resultstatus.Contains("Failure"))
                            {
                                resultstatus = resultresponse.Message.Contains("Insert Success") ? "Success" : "Failure";
                            }
                            var bankAccountLog_Model = await _refreshBAD.MapBank_Acc_Summ_NS_ToLogModel(item);
                            if (resultstatus.Contains("Success"))
                            {
                                try
                                {
                                    Console.WriteLine("Add Bank Acc Summary Method End");
                                    Console.WriteLine("L3 Method And Logic Execution Started");
                                    bankAccountLog_Model.Status = "Success";
                                    bankAccountLog_Model.Message = "Record inserted successfully.";
                                    bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                    bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";

                                    //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                                    var mapbankAccLogs = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                    mapbankAccLogs.CREATED_BY = UserId;
                                    mapbankAccLogs.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                    var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLogs, userid);
                                    var LoginStatusL2 = bankdetailsLogs.Contains("Success") ? "Success" : "Failure";
                                    if (LoginStatusL2.Contains("Success"))
                                    {
                                        Console.WriteLine("Bank Acc Summary Log inserted successfully.");
                                        var subsidiaryresponse = await _refreshBAD.TriggerBankDetail_By_SusidiaryidAsync_L3(item.SubId);
                                        BankDetails_BySubsidiary_NS = subsidiaryresponse.Data as List<BankDetails_By_Subsidiary>;
                                        foreach (var subitem in BankDetails_BySubsidiary_NS)
                                        {
                                            subitem.Created_By = UserId;
                                            subitem.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                            subitem.Mkey = resultresponse.Mkey;
                                            subitem.SubId = item.SubId;
                                            Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                            Console.WriteLine("Add Sub SubdiaryBank Details Execution Started");
                                            //var resultSubresponses = await CommonService.BankPortalService.AddSubSidiaryBankDetailsSummary_L3(subitem);
                                            var resultSubresponses = "Failure";
                                            //string subsidiaryLog = resultSubresponses.Message.Contains("Failure") ? "Success" : "Failure";
                                            //subitem.SrNo = resultSubresponses.SrNo;
                                            var bankDetailsAccountLog_Model = await _refreshBAD.MapBank_Details_By_Subsidiary_ToLogModel(subitem);
                                            Console.WriteLine("Add Sub SubdiaryBank Details Method End");
                                            if (resultSubresponses.Contains("Success"))
                                            {
                                                try
                                                {
                                                    Console.WriteLine("Add Bank Acc Summary Method End");
                                                    Console.WriteLine("L3 Method And Logic Execution Started");
                                                    bankDetailsAccountLog_Model.Status = "Success";
                                                    bankDetailsAccountLog_Model.Message = " Bank Details Subsidiary Account Summary Record inserted successfully.";
                                                    bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                                    bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                                    //bankDetailsAccountLog_Model.CREATED_BY= UserId;
                                                    var bankDetailsLogresponse = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                                    var LogBankDetailsSubsidairyStatus_L3 = bankDetailsLogresponse.Contains("Success") ? "Success" : "Failure";
                                                    if (LogBankDetailsSubsidairyStatus_L3.Contains("Success"))
                                                    {
                                                        Console.WriteLine("Bank Details SubSidiary Acc Summary L3 inserted successfully.");
                                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                        Console.WriteLine("Record inserted successfully.");
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                        Console.WriteLine("Failed to insert record.");
                                                    }
                                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                    Console.WriteLine("Record inserted successfully.");
                                                }
                                                catch (Exception ex)
                                                {
                                                    string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                                    Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                                    Console.WriteLine("Failed to insert record.");
                                                    bankDetailsAccountLog_Model.Status = "Error";
                                                    bankDetailsAccountLog_Model.Message = errorMessage;
                                                    bankDetailsAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                                    bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                                    var ExceptiombankdetailsLog = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                                    // Console.WriteLine("Exception Occurred While Mapping Bank Details SubSidiary To Log Model: " + ex.Message);
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                Console.WriteLine("Record inserted successfully.");

                                                //var makeHistroryStatus = CommonService.BankPortalService.ProcessBankAccSubsidiarySummaryMakeHistoryAsync_L2(subitem.Created_By, 1);
                                            }
                                        }
                                        Console.WriteLine($"Subsidiary: {item.Subsidiary}, Account Number: {item.custrecord_htl_bank_account_number}, Description: {item.description}, Account Balance: {item.account_bal}");
                                        Console.WriteLine("Record inserted successfully.");
                                    }
                                    else
                                    {
                                        var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                        bankAccountLog_Model.Status = "Error";
                                        bankAccountLog_Model.Message = "Failed to insert Bank Acc Summary Log.";
                                        bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                        bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                        var mapbankAcc_FailedLog = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                        mapbankAcc_FailedLog.CREATED_BY = UserId;
                                        mapbankAcc_FailedLog.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                        var bankdetailsFailedLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAcc_FailedLog, userid);
                                        Console.WriteLine("Failed to insert Bank Acc Summary Log.");

                                    }

                                }
                                catch (Exception ex)
                                {
                                    string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                    Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                    Console.WriteLine("Failed to insert record.");
                                    bankAccountLog_Model.Status = "Error";
                                    bankAccountLog_Model.Message = errorMessage;
                                    bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                    bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                    //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                                    var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                    var mapbankAccLog_Fail_l2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                    var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_l2, userid);
                                    Console.WriteLine("Exception Occurred While Mapping Bank Acc Summary To Log Model: " + ex.Message);
                                }

                            }
                            else
                            {
                                string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                Console.WriteLine("Failed to insert record.");
                                bankAccountLog_Model.Status = "Error";
                                bankAccountLog_Model.Message = errorMessage;
                                bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";

                                // var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                                var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                var mapbankAccLog_Fail = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                mapbankAccLog_Fail.CREATED_BY = UserId;
                                mapbankAccLog_Fail.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                var bankdetailsLog_fail = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail, userid);
                                WriteErrorToTextFile(errorMessage);
                            }
                        }
                    }
                    else
                    {
                        responseObject.Message = "Failed to insert Bank Account Details Level By Level.";
                        responseObject.Status = "Error";
                        return Ok(responseObject);
                    }   
                }

                responseObject.Status = "Ok";
                responseObject.Message = "Bank Account Details Refreshed Successfully for ID: " + request.BankAccountNumber;
                responseObject.Data = null;
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during Refreshing Bank Account Details by ID";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }

        public static void WriteErrorToTextFile(string errorMessage)
        {
            try
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogsTxt");

                // Create folder if not exists
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, "WhatsAppErrorLog.txt");

                // Append text with timestamp
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("==============================================");
                    writer.WriteLine($"Date : {DateTime.Now}");
                    writer.WriteLine("Error Message:");
                    writer.WriteLine(errorMessage);
                    writer.WriteLine("==============================================\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to write log file: " + ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ResponseObject> Insert_BANK_Acc_Details_Level_By_Level( string Name , RefreshBankAccDetailsByIdRequest request)
        {
            var responseObject = new ResponseObject();
            var Bank_Acc_Summ_NS = new List<Bank_Acc_Summ_NS>();
            var Bank_Acc_Summ_NS_Linq = new List<Bank_Acc_Summ_NS>();
            var bankAccSummaryResponse = new List<Bank_Acc_Subsidiary_Summ_NS>();
            var bank_Acc_Subsidiarydetails = new Bank_Acc_Subsidiary_Summ_NS();
            var bank_Acc_details = new BankDetails_By_Subsidiary();
            var bank_Acc_sum_ns = new Bank_Acc_Summ_NS();
            var BankDetails_BySubsidiary_NS = new List<BankDetails_By_Subsidiary>();
            decimal UserId = Convert.ToDecimal(request.UserId);
            string userid = Convert.ToString(request.UserId);
            try
            {
                var BankAccsubsidiaryresponse = await _refreshBAD.TriggerBank_Acc_Subsidiary_Summ_NSAsync_L1();
                bankAccSummaryResponse = BankAccsubsidiaryresponse.Data as List<Bank_Acc_Subsidiary_Summ_NS>;
                Console.WriteLine("inserted Previouse Data in History Table successfully.");
                bank_Acc_Subsidiarydetails = bankAccSummaryResponse?.FirstOrDefault(x => x.subid == request.SubId);
                var bank_Acc_Subsidiary_Log_Model = await _refreshBAD.MapBank_Acc_Subsidiary_Summ_NS_ToLogModel(bank_Acc_Subsidiarydetails);
                bank_Acc_Subsidiary_Log_Model.Status = "Success";
                bank_Acc_Subsidiary_Log_Model.Message = " Bank Account Subsidiary Record inserted successfully.";
                bank_Acc_Subsidiary_Log_Model.ActionName = "UpdateBank_Acc_Summ_Async_L1";
                bank_Acc_Subsidiary_Log_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                var mapbankAccLog = await _refreshBAD.MapBank_Acc_Log_NS_Model(bank_Acc_Subsidiary_Log_Model);
                mapbankAccLog.CREATED_BY_Name = Name;
                mapbankAccLog.CREATED_BY = UserId;
                var bankdetailsLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog, userid);


                var LoginStatus = bankdetailsLog.Contains("Success") ? "Success" : "Failure";
                if (LoginStatus.Contains("Success"))
                {


                    bank_Acc_Subsidiarydetails.Last_Updated_By_Name = Name;
                    var bank_acc_subsidiaryStatus = await _refreshBAD.UpdateBank_Acc_Subsidiary_Summ_NsSummary_L1(bank_Acc_Subsidiarydetails!, userid, Name);
                    var bnkStatus = bank_acc_subsidiaryStatus.Message.Contains("Update Success with History") ? "Success" : "Failure";
                    if (bnkStatus.Contains("Failure"))
                    {
                        bnkStatus = bank_acc_subsidiaryStatus.Message.Contains("Insert Success") ? "Success" : "Failure";
                    }

                    if (bank_Acc_Subsidiarydetails.subid > 0 && bnkStatus.Contains("Success"))
                    {
                        var subsidiaryresponse = await _refreshBAD.TriggerBankDetail_By_SusidiaryidAsync_L3(bank_Acc_Subsidiarydetails.subid);
                        BankDetails_BySubsidiary_NS = subsidiaryresponse.Data as List<BankDetails_By_Subsidiary>;
                        foreach (var subitem in BankDetails_BySubsidiary_NS)
                        {
                            subitem.Created_By = Convert.ToDecimal(UserId);
                            subitem.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                            subitem.Mkey = bank_acc_subsidiaryStatus.Mkey;
                            subitem.SubId = bank_Acc_Subsidiarydetails.subid;
                            //Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {bnksubitem.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                            //Console.WriteLine("Add Sub SubdiaryBank Details Execution Started");
                            var resultSubresponses = await _refreshBAD.Update_SubSidiaryBankDetailsSummary_L3(subitem, userid, Name);
                            string subsidiaryLog = resultSubresponses.Message.Contains("Update Success") ? "Success" : "Failure";
                            if (subsidiaryLog.Contains("Failure"))
                            {
                                subsidiaryLog = resultSubresponses.Message.Contains("Insert Success") ? "Success" : "Failure";
                            }
                            subitem.SrNo = resultSubresponses.SrNo;
                            var bankDetailsAccountLog_Model = await _refreshBAD.MapBank_Details_By_Subsidiary_ToLogModel(subitem);
                            Console.WriteLine("Add Sub SubdiaryBank Details Method End");
                            if (resultSubresponses.Message.Contains("Success"))
                            {
                                Console.WriteLine("Add Bank Acc Summary Method End");
                                Console.WriteLine("L3 Method And Logic Execution Started");
                                bankDetailsAccountLog_Model.Status = "Success";
                                bankDetailsAccountLog_Model.Message = " Bank Details Subsidiary Account Summary Record inserted successfully.";
                                bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                try
                                {
                                    //var bankDetailsLogresponse = await CommonService.BankPortalService.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model);
                                    var mapbankAccLog_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                    mapbankAccLog_L2.CREATED_BY = UserId;
                                    mapbankAccLog_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                    var LogBankDetailsSubsidairyStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_L2, userid);

                                    // This Line of Code is for Testing Purpose to check the L3 Logic when L2 is Success
                                    // var LogBankDetailsSubsidairyStatus_L3 = "Success";       // bankDetailsLogresponse.Contains("Success") ? "Success" : "Failure";
                                    // End
                                    if (LogBankDetailsSubsidairyStatus_L3.Contains("Success"))
                                    {
                                        Console.WriteLine("Bank Details SubSidiary Acc Summary L3 inserted successfully.");
                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                        Console.WriteLine("Record inserted successfully.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                        bankDetailsAccountLog_Model.Status = "Error";
                                        bankDetailsAccountLog_Model.Message = "Bank Details SubSidiary Acc Summary L3.Insert Failed";
                                        bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                        bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                        var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                        mapbankAccLog_Fail_L2.CREATED_BY = UserId;
                                        mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                        var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);
                                        Console.WriteLine("Failed to insert record.");
                                    }
                                    // Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                    // Console.WriteLine("Record inserted successfully.");
                                }
                                catch (Exception ex)
                                {
                                    string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {subitem.Subsidiary}, Project: {subitem.Project}. Error: {ex.Message}";
                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, Project: {subitem.Project},closing_balance_as_per_bank_statement: {subitem.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {subitem.Current_Account_Balance_As_Per_Bank_Book}");
                                    Console.WriteLine("Failed to insert record.");
                                    bankDetailsAccountLog_Model.Status = "Error";
                                    bankDetailsAccountLog_Model.Message = errorMessage;
                                    bankDetailsAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                    bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                    var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(bank_Acc_Subsidiarydetails.Created_By, 1);
                                    var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                    mapbankAccLog_Fail_L2.CREATED_BY = UserId;
                                    mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                    var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);

                                    //var ExceptiombankdetailsLog = await CommonService.BankPortalService.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model);
                                    // Console.WriteLine("Exception Occurred While Mapping Bank Details SubSidiary To Log Model: " + ex.Message);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(UserId, 1);

                                Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {subitem.Custrecord_Htl_Bank_Account_Number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                bankDetailsAccountLog_Model.Status = "Error";
                                bankDetailsAccountLog_Model.Message = "Bank Details SubSidiary Acc Summary L3.Insert Failed";
                                bankDetailsAccountLog_Model.ActionName = "Update Bank Details Subsidairy _Acc_Summ_Async_L3";
                                bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                var mapbankAccLog_Fail_L2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L3(bankDetailsAccountLog_Model);
                                mapbankAccLog_Fail_L2.CREATED_BY = UserId;
                                mapbankAccLog_Fail_L2.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                var LogBankDetailsSubsidairyFailedStatus_L3 = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_L2, userid);
                                Console.WriteLine("Failed to insert record.");
                            }
                        }
                        //Console.WriteLine($"Subsidiary: {item.Subsidiary}, Account Number: {item.custrecord_htl_bank_account_number}, Description: {item.description}, Account Balance: {item.account_bal}");
                        //Console.WriteLine("Record inserted successfully.");

                        Console.WriteLine("Bank Acc Summary Log inserted successfully.");
                        Console.WriteLine($"Subsidiary: {bank_Acc_Subsidiarydetails.subsidiary}, account_bal: {bank_Acc_Subsidiarydetails.account_bal},closing_balance_as_per_bank_statement: {bank_Acc_Subsidiarydetails.closing_balance_as_per_bank_statement}, current_account_balance_as_per_bank_book: {bank_Acc_Subsidiarydetails.current_account_balance_as_per_bank_book}");
                        Console.WriteLine("Record inserted successfully.");
                        responseObject.Status = "Success";
                        responseObject.Message = "Bank Account Details Refreshed Successfully for ID: " + request.BankAccountNumber;
                    }
                    else
                    {
                        bank_Acc_Subsidiary_Log_Model.Status = "Error";
                        bank_Acc_Subsidiary_Log_Model.Message = " Bank Account Subsidiary Record inserted successfully.";
                        responseObject.Message = " Bank Account Subsidiary Record inserted successfully.";
                        bank_Acc_Subsidiary_Log_Model.ActionName = "UpdateBank_Acc_Summ_Async_L1";
                        bank_Acc_Subsidiary_Log_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                        responseObject.Status = "Error";
                        var mapbankAccLogs = await _refreshBAD.MapBank_Acc_Log_NS_Model(bank_Acc_Subsidiary_Log_Model);
                        mapbankAccLogs.CREATED_BY_Name = Name;
                        mapbankAccLogs.CREATED_BY = UserId;
                        var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLogs, userid);
                    }

                }
                else
                {
                    bank_Acc_Subsidiary_Log_Model.Status = "Error";
                    bank_Acc_Subsidiary_Log_Model.Message = " Bank Account Subsidiary Record inserted successfully.";
                    bank_Acc_Subsidiary_Log_Model.ActionName = "UpdateBank_Acc_Summ_Async_L1";
                    bank_Acc_Subsidiary_Log_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                    responseObject.Status = "Error";
                    responseObject.Message = " Bank Account Subsidiary Record inserted successfully.";
                    var mapbankAccLogs = await _refreshBAD.MapBank_Acc_Log_NS_Model(bank_Acc_Subsidiary_Log_Model);
                    mapbankAccLogs.CREATED_BY_Name = Name;
                    mapbankAccLogs.CREATED_BY = UserId;
                    var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLogs, userid);
                }

                string storeprocedure = "[dbo].[sp_GetWhatsAppTemplate_Details]";  // Example stored procedure name to get the String Query but Currently not used.
                Console.WriteLine("L2 Method And Logic Execution Started");
                var response = _refreshBAD.TriggerBankSummary_BY_NetSuiteQlAsync_L2();
                Bank_Acc_Summ_NS = (List<Bank_Acc_Summ_NS>)response.Result.Data;
                Bank_Acc_Summ_NS = Bank_Acc_Summ_NS.Where(x => x.SubId == request.SubId).ToList();
                //Console.WriteLine(strQuery.);

                string templateName = string.Empty;
                foreach (var item in Bank_Acc_Summ_NS)
                {
                    item.CreatedBy = Convert.ToDecimal(UserId);
                    item.CreatedByName = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                    Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                    Console.WriteLine("Add Bank Acc SummaryMethod Execution Started");
                    var resultresponse = await _refreshBAD.Update_Bank_Acc_Summ_Async_L2(item, userid, Name);
                    string resultstatus = resultresponse.Message.Contains("Update Success") ? "Success" : "Failure";
                    if (resultstatus.Contains("Failure"))
                    {
                        resultstatus = resultresponse.Message.Contains("Insert Success") ? "Success" : "Failure";
                    }

                    var bankAccountLog_Model = await _refreshBAD.MapBank_Acc_Summ_NS_ToLogModel(item);
                    if (resultstatus.Contains("Success"))
                    {
                        try
                        {
                            Console.WriteLine("Add Bank Acc Summary Method End");
                            Console.WriteLine("L3 Method And Logic Execution Started");
                            bankAccountLog_Model.Status = "Success";
                            bankAccountLog_Model.Message = "Record inserted successfully.";
                            bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                            bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";

                            //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                            var mapbankAccLogs = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                            mapbankAccLogs.CREATED_BY = UserId;
                            mapbankAccLogs.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                            var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLogs, userid);
                            var LoginStatusL2 = bankdetailsLogs.Contains("Success") ? "Success" : "Failure";
                            if (LoginStatusL2.Contains("Success"))
                            {
                                Console.WriteLine("Bank Acc Summary Log inserted successfully.");
                                var subsidiaryresponse = await _refreshBAD.TriggerBankDetail_By_SusidiaryidAsync_L3(item.SubId);
                                BankDetails_BySubsidiary_NS = subsidiaryresponse.Data as List<BankDetails_By_Subsidiary>;
                                foreach (var subitem in BankDetails_BySubsidiary_NS)
                                {
                                    subitem.Created_By = Convert.ToDecimal(UserId);
                                    subitem.Created_By_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                    subitem.Mkey = resultresponse.Mkey;
                                    subitem.SubId = item.SubId;
                                    Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                    Console.WriteLine("Add Sub SubdiaryBank Details Execution Started");
                                    //var resultSubresponses = await CommonService.BankPortalService.AddSubSidiaryBankDetailsSummary_L3(subitem);
                                    var resultSubresponses = "Failure";
                                    //string subsidiaryLog = resultSubresponses.Message.Contains("Failure") ? "Success" : "Failure";
                                    //subitem.SrNo = resultSubresponses.SrNo;
                                    var bankDetailsAccountLog_Model = await _refreshBAD.MapBank_Details_By_Subsidiary_ToLogModel(subitem);
                                    Console.WriteLine("Add Sub SubdiaryBank Details Method End");
                                    if (resultSubresponses.Contains("Success"))
                                    {
                                        try
                                        {
                                            Console.WriteLine("Add Bank Acc Summary Method End");
                                            Console.WriteLine("L3 Method And Logic Execution Started");
                                            bankDetailsAccountLog_Model.Status = "Success";
                                            bankDetailsAccountLog_Model.Message = " Bank Details Subsidiary Account Summary Record inserted successfully.";
                                            bankDetailsAccountLog_Model.ActionName = "Add Bank Details Subsidairy _Acc_Summ_Async_L3";
                                            bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                            //bankDetailsAccountLog_Model.CREATED_BY= UserId;
                                            var bankDetailsLogresponse = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                            var LogBankDetailsSubsidairyStatus_L3 = bankDetailsLogresponse.Contains("Success") ? "Success" : "Failure";
                                            if (LogBankDetailsSubsidairyStatus_L3.Contains("Success"))
                                            {
                                                Console.WriteLine("Bank Details SubSidiary Acc Summary L3 inserted successfully.");
                                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                Console.WriteLine("Record inserted successfully.");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Bank Details SubSidiary Acc Summary L3.Insert Failed");
                                                Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                                Console.WriteLine("Failed to insert record.");
                                            }
                                            Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                            Console.WriteLine("Record inserted successfully.");
                                        }
                                        catch (Exception ex)
                                        {
                                            string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                                            Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                                            Console.WriteLine("Failed to insert record.");
                                            bankDetailsAccountLog_Model.Status = "Error";
                                            bankDetailsAccountLog_Model.Message = errorMessage;
                                            bankDetailsAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                            bankDetailsAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                            var ExceptiombankdetailsLog = await _refreshBAD.AddSubSidairy_BankDetailsSummary_LogAsync_L3(bankDetailsAccountLog_Model, userid);
                                            // Console.WriteLine("Exception Occurred While Mapping Bank Details SubSidiary To Log Model: " + ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                                        Console.WriteLine("Record inserted successfully.");

                                        //var makeHistroryStatus = CommonService.BankPortalService.ProcessBankAccSubsidiarySummaryMakeHistoryAsync_L2(subitem.Created_By, 1);
                                    }
                                }
                                Console.WriteLine($"Subsidiary: {item.Subsidiary}, Account Number: {item.custrecord_htl_bank_account_number}, Description: {item.description}, Account Balance: {item.account_bal}");
                                Console.WriteLine("Record inserted successfully.");
                            }
                            else
                            {
                                var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                                bankAccountLog_Model.Status = "Error";
                                bankAccountLog_Model.Message = "Failed to insert Bank Acc Summary Log.";
                                bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                                bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                                var mapbankAcc_FailedLog = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                                mapbankAcc_FailedLog.CREATED_BY = UserId;
                                mapbankAcc_FailedLog.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                                var bankdetailsFailedLog = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAcc_FailedLog, userid);
                                Console.WriteLine("Failed to insert Bank Acc Summary Log.");

                            }

                        }
                        catch (Exception ex)
                        {
                            string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                            Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                            Console.WriteLine("Failed to insert record.");
                            bankAccountLog_Model.Status = "Error";
                            bankAccountLog_Model.Message = errorMessage;
                            bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                            bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";
                            //var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                            var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                            var mapbankAccLog_Fail_l2 = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                            var bankdetailsLogs = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail_l2, userid);
                            Console.WriteLine("Exception Occurred While Mapping Bank Acc Summary To Log Model: " + ex.Message);
                        }

                    }
                    else
                    {
                        string errorMessage = $"Failed to insert Bank_Acc_Summ_NS record for Subsidiary: {item.Subsidiary}, Project: {item.Project}. Error: {resultresponse.Message}";
                        Console.WriteLine($"Subsidiary: {item.Subsidiary}, Project: {item.Project},closing_balance_as_per_bank_statement: {item.Closing_Balance_As_Per_Bank_Statement}, current_account_balance_as_per_bank_book: {item.Current_Account_Balance_As_Per_Bank_Book}");
                        Console.WriteLine("Failed to insert record.");
                        bankAccountLog_Model.Status = "Error";
                        bankAccountLog_Model.Message = errorMessage;
                        bankAccountLog_Model.ActionName = "UpdateBank_Acc_Summ_Async_L2";
                        bankAccountLog_Model.MethodName = "Refresh_Bank_Acc_Details_ById";

                        // var bankdetailsLog = await CommonService.BankPortalService.AddBank_Acc_summ_LogAsyncL2(bankAccountLog_Model);
                        var roleBackError = _refreshBAD.ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(item.CreatedBy, 1);
                        var mapbankAccLog_Fail = await _refreshBAD.MapBank_Acc_Log_NS_Model_L2(bankAccountLog_Model);
                        mapbankAccLog_Fail.CREATED_BY = UserId;
                        mapbankAccLog_Fail.CREATED_BY_Name = string.IsNullOrEmpty(Name) ? "Admin" : Name;
                        var bankdetailsLog_fail = await _refreshBAD.AddBank_Acc_Log_NS_Async(mapbankAccLog_Fail, userid);
                        WriteErrorToTextFile(errorMessage);
                    }
                }
                //Console.WriteLine($"Subsidiary: {subitem.Subsidiary}, account_bal: {subitem.Account_Bal},accounttype: {subitem.AccountType}, custrecord_htl_bank_account_number: {item.custrecord_htl_bank_account_number} , displaynamewithhierarchy : {subitem.DisplayNameWithHierarchy}");
                Console.WriteLine("All records marked with DELETE_FLAG = 'Y' have been deleted successfully.");
                var delete_FlagStatus = _refreshBAD.ProcessBankAccSubsidiarySummaryDeleteDetailsAsync(UserId, request.BusinessGroupId);

                // responseObject.Status = "Success";
                //responseObject.Message = "Bank Account Details Refreshed Successfully for ID: " + request.BankAccountNumber;
                return (responseObject);

            }
            catch(Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = "An error occurred during Refreshing Bank Account Details by ID";
                responseObject.Data = new { error = ex.Message };
                return (responseObject);
            }




        }

        [HttpPost("BanK_Account_Details")]
        public async Task<IActionResult> RefreshBankAccountDetailsByBank([FromBody] CommonBankAccountDetailResponse_ByAccount request)
        {
            CommonResponseObject responseObject = new CommonResponseObject();
            var bankAccDetails_log = new Bank_Acc_Response_log_Model();
            var bankAccresponse = new Bank_Acc_Response_Details();
            string strStatus = string.Empty;
            int? userId = request.UserId;
            string Name = User.Identity?.Name;
            decimal UserId = Convert.ToDecimal(request.UserId);
            string userid = string.Empty;
            int? businessGroupId = 0;

            var encrypt_BankPortal = _commonService.EncryptionObje<CommonBankAccountDetailResponse_ByAccount>(request, _encryptionKey);
            var dashboard_BnakPortal = _commonService.DecryptObject<CommonBankAccountDetailResponse_ByAccount>(encrypt_BankPortal, _encryptionKey);

            if (dashboard_BnakPortal.UserId > 0)
            {
                UserId = Convert.ToDecimal(dashboard_BnakPortal.UserId);
                userid = Convert.ToString(UserId);
                businessGroupId = dashboard_BnakPortal.BusinessGroupId;
            }
            else
            {
                UserId = await _auth.GetUserIdbyUserName(Name);
                userid = Convert.ToString(UserId);
                businessGroupId = 1;
            }
            try
            {
                string env = _evn;
                string AdminUserId = userid;
                string strFolder = @"D:\Application";
                //string strFolder = @"D:\Bank_Account";

                Console.WriteLine("Process Started");

                var bankApprovalConfig = await _refreshBAD.GetBank_ApprovalConfig(env);
                string SHARED_SYMMETRIC_KEY = "33fb9741ea6c3cf6b1e8f62e103059a13a9aaa821bbfdbf9e39190a272ebcb19";

                if (!SHARED_SYMMETRIC_KEY.Contains("NoAsymetricKeyPresentinTheAuthkeyFiles"))
                {
                    Aes aes1 = Aes.Create();
                    aes1.Key = _refreshBAD.digest(SHARED_SYMMETRIC_KEY);

                    string decryptedPublicKey = Jose.JWT.Decode(
                        bankApprovalConfig.PubKey,
                        aes1.Key,
                        JweAlgorithm.A256KW,
                        JweEncryption.A128CBC_HS256
                    );

                    string decryptedpvtkey = Jose.JWT.Decode(
                        bankApprovalConfig.PvtKey,
                        aes1.Key,
                        JweAlgorithm.A256KW,
                        JweEncryption.A128CBC_HS256
                    );
                    string accountno = string.IsNullOrEmpty(request.AccountNo) ? null : request.AccountNo;
                    var bankAccountDetailsList = await _refreshBAD.GetBankAccDetailsAsync(accountno);
                    HttpClient client = new HttpClient();

                    foreach (var bankAcc in bankAccountDetailsList)
                    {
                        bankAccDetails_log = await _refreshBAD.MapBank_Acc_Summ_NS_ToLogModel(bankAcc);
                        try
                        {
                            if (bankAcc.AcctNumber != null)
                            {
                                int mkey = await _refreshBAD.InsertAccountBalanceDetailsAsync(bankAcc.AcctNumber, bankAcc.CustomerID ,userid ,Name);
                                try
                                {

                                    //var bnkaccount= await _refreshBAD.GetBankAccDetailsAsync(bankAcc.AcctNumber);
                                    var customerRequest = await _refreshBAD.GenerateCustomerRequestJson(bankAcc);
                                    string encToken = Jose.JWT.Encode(customerRequest, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
                                    var signaturePayload = await _refreshBAD.GenerateRequestJsonSignature(bankAcc, encToken);
                                    var requestJson = await _refreshBAD.GenerateRequestJson(bankAcc, encToken);
                                    string signature = _refreshBAD.SignData(signaturePayload, decryptedpvtkey);
                                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, bankApprovalConfig.CallURL)
                                    {
                                        Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                                    };

                                    //httpRequest.Headers.Add("x-client-id", bankApprovalConfig.clientid);
                                    //httpRequest.Headers.Add("x-client-secret", bankApprovalConfig.clientsecret);
                                    //httpRequest.Headers.Add("x-signature", signature);

                                    httpRequest.Headers.Add("x-client-id", bankApprovalConfig.clientid);
                                    httpRequest.Headers.Add("x-client-secret", bankApprovalConfig.clientsecret);
                                    httpRequest.Headers.Add("x-client-certificate", bankApprovalConfig.clientcertificate);
                                    httpRequest.Headers.Add("x-api-interaction-id", bankApprovalConfig.apiinteractionid);
                                    httpRequest.Headers.Add("x-signature", signature);
                                    httpRequest.Headers.Add("x-forwarded-for", "222");
                                    httpRequest.Headers.Add("x-timestamp", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString());
                                    var response = await client.SendAsync(httpRequest);
                                    //string jsonContent = await response.Content.ReadAsStringAsync();

                                    //if (!System.IO.File.Exists(strFolder + "\\Log.txt"))
                                    //    System.IO.File.WriteAllText(strFolder + "\\Log.txt", jsonContent);
                                    //else
                                    //    System.IO.File.AppendAllText(strFolder + "\\Log.txt", jsonContent);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        string jsonContent = await response.Content.ReadAsStringAsync();
                                        Console.WriteLine("jsonContent" + jsonContent);
                                        JsonDocument doc = JsonDocument.Parse(jsonContent);
                                        RootResponse bodyObj = JsonSerializer.Deserialize<RootResponse>(jsonContent);
                                        if (bodyObj?.Response?.body?.encryptData != null)
                                        {
                                            string decryptedData = Jose.JWT.Decode(bodyObj.Response.body.encryptData, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
                                            var accountBalance = JsonSerializer.Deserialize<AccountBalanceModel>(decryptedData);

                                            var updateResponse = _refreshBAD.UpdateBankDetailsHdr(accountBalance.currentBalance, DateTime.Now, accountBalance.acctNumber, accountBalance.unclearFunds, accountBalance.netBalance, accountBalance.balAvailable, accountBalance.holdAmount, accountBalance.overdraft, accountBalance.customerName);
                                            if (updateResponse.Contains("Update successful for AccountNo:"))
                                            {
                                                Console.WriteLine("Deserilized the JsonContent Completed");
                                                Console.WriteLine("jsonContent Data pass into AccountBalanceModel");
                                                var bnkAccDetails = await _refreshBAD.GetBnakAccDetailResponse_byMkey(mkey);
                                                // bankAccResp = await GetAccountBalanceDetailsByMkeyAsync(Accountbalancemkey);
                                                bnkAccDetails.AcctNumber = accountBalance.acctNumber;
                                                bnkAccDetails.CurrentBalance = Convert.ToDecimal(accountBalance.currentBalance);
                                                bnkAccDetails.UnclearFunds = Convert.ToDecimal(accountBalance.unclearFunds);
                                                bnkAccDetails.NetBalance = Convert.ToDecimal(accountBalance.netBalance);
                                                bnkAccDetails.BalAvailable = Convert.ToDecimal(accountBalance.balAvailable);
                                                bnkAccDetails.HoldAmount = Convert.ToDecimal(accountBalance.holdAmount);
                                                bnkAccDetails.Overdraft = Convert.ToDecimal(accountBalance.overdraft);
                                                bnkAccDetails.CustomerName = accountBalance.customerName;
                                                bnkAccDetails.CustomerID = accountBalance.customerID;
                                                bnkAccDetails.Status = "Success";
                                                bnkAccDetails.ResponseData = jsonContent;
                                                bnkAccDetails.ResponseTime = DateTime.Now;
                                                bnkAccDetails.Result = bodyObj.Response.metadata.status.result;
                                                bnkAccDetails.BankStatus = bodyObj.Response.body.accountDetails.status;
                                                bnkAccDetails.Relationship = bodyObj.Response.body.accountDetails.relationship;
                                                bnkAccDetails.ChequeBookFacility = bodyObj.Response.body.accountDetails.chequeBookFacility;
                                                bnkAccDetails.MinBalance = Convert.ToDecimal(bodyObj.Response.body.accountDetails.minBalance);
                                                bnkAccDetails.DateString = bodyObj.Response.body.accountDetails.openingDate.dateString;
                                                bnkAccDetails.BranchCode = bodyObj.Response.body.customerAccountDetails.accountDetails.branchCode;
                                                bnkAccDetails.AcctTypeCode = bodyObj.Response.body.customerAccountDetails.accountDetails.acctTypeCode;
                                                bnkAccDetails.CurrencyDesc = bodyObj.Response.body.customerAccountDetails.accountDetails.currencyDesc;
                                                bnkAccDetails.CurrencyCode = bodyObj.Response.body.customerAccountDetails.accountDetails.currencyCode;
                                                bnkAccDetails.AccountType = bodyObj.Response.body.customerAccountDetails.accountDetails.accountType;

                                                bnkAccDetails.CreatedBy = Convert.ToInt32(AdminUserId);
                                                bnkAccDetails.LastUpdateDate = DateTime.Now;
                                                Console.WriteLine("UpdateAccountBalanceDetailsAsync Method Start");
                                                bnkAccDetails.CreatedByName = Name;
                                                string updateResponseFinal = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bnkAccDetails, userid);
                                                Console.WriteLine("UpdateAccountBalanceDetailsAsync Method Closed");
                                                if (updateResponse.Contains("Success"))
                                                {
                                                    Console.WriteLine($"{bnkAccDetails.CustomerID}  Data Update Successfully");
                                                }
                                                bankAccDetails_log = await _refreshBAD.MapBank_Acc_Response_Details_ToLogModel(bnkAccDetails);
                                                Console.WriteLine("Bank Details Updated Successfully");
                                                bankAccDetails_log.Status = "Success";
                                                bankAccDetails_log.Message = " Canara Bank Get Bank Details Api Success.";
                                                bankAccDetails_log.ActionName = "CanaraBankApiSuccess";
                                                bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                                var SuccessfullybankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                                if (SuccessfullybankdetailsLog.Contains("Success"))
                                                {
                                                    Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                                    //  strStatus= "Success";
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Bank Account Details Log Insertion Failed");
                                                    // strStatus= "Failure";
                                                }

                                            }
                                            else
                                            {
                                                Console.WriteLine("Canara Bank Get Bank Details Api Failed to Update Bank Details.");
                                                bankAccDetails_log.Status = "Error";
                                                bankAccDetails_log.Message = " Canara Bank Get Bank Details Api Failed to Update Bank Details.";
                                                bankAccDetails_log.ActionName = "UpdateBankDetailsHdr";
                                                bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                                var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                                if (ExceptiombankdetailsLog.Contains("Success"))
                                                {
                                                    Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Bank Account Details Log Insertion Failed");

                                                }
                                                // strStatus = "Failure";
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("==========================================================================================");
                                            Console.WriteLine("Api Response Failed");
                                            string encryptData = string.Empty;
                                            //var doc = JsonDocument.Parse(jsonContent);
                                            using (doc = JsonDocument.Parse(jsonContent))
                                            {
                                                encryptData = doc.RootElement
                                                                       .GetProperty("Response")
                                                                       .GetProperty("body")
                                                                       .GetProperty("encryptData")
                                                                       .GetString();

                                                Console.WriteLine("Encrypted Data: " + encryptData);

                                            }
                                            Console.WriteLine("jsonContent Decrypt");
                                            Console.WriteLine("encryptData for Find the error Cause Start");
                                            var decryptedFailedApiresponse = Jose.JWT.Decode(encryptData, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
                                            Console.WriteLine("encryptData for Find the error Cause Completed");
                                            bankAccresponse = await _refreshBAD.GetBnakAccDetailResponse_byMkey(mkey);
                                            //bankAccResp = await GetAccountBalanceDetailsByMkeyAsync(Acmkecountbalancemkey);

                                            bankAccresponse.ResponseData = decryptedFailedApiresponse;
                                            //bankAccResp.acctNumber = null;
                                            bankAccresponse.CurrentBalance = null;
                                            bankAccresponse.UnclearFunds = null;
                                            bankAccresponse.NetBalance = null;
                                            bankAccresponse.BalAvailable = null;
                                            bankAccresponse.HoldAmount = null;
                                            bankAccresponse.Overdraft = null;
                                            bankAccresponse.CustomerName = null;
                                            //bankAccResp.customerID = AccountBalance.customerID;
                                            bankAccresponse.ResponseTime = DateTime.UtcNow;
                                            bankAccresponse.Status = "Error";
                                            bankAccresponse.LastUpdateDate = DateTime.UtcNow;
                                            bankAccresponse.CreatedBy = Convert.ToInt32(AdminUserId);
                                            bankAccresponse.CreatedByName = Name;
                                            var updateresponses = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccresponse, AdminUserId);
                                            if (updateresponses.Contains("Success"))
                                            {
                                                Console.WriteLine($"{bankAccresponse.CustomerID}  Data Update Successfully");
                                            }


                                            bankAccDetails_log = await _refreshBAD.MapBank_Acc_Response_Details_ToLogModel(bankAccresponse);
                                            Console.WriteLine("Canara Bank Get Bank Details Api Failed to Decrypt Data.");
                                            bankAccDetails_log.Status = "Error";
                                            bankAccDetails_log.Message = " Canara Bank Get Bank Details Api Failed to Decrypt Data.";
                                            bankAccDetails_log.ActionName = "DecryptDataFailed";
                                            bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                            var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                            if (ExceptiombankdetailsLog.Contains("Success"))
                                            {
                                                Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Bank Account Details Log Insertion Failed");
                                            }
                                            // strStatus = "Failure";
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("==========================================================================================");
                                        Console.WriteLine("Api Response Failed Then Start Decrypt Error Response");
                                        string jsonContent = await response.Content.ReadAsStringAsync();
                                        Console.WriteLine("jsonContent Start To Decrypt");

                                        string encryptData = string.Empty;
                                        var doc = JsonDocument.Parse(jsonContent);
                                        using (doc = JsonDocument.Parse(jsonContent))
                                        {
                                            encryptData = doc.RootElement
                                                                   .GetProperty("Response")
                                                                   .GetProperty("body")
                                                                   .GetProperty("encryptData")
                                                                   .GetString();

                                            Console.WriteLine("Encrypted Data: " + encryptData);
                                        }
                                        Console.WriteLine("jsonContent Decrypt");
                                        Console.WriteLine("encryptData for Find the error Cause Start");

                                        var decryptedFailedApiresponse = Jose.JWT.Decode(encryptData, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
                                        Console.WriteLine("jsonContent Decrypted");
                                        bankAccresponse = await _refreshBAD.GetBnakAccDetailResponse_byMkey(mkey);
                                        bankAccresponse.ResponseData = decryptedFailedApiresponse;
                                        //bankAccResp.acctNumber = null;
                                        bankAccresponse.CurrentBalance = null;
                                        bankAccresponse.UnclearFunds = null;
                                        bankAccresponse.NetBalance = null;
                                        bankAccresponse.BalAvailable = null;
                                        bankAccresponse.HoldAmount = null;
                                        bankAccresponse.Overdraft = null;
                                        bankAccresponse.CustomerName = null;
                                        //bankAccResp.customerID = AccountBalance.customerID;
                                        bankAccresponse.ResponseTime = DateTime.UtcNow;
                                        bankAccresponse.Status = "Error";
                                        bankAccresponse.LastUpdateDate = DateTime.UtcNow;
                                        bankAccresponse.CreatedBy = Convert.ToInt32(AdminUserId);
                                        Console.WriteLine("UpdateAccountBalanceDetailsAsync Method Start");
                                        bankAccresponse.CreatedByName = Name;
                                        var updateresponse = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccresponse, AdminUserId);
                                        if (updateresponse.Contains("Success"))
                                        {
                                            Console.WriteLine($"{bankAccresponse.CustomerID}  Data Update Successfully");
                                        }

                                        bankAccDetails_log = await _refreshBAD.MapBank_Acc_Response_Details_ToLogModel(bankAccresponse);
                                        Console.WriteLine("Canara Bank Get Bank Details Api Failed.");
                                        bankAccDetails_log.Status = "Error";
                                        bankAccDetails_log.Message = " Canara Bank Get Bank Details Api Failed.";
                                        bankAccDetails_log.ActionName = "CanaraBankApiFailed";
                                        bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                        var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                        if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                                        {
                                            Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Bank Account Details Log Insertion Failed");

                                        }
                                        // strStatus = "Failure";
                                    }


                                }
                                catch (Exception ex)
                                {
                                    bankAccresponse = await _refreshBAD.GetBnakAccDetailResponse_byMkey(mkey);
                                    if (ex.InnerException != null)
                                    {
                                        bankAccresponse.ResponseData = ex.InnerException.InnerException.Message + " " + ex.Message;
                                    }

                                    //bankAccResp.acctNumber = null;
                                    bankAccresponse.CurrentBalance = null;
                                    bankAccresponse.UnclearFunds = null;
                                    bankAccresponse.NetBalance = null;
                                    bankAccresponse.BalAvailable = null;
                                    bankAccresponse.HoldAmount = null;
                                    bankAccresponse.Overdraft = null;
                                    bankAccresponse.CustomerName = null;
                                    //bankAccResp.customerID = AccountBalance.customerID;
                                    bankAccresponse.ResponseTime = DateTime.UtcNow;
                                    bankAccresponse.Status = "Error";
                                    bankAccresponse.LastUpdateDate = DateTime.UtcNow;
                                    bankAccresponse.CreatedBy = Convert.ToInt32(AdminUserId);
                                    bankAccresponse.CreatedByName = Name;
                                    var updateresponse = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccresponse, AdminUserId);


                                    if (updateresponse != null)
                                    {
                                        Console.WriteLine($" Bank Account Response Update At Mkey :{mkey} Successfully ");
                                    }
                                    else
                                    {
                                        Console.WriteLine($" Bank Account Response Failed ANd Mkey :{mkey}");
                                    }
                                    bankAccDetails_log = await _refreshBAD.MapBank_Acc_Response_Details_ToLogModel(bankAccresponse);
                                    Console.WriteLine("Error Exception Response Closed");
                                    bankAccDetails_log.Status = "Error";
                                    bankAccDetails_log.Message = " Exception Occurred While Processing Bank Account Number: " + bankAcc.AcctNumber + " Error: " + ex.Message;
                                    bankAccDetails_log.ActionName = "ExceptionInBankAccountProcessing";
                                    bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                    var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                    if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                                    {
                                        Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Bank Account Details Log Insertion Failed");
                                    }
                                    Console.WriteLine("Exception Occurred While Mapping Bank Acc Summary To Log Model: " + ex.Message);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Account Number is Null or Empty.");
                                bankAccDetails_log.Status = "Error";
                                bankAccDetails_log.Message = " Account Number is Null or Empty.";
                                bankAccDetails_log.ActionName = "AccountNumberNullOrEmpty";
                                bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                                {
                                    Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                }
                                else
                                {
                                    Console.WriteLine("Bank Account Details Log Insertion Failed");
                                }
                                // strStatus = "Failure";
                            }


                        }
                        catch (Exception ex)
                        {
                            System.IO.File.AppendAllText(
                                strFolder + "\\Log.txt",
                                $"{DateTime.Now} ERROR: {ex.Message}\n"
                            );
                            Console.WriteLine("Exception Occurred While Processing Bank Account Number: " + bankAcc.AcctNumber + " Error: " + ex.Message);
                            bankAccDetails_log.Status = "Error";
                            bankAccDetails_log.Message = " Exception Occurred While Processing Bank Account Number: " + bankAcc.AcctNumber + " Error: " + ex.Message;
                            bankAccDetails_log.ActionName = "ExceptionInBankAccountProcessing";
                            bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                            var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                            if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                            {
                                Console.WriteLine("Bank Account Details Log Inserted Successfully");
                            }
                            else
                            {
                                Console.WriteLine("Bank Account Details Log Insertion Failed");
                            }
                            strStatus = "Failure";
                        }
                    }
                }

                // ✅ SUCCESS RESPONSE
                //responseObject.Status = "Success";
                //responseObject.Message = "Bank account refresh completed successfully";
                //responseObject.Data = null;

                //return Ok(responseObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred During Refreshing Bank Account Details by Account Number: " + ex.Message);
                bankAccDetails_log.Status = "Error";
                bankAccDetails_log.Message = " Exception Occurred During Refreshing Bank Account Details by Account Number: " + ex.Message;
                bankAccDetails_log.ActionName = "ExceptionInRefreshBankAccountDetailsByAccountNo";
                bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                {
                    Console.WriteLine("Bank Account Details Log Inserted Successfully");
                }
                else
                {
                    Console.WriteLine("Bank Account Details Log Insertion Failed");
                }
                //responseObject.Status = "Error";
                //responseObject.Message = "An error occurred during Refreshing Bank Account Details by Account Number";
                //responseObject.Data = new { error = ex.Message };
                return Ok(bankAccDetails_log);

                //return Ok(responseObject);
            }
            return Ok(bankAccDetails_log);
        }

        [HttpPost("Bank_Account_Details_ByAccountNo")]

        public async Task<IActionResult> RefreshBankAccountDetailsByAccountNo([FromBody] CommonBankAccountDetailResponse_ByAccount request)
        {
            CommonResponseObject responseObject = new CommonResponseObject();
            var bankAccDetails_log = new Bank_Acc_Response_log_Model();
            var bankAccresponse = new Bank_Acc_Response_Details();
            string strStatus = string.Empty;
            int? userId = request.UserId;
            string Name = User.Identity?.Name;
            decimal UserId = Convert.ToDecimal(request.UserId);
            string userid = string.Empty;
            int? businessGroupId = 0;

            var encrypt_BankPortal = _commonService.EncryptionObje<CommonBankAccountDetailResponse_ByAccount>(request, _encryptionKey);
            var dashboard_BnakPortal = _commonService.DecryptObject<CommonBankAccountDetailResponse_ByAccount>(encrypt_BankPortal, _encryptionKey);
            if (dashboard_BnakPortal.UserId > 0)
            {
                UserId = Convert.ToDecimal(dashboard_BnakPortal.UserId);
                userid = Convert.ToString(UserId);
                businessGroupId = dashboard_BnakPortal.BusinessGroupId;
            }
            else
            {
                UserId = await _auth.GetUserIdbyUserName(Name);
                userid = Convert.ToString(UserId);
                businessGroupId = 1;
            }
            try
            {
                string env = _evn;
                string AdminUserId = userid;
                string strFolder = @"D:\Application";
                //string strFolder = @"D:\Bank_Account";
                Console.WriteLine("Process Started");
                var bankApprovalConfig = await _refreshBAD.GetBank_ApprovalConfig(env);
                string SHARED_SYMMETRIC_KEY = "33fb9741ea6c3cf6b1e8f62e103059a13a9aaa821bbfdbf9e39190a272ebcb19";
                if (!SHARED_SYMMETRIC_KEY.Contains("NoAsymetricKeyPresentinTheAuthkeyFiles"))
                {
                    Aes aes1 = Aes.Create();
                    aes1.Key = _refreshBAD.digest(SHARED_SYMMETRIC_KEY);

                    string decryptedPublicKey = Jose.JWT.Decode(
                        bankApprovalConfig.PubKey,
                        aes1.Key,
                        JweAlgorithm.A256KW,
                        JweEncryption.A128CBC_HS256
                    );

                    string decryptedpvtkey = Jose.JWT.Decode(
                        bankApprovalConfig.PvtKey,
                        aes1.Key,
                        JweAlgorithm.A256KW,
                        JweEncryption.A128CBC_HS256
                    );
                    string accountno = string.IsNullOrEmpty(request.AccountNo) ? null : request.AccountNo;
                    var bankAccountDetailsList = await _refreshBAD.GetBankAccDetailsAsync(accountno);
                    HttpClient client = new HttpClient();

                    foreach (var bankAcc in bankAccountDetailsList)
                    {
                        bankAccDetails_log = await _refreshBAD.MapBank_Acc_Summ_NS_ToLogModel(bankAcc);
                        try
                        {
                            if (bankAcc.AcctNumber != null)
                            {
                                int mkey = await _refreshBAD.InsertAccountBalanceDetailsAsync(bankAcc.AcctNumber, bankAcc.CustomerID ,userid ,Name);
                                try
                                {
                                   
                                    //var bnkaccount= await _refreshBAD.GetBankAccDetailsAsync(bankAcc.AcctNumber);
                                    var customerRequest = await _refreshBAD.GenerateCustomerRequestJson(bankAcc);
                                    string encToken = Jose.JWT.Encode(customerRequest, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
                                    var signaturePayload = await _refreshBAD.GenerateRequestJsonSignature(bankAcc, encToken);
                                    var requestJson = await _refreshBAD.GenerateRequestJson(bankAcc, encToken);
                                    string signature = _refreshBAD.SignData(signaturePayload, decryptedpvtkey);
                                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, bankApprovalConfig.CallURL)
                                    {
                                        Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                                    };

                                    //httpRequest.Headers.Add("x-client-id", bankApprovalConfig.clientid);
                                    //httpRequest.Headers.Add("x-client-secret", bankApprovalConfig.clientsecret);
                                    //httpRequest.Headers.Add("x-signature", signature);

                                    httpRequest.Headers.Add("x-client-id", bankApprovalConfig.clientid);
                                    httpRequest.Headers.Add("x-client-secret", bankApprovalConfig.clientsecret);
                                    httpRequest.Headers.Add("x-client-certificate", bankApprovalConfig.clientcertificate);
                                    httpRequest.Headers.Add("x-api-interaction-id", bankApprovalConfig.apiinteractionid);
                                    httpRequest.Headers.Add("x-signature", signature);
                                    httpRequest.Headers.Add("x-forwarded-for", "222");
                                    httpRequest.Headers.Add("x-timestamp", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString());
                                    var response = await client.SendAsync(httpRequest);
                                    //string jsonContent = await response.Content.ReadAsStringAsync();

                                    //if (!System.IO.File.Exists(strFolder + "\\Log.txt"))
                                    //    System.IO.File.WriteAllText(strFolder + "\\Log.txt", jsonContent);
                                    //else
                                    //    System.IO.File.AppendAllText(strFolder + "\\Log.txt", jsonContent);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        string jsonContent = await response.Content.ReadAsStringAsync();
                                        Console.WriteLine("jsonContent" + jsonContent);
                                        JsonDocument doc = JsonDocument.Parse(jsonContent);
                                        RootResponse bodyObj = JsonSerializer.Deserialize<RootResponse>(jsonContent);
                                        if (bodyObj?.Response?.body?.encryptData != null)
                                        {
                                            string decryptedData = Jose.JWT.Decode(bodyObj.Response.body.encryptData, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
                                            var accountBalance = JsonSerializer.Deserialize<AccountBalanceModel>(decryptedData);

                                            var updateResponse = _refreshBAD.UpdateBankDetailsHdr(accountBalance.currentBalance, DateTime.Now, accountBalance.acctNumber, accountBalance.unclearFunds, accountBalance.netBalance, accountBalance.balAvailable, accountBalance.holdAmount, accountBalance.overdraft, accountBalance.customerName);
                                            if (updateResponse.Contains("Update successful for AccountNo:"))
                                            {
                                                Console.WriteLine("Deserilized the JsonContent Completed");
                                                Console.WriteLine("jsonContent Data pass into AccountBalanceModel");
                                                var bnkAccDetails = await _refreshBAD.GetBnakAccDetailResponse_byMkey(mkey);
                                                // bankAccResp = await GetAccountBalanceDetailsByMkeyAsync(Accountbalancemkey);
                                                bnkAccDetails.AcctNumber = accountBalance.acctNumber;
                                                bnkAccDetails.CurrentBalance = Convert.ToDecimal(accountBalance.currentBalance);
                                                bnkAccDetails.UnclearFunds = Convert.ToDecimal(accountBalance.unclearFunds);
                                                bnkAccDetails.NetBalance = Convert.ToDecimal(accountBalance.netBalance);
                                                bnkAccDetails.BalAvailable = Convert.ToDecimal(accountBalance.balAvailable);
                                                bnkAccDetails.HoldAmount = Convert.ToDecimal(accountBalance.holdAmount);
                                                bnkAccDetails.Overdraft = Convert.ToDecimal(accountBalance.overdraft);
                                                bnkAccDetails.CustomerName = accountBalance.customerName;
                                                bnkAccDetails.CustomerID = accountBalance.customerID;
                                                bnkAccDetails.Status = "Success";
                                                bnkAccDetails.ResponseData = jsonContent;
                                                bnkAccDetails.ResponseTime = DateTime.Now;
                                                bnkAccDetails.Result = bodyObj.Response.metadata.status.result;
                                                bnkAccDetails.BankStatus = bodyObj.Response.body.accountDetails.status;
                                                bnkAccDetails.Relationship = bodyObj.Response.body.accountDetails.relationship;
                                                bnkAccDetails.ChequeBookFacility = bodyObj.Response.body.accountDetails.chequeBookFacility;
                                                bnkAccDetails.MinBalance = Convert.ToDecimal(bodyObj.Response.body.accountDetails.minBalance);
                                                bnkAccDetails.DateString = bodyObj.Response.body.accountDetails.openingDate.dateString;
                                                bnkAccDetails.BranchCode = bodyObj.Response.body.customerAccountDetails.accountDetails.branchCode;
                                                bnkAccDetails.AcctTypeCode = bodyObj.Response.body.customerAccountDetails.accountDetails.acctTypeCode;
                                                bnkAccDetails.CurrencyDesc = bodyObj.Response.body.customerAccountDetails.accountDetails.currencyDesc;
                                                bnkAccDetails.CurrencyCode = bodyObj.Response.body.customerAccountDetails.accountDetails.currencyCode;
                                                bnkAccDetails.AccountType = bodyObj.Response.body.customerAccountDetails.accountDetails.accountType;

                                                bnkAccDetails.CreatedBy = Convert.ToInt32(AdminUserId);
                                                bnkAccDetails.LastUpdateDate = DateTime.Now;
                                                bnkAccDetails.LastUpdatedBy = Convert.ToInt32(AdminUserId);
                                                Console.WriteLine("UpdateAccountBalanceDetailsAsync Method Start");
                                                bnkAccDetails.CreatedByName= Name;
                                                string updateResponseFinal = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bnkAccDetails, userid);
                                                Console.WriteLine("UpdateAccountBalanceDetailsAsync Method Closed");
                                                if (updateResponse.Contains("Success"))
                                                {
                                                    Console.WriteLine($"{bnkAccDetails.CustomerID}  Data Update Successfully");
                                                }
                                                bankAccDetails_log = await _refreshBAD.MapBank_Acc_Response_Details_ToLogModel(bnkAccDetails);
                                                Console.WriteLine("Bank Details Updated Successfully");
                                                bankAccDetails_log.Status = "Success";
                                                bankAccDetails_log.Message = " Canara Bank Get Bank Details Api Success.";
                                                bankAccDetails_log.ActionName = "CanaraBankApiSuccess";
                                                bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                                var SuccessfullybankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                                if (SuccessfullybankdetailsLog.Contains("Record inserted successfully"))
                                                {
                                                    Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                                    //  strStatus= "Success";
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Bank Account Details Log Insertion Failed");
                                                    // strStatus= "Failure";
                                                }

                                            }
                                            else
                                            {
                                                Console.WriteLine("Canara Bank Get Bank Details Api Failed to Update Bank Details.");
                                                bankAccDetails_log.Status = "Error";
                                                bankAccDetails_log.Message = " Canara Bank Get Bank Details Api Failed to Update Bank Details.";
                                                bankAccDetails_log.ActionName = "UpdateBankDetailsHdr";
                                                bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                                var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                                if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                                                {
                                                    Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Bank Account Details Log Insertion Failed");

                                                }
                                                // strStatus = "Failure";
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("==========================================================================================");
                                            Console.WriteLine("Api Response Failed");
                                            string encryptData = string.Empty;
                                            //var doc = JsonDocument.Parse(jsonContent);
                                            using (doc = JsonDocument.Parse(jsonContent))
                                            {
                                                encryptData = doc.RootElement
                                                                       .GetProperty("Response")
                                                                       .GetProperty("body")
                                                                       .GetProperty("encryptData")
                                                                       .GetString();

                                                Console.WriteLine("Encrypted Data: " + encryptData);

                                            }
                                            Console.WriteLine("jsonContent Decrypt");
                                            Console.WriteLine("encryptData for Find the error Cause Start");
                                            var decryptedFailedApiresponse = Jose.JWT.Decode(encryptData, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
                                            Console.WriteLine("encryptData for Find the error Cause Completed");
                                            bankAccresponse = await _refreshBAD.GetBnakAccDetailResponse_byMkey(mkey);
                                            //bankAccResp = await GetAccountBalanceDetailsByMkeyAsync(Acmkecountbalancemkey);

                                            bankAccresponse.ResponseData = decryptedFailedApiresponse;
                                            //bankAccResp.acctNumber = null;
                                            bankAccresponse.CurrentBalance = null;
                                            bankAccresponse.UnclearFunds = null;
                                            bankAccresponse.NetBalance = null;
                                            bankAccresponse.BalAvailable = null;
                                            bankAccresponse.HoldAmount = null;
                                            bankAccresponse.Overdraft = null;
                                            bankAccresponse.CustomerName = null;
                                            //bankAccResp.customerID = AccountBalance.customerID;
                                            bankAccresponse.ResponseTime = DateTime.UtcNow;
                                            bankAccresponse.Status = "Error";
                                            bankAccresponse.LastUpdateDate = DateTime.UtcNow;
                                            bankAccresponse.CreatedBy = Convert.ToInt32(AdminUserId);
                                            bankAccresponse.CreatedByName = Name;
                                            bankAccresponse.LastUpdatedBy = Convert.ToInt32(AdminUserId);
                                            var updateresponses = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccresponse, AdminUserId);
                                            if (updateresponses.Contains("Success"))
                                            {
                                                Console.WriteLine($"{bankAccresponse.CustomerID}  Data Update Successfully");
                                            }


                                            bankAccDetails_log = await _refreshBAD.MapBank_Acc_Response_Details_ToLogModel(bankAccresponse);
                                            Console.WriteLine("Canara Bank Get Bank Details Api Failed to Decrypt Data.");
                                            bankAccDetails_log.Status = "Error";
                                            bankAccDetails_log.Message = " Canara Bank Get Bank Details Api Failed to Decrypt Data.";
                                            bankAccDetails_log.ActionName = "DecryptDataFailed";
                                            bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                            var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                            if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                                            {
                                                Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Bank Account Details Log Insertion Failed");
                                            }
                                            // strStatus = "Failure";
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("==========================================================================================");
                                        Console.WriteLine("Api Response Failed Then Start Decrypt Error Response");
                                        string jsonContent = await response.Content.ReadAsStringAsync();
                                        Console.WriteLine("jsonContent Start To Decrypt");

                                        string encryptData = string.Empty;
                                        var doc = JsonDocument.Parse(jsonContent);
                                        using (doc = JsonDocument.Parse(jsonContent))
                                        {
                                            encryptData = doc.RootElement
                                                                   .GetProperty("Response")
                                                                   .GetProperty("body")
                                                                   .GetProperty("encryptData")
                                                                   .GetString();

                                            Console.WriteLine("Encrypted Data: " + encryptData);
                                        }
                                        Console.WriteLine("jsonContent Decrypt");
                                        Console.WriteLine("encryptData for Find the error Cause Start");

                                        var decryptedFailedApiresponse = Jose.JWT.Decode(encryptData, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
                                        Console.WriteLine("jsonContent Decrypted");
                                        bankAccresponse = await _refreshBAD.GetBnakAccDetailResponse_byMkey(mkey);
                                        bankAccresponse.ResponseData = decryptedFailedApiresponse;
                                        //bankAccResp.acctNumber = null;
                                        bankAccresponse.CurrentBalance = null;
                                        bankAccresponse.UnclearFunds = null;
                                        bankAccresponse.NetBalance = null;
                                        bankAccresponse.BalAvailable = null;
                                        bankAccresponse.HoldAmount = null;
                                        bankAccresponse.Overdraft = null;
                                        bankAccresponse.CustomerName = null;
                                        //bankAccResp.customerID = AccountBalance.customerID;
                                        bankAccresponse.ResponseTime = DateTime.UtcNow;
                                        bankAccresponse.Status = "Error";
                                        bankAccresponse.LastUpdateDate = DateTime.UtcNow;
                                        bankAccresponse.CreatedBy = Convert.ToInt32(AdminUserId);
                                        bankAccresponse.LastUpdatedBy = Convert.ToInt32(AdminUserId);
                                        Console.WriteLine("UpdateAccountBalanceDetailsAsync Method Start");
                                        bankAccresponse.CreatedByName = Name;
                                        var updateresponse = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccresponse, AdminUserId);
                                        if (updateresponse.Contains("Success"))
                                        {
                                            Console.WriteLine($"{bankAccresponse.CustomerID}  Data Update Successfully");
                                        }

                                        bankAccDetails_log = await _refreshBAD.MapBank_Acc_Response_Details_ToLogModel(bankAccresponse);
                                        Console.WriteLine("Canara Bank Get Bank Details Api Failed.");
                                        bankAccDetails_log.Status = "Error";
                                        bankAccDetails_log.Message = $"Canara Bank Get Bank Details Api Failed.+ {decryptedFailedApiresponse}";
                                        bankAccDetails_log.ActionName = "CanaraBankApiFailed";
                                        bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                        var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                        if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                                        {
                                            Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Bank Account Details Log Insertion Failed");

                                        }
                                        // strStatus = "Failure";
                                    }


                                }
                                catch(Exception ex)
                                {
                                    bankAccresponse = await _refreshBAD.GetBnakAccDetailResponse_byMkey(mkey);
                                    if (ex.InnerException != null)
                                    {
                                        bankAccresponse.ResponseData = ex.InnerException.InnerException.Message + " " + ex.Message;
                                    }

                                    //bankAccResp.acctNumber = null;
                                    bankAccresponse.CurrentBalance = null;
                                    bankAccresponse.UnclearFunds = null;
                                    bankAccresponse.NetBalance = null;
                                    bankAccresponse.BalAvailable = null;
                                    bankAccresponse.HoldAmount = null;
                                    bankAccresponse.Overdraft = null;
                                    bankAccresponse.CustomerName = null;
                                    //bankAccResp.customerID = AccountBalance.customerID;
                                    bankAccresponse.ResponseTime = DateTime.UtcNow;
                                    bankAccresponse.Status = "Error";
                                    bankAccresponse.LastUpdateDate = DateTime.UtcNow;
                                    bankAccresponse.CreatedBy = Convert.ToInt32(AdminUserId);
                                    bankAccresponse.LastUpdatedBy = Convert.ToInt32(AdminUserId);
                                    bankAccresponse.CreatedByName = Name;
                                    var updateresponse = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccresponse, AdminUserId);
                                    
                                    
                                    if (updateresponse != null)
                                    {
                                        Console.WriteLine($" Bank Account Response Update At Mkey :{mkey} Successfully ");
                                    }
                                    else
                                    {
                                        Console.WriteLine($" Bank Account Response Failed ANd Mkey :{mkey}");
                                    }
                                    bankAccDetails_log = await _refreshBAD.MapBank_Acc_Response_Details_ToLogModel(bankAccresponse);
                                    Console.WriteLine("Error Exception Response Closed");
                                    bankAccDetails_log.Status = "Error";
                                    bankAccDetails_log.Message = " Exception Occurred While Processing Bank Account Number: " + bankAcc.AcctNumber + " Error: " + ex.Message; 
                                    bankAccDetails_log.ActionName = "ExceptionInBankAccountProcessing";
                                    bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                    var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                    if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                                    {
                                        Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Bank Account Details Log Insertion Failed");
                                    }
                                    Console.WriteLine("Exception Occurred While Mapping Bank Acc Summary To Log Model: " + ex.Message);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Account Number is Null or Empty.");
                                bankAccDetails_log.Status = "Error";
                                bankAccDetails_log.Message = " Account Number is Null or Empty.";
                                bankAccDetails_log.ActionName = "AccountNumberNullOrEmpty";
                                bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                                var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                                if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                                {
                                    Console.WriteLine("Bank Account Details Log Inserted Successfully");
                                }
                                else
                                {
                                    Console.WriteLine("Bank Account Details Log Insertion Failed");
                                }
                               // strStatus = "Failure";
                            }


                        }
                        catch (Exception ex)
                        {
                            System.IO.File.AppendAllText(
                                strFolder + "\\Log.txt",
                                $"{DateTime.Now} ERROR: {ex.Message}\n"
                            );
                            Console.WriteLine("Exception Occurred While Processing Bank Account Number: " + bankAcc.AcctNumber + " Error: " + ex.Message);
                            bankAccDetails_log.Status = "Error";
                            bankAccDetails_log.Message = " Exception Occurred While Processing Bank Account Number: " + bankAcc.AcctNumber + " Error: " + ex.Message;
                            bankAccDetails_log.ActionName = "ExceptionInBankAccountProcessing";
                            bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                            var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                            if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                            {
                                Console.WriteLine("Bank Account Details Log Inserted Successfully");
                            }
                            else
                            {
                                Console.WriteLine("Bank Account Details Log Insertion Failed");
                            }
                            strStatus = "Failure";
                        }
                    }

                    //responseObject.Status = result.Status;
                    //responseObject.Message = result.Message;
                    //responseObject.Data = result.Data;
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred During Refreshing Bank Account Details by Account Number: " + ex.Message);
                bankAccDetails_log.Status = "Error";
                bankAccDetails_log.Message = " Exception Occurred During Refreshing Bank Account Details by Account Number: " + ex.Message;
                bankAccDetails_log.ActionName = "ExceptionInRefreshBankAccountDetailsByAccountNo";
                bankAccDetails_log.MethodName = "RefreshBankAccountDetailsByAccountNo";
                var ExceptiombankdetailsLog = await _refreshBAD.InsertBankAccResponseLogAsync(bankAccDetails_log, userid);
                if (ExceptiombankdetailsLog.Contains("Record inserted successfully"))
                {
                    Console.WriteLine("Bank Account Details Log Inserted Successfully");
                }
                else
                {
                    Console.WriteLine("Bank Account Details Log Insertion Failed");
                }
                //responseObject.Status = "Error";
                //responseObject.Message = "An error occurred during Refreshing Bank Account Details by Account Number";
                //responseObject.Data = new { error = ex.Message };
                return Ok(bankAccDetails_log);
            }
            return Ok(bankAccDetails_log);
        }

        #region
        // Commented Code for RefreshBankAccountDetailsByBank Method


        //[HttpPost("BanK_Account_Details")]

        //public async Task<IActionResult> RefreshBankAccountDetailsByBank([FromBody] CommoninputResponse request)
        //{
        //    CommonResponseObject responseObject = new CommonResponseObject();
        //    try
        //    {
        //        int? userId = request.UserId;
        //        //int userid = request.UserId;
        //        string Name = User.Identity.Name?.ToString();
        //        //decimal UserId = await _auth.GetUserIdbyUserName(Name);
        //        decimal UserId = Convert.ToDecimal(request.UserId);
        //        string userid = string.Empty;
        //        // string Name = string.Empty;
        //        int? businessGroupId = 0;
        //        var encrypt_BankPortal = _commonService.EncryptionObje<CommoninputResponse>(request, _encryptionKey);
        //        var dashboard_BnakPortal = _commonService.DecryptObject<CommoninputResponse>(encrypt_BankPortal, _encryptionKey);  //jsonEncrypt.jsonEncrypt
        //        if (dashboard_BnakPortal.UserId > 0)
        //        {
        //            UserId = Convert.ToDecimal(dashboard_BnakPortal.UserId);
        //            userid = Convert.ToString(UserId);
        //            businessGroupId = dashboard_BnakPortal.BusinessGroupId;
        //        }
        //        else
        //        {
        //            //Name = User.Identity.Name?.ToString();
        //            UserId = await _auth.GetUserIdbyUserName(Name);
        //            userid = Convert.ToString(UserId);
        //            businessGroupId = 1;
        //        }

        //        int bankAccResDMkey = 0;
        //        AccountBalanceDetails bankAccResp = new AccountBalanceDetails();
        //        string env = _evn;
        //        string AdminUserId = userid;   /*System.Configuration.ConfigurationManager.AppSettings["AdminUserId"]*/;
        //        string strFolder = "D:\\Applications";
        //        //string strFolder = "D:\\Data\\User Profile\\Itemad Hyder\\repo\\Log";
        //        //try
        //        //{
        //        Console.WriteLine("Process Started");
        //        //int mkey = 3;
        //        string decryptedpvtkey = null;
        //        string decryptedPublicKey = null;
        //        var bankApprovalConfig = await _refreshBAD.GetBank_ApprovalConfig(env);
        //        string Asymkey;
        //        //string password = System.Configuration.ConfigurationManager.AppSettings["EncryptionPassword"];
        //        //string plainFilePath = System.Configuration.ConfigurationManager.AppSettings["PlainSecretFile"];
        //        string fileName = _fileName;  //System.Configuration.ConfigurationManager.AppSettings["FileName"];
        //        Asymkey = "33fb9741ea6c3cf6b1e8f62e103059a13a9aaa821bbfdbf9e39190a272ebcb19";
        //        //string filepath = await FindOrCreateWordDocumentPath(fileName);
        //        //SecureWordFile(filepath, bankApprovalConfig.AsymKey);
        //        //Asymkey = await ReadProtectedWordDocument(filepath, bankApprovalConfig.AsymKey);
        //        //if (Asymkey == null)
        //        //{
        //        //    Console.WriteLine(Asymkey);
        //        //}
        //        Console.WriteLine("Asymetric key Fetch Successfully");
        //        String SHARED_SYMMETRIC_KEY = "33fb9741ea6c3cf6b1e8f62e103059a13a9aaa821bbfdbf9e39190a272ebcb19";

        //        /*Asymkey.Replace(" ", "")
        //                                             .Replace("\n", "")
        //                                             .Replace("\r", "");*/

        //        if (!SHARED_SYMMETRIC_KEY.Contains("NoAsymetricKeyPresentinTheAuthkeyFiles"))
        //        {
        //            //Program jw = new Program();
        //            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //            rsa = new RSACryptoServiceProvider(2048);
        //            Console.WriteLine("Genrate Key");
        //            Console.WriteLine("bankApprovalConfig.PubKey" + bankApprovalConfig.PubKey);
        //            Aes aes1 = Aes.Create();
        //            aes1.Key = _refreshBAD.digest(SHARED_SYMMETRIC_KEY);
        //            Console.WriteLine("Genrate Key--Decode Value/Param:" + aes1.Key);

        //            try
        //            {
        //                decryptedPublicKey = Jose.JWT.Decode(bankApprovalConfig.PubKey, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
        //                Console.WriteLine("Genrate Key--generated");
        //                //DecryptJWE(bankApprovalConfig.PubKey, Asymkey);
        //                Console.WriteLine("Public key Decryption Sucessfully");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Public key Decryption Failed");
        //                Console.WriteLine("Error" + ex.ToString());
        //                Console.Out.FlushAsync();

        //            }
        //            try
        //            {
        //                decryptedpvtkey = Jose.JWT.Decode(bankApprovalConfig.PvtKey, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
        //                //  var decryptedpvtkey = DecryptJWE(bankApprovalConfig.PvtKey, Asymkey);
        //                Console.WriteLine("Public key Decryption Sucessfully");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Private key Decryption Failed");
        //                Console.WriteLine("Error" + ex.ToString());
        //                Console.Out.FlushAsync();
        //            }


        //            var bankAccountDetailsList = await _refreshBAD.GetBankAccDetailsAsync();
        //            int Accountbalancemkey = 0;
        //            HttpClient Client = new HttpClient();
        //            System.Net.ServicePointManager.SecurityProtocol =
        //           SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        //            foreach (var bankAcc in bankAccountDetailsList)
        //            {
        //                try
        //                {


        //                    Console.WriteLine("==========================================================================================");
        //                    Console.WriteLine("Foreach Start");
        //                    Accountbalancemkey = await _refreshBAD.InsertAccountBalanceDetailsAsync(bankAcc.AcctNumber, bankAcc.CustomerID);
        //                    bankAccResDMkey = Accountbalancemkey;
        //                    if (Accountbalancemkey <= 0)
        //                    {
        //                        Console.WriteLine($" Insert In Account BalanaceTable Failed");
        //                    }
        //                    Console.WriteLine($" Insert Account Balanace with Mkey Value = {Accountbalancemkey}");
        //                    var custumerRequest = await _refreshBAD.GenerateCustomerRequestJson(bankAcc);
        //                    Console.WriteLine("Custumer Authorization Payload :" + custumerRequest);
        //                    string enc_token = Jose.JWT.Encode(custumerRequest, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
        //                    Console.WriteLine("Authorization paylload Encrypt" + enc_token);
        //                    var BankInquirySignaturePaylodjson = await _refreshBAD.GenerateRequestJsonSignature(bankAcc, enc_token);
        //                    var BankInquiryjson = await _refreshBAD.GenerateRequestJson(bankAcc, enc_token);
        //                    Console.WriteLine("BankInquiryjson " + BankInquiryjson);
        //                    //string signature = SignData(BankInquiryjson, decryptedpvtkey);
        //                    string signature = _refreshBAD.SignData(BankInquirySignaturePaylodjson, decryptedpvtkey);    // Chnages By Narend SIr 
        //                    Console.WriteLine("Digital Signature: " + signature);
        //                    Console.WriteLine("==========================================================================================");
        //                    //bool isVerified = VerifyData(BankInquiryjson, signature, decryptedPublicKey);
        //                    bool isVerified =  _refreshBAD.VerifyData(BankInquirySignaturePaylodjson, signature, decryptedPublicKey);   // Chnages By Narend SIr 
        //                    if (isVerified)
        //                    {
        //                        Console.WriteLine("Signature Verification: " + isVerified);
        //                        Console.WriteLine("==========================================================================================");
        //                        //Console.ReadKey();
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("Signature Verification Failed");
        //                    }

        //                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, bankApprovalConfig.CallURL);
        //                    Console.WriteLine("BankInquiryjson" + BankInquiryjson);

        //                    if (bankApprovalConfig.KeyType.Trim() == env)
        //                    {
        //                        // var httpRequest = new HttpRequestMessage(HttpMethod.Post, bankApprovalConfig.CallURL)
        //                        //{
        //                        httpRequest.Content = new StringContent(BankInquiryjson, Encoding.UTF8, "application/json");
        //                        //};
        //                    }
        //                    else
        //                    {
        //                        //var httpRequest = new HttpRequestMessage(HttpMethod.Post, bankApprovalConfig.CallURL)
        //                        //{
        //                        httpRequest.Content = new StringContent(BankInquiryjson, Encoding.UTF8, "application/json");
        //                        //};
        //                    }
        //                    if (!bankApprovalConfig.CallURL.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        throw new InvalidOperationException("The API call must be made over HTTPS.");
        //                    }

        //                    // Add headers
        //                    Console.WriteLine("Post Called");
        //                    httpRequest.Headers.Add("x-client-id", bankApprovalConfig.clientid);
        //                    httpRequest.Headers.Add("x-client-secret", bankApprovalConfig.clientsecret);
        //                    httpRequest.Headers.Add("x-client-certificate", bankApprovalConfig.clientcertificate);
        //                    httpRequest.Headers.Add("x-api-interaction-id", bankApprovalConfig.apiinteractionid);
        //                    httpRequest.Headers.Add("x-signature", signature);
        //                    httpRequest.Headers.Add("x-forwarded-for", "222");
        //                    httpRequest.Headers.Add("x-timestamp", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString());
        //                    var response = await Client.SendAsync(httpRequest);

        //                    try
        //                    {
        //                        Console.WriteLine("No response received from API.");
        //                        if (response == null || !(response.IsSuccessStatusCode))
        //                        {
        //                            string apitriggerAgainResponse = await _refreshBAD.CallBankApiWithRetryAsync(bankApprovalConfig, BankInquiryjson, signature);

        //                            if (File.Exists(strFolder + "\\Log.txt") == false)
        //                            {
        //                                using (System.IO.StreamWriter sw = File.CreateText(strFolder + "\\Log.txt"))
        //                                {

        //                                    sw.Write("\n");
        //                                    sw.WriteLine("---------------------Success-----------------------------------------" + "\n");
        //                                    sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                                    sw.WriteLine(System.DateTime.Now + "\n");
        //                                    sw.WriteLine(apitriggerAgainResponse + "\n");
        //                                    sw.WriteLine("---------------------Success-----------------------------------------" + "\n");


        //                                }
        //                            }
        //                            else
        //                            {
        //                                using (System.IO.StreamWriter sw = File.AppendText(strFolder + "\\Log.txt"))
        //                                {
        //                                    sw.Write("\n");
        //                                    sw.WriteLine("--------------------------------------------------------------" + "\n");
        //                                    sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                                    sw.WriteLine(System.DateTime.Now + "\n");
        //                                    sw.WriteLine(apitriggerAgainResponse + "\n");
        //                                    sw.WriteLine("--------------------------------------------------------------" + "\n");



        //                                }
        //                            }

        //                            bankAccResp = await _refreshBAD.GetAccountBalanceDetailsByMkeyAsync(Accountbalancemkey);
        //                            bankAccResp.ResponseData = apitriggerAgainResponse;
        //                            bankAccResp.Status = "Error";
        //                            bankAccResp.ResponseTime = DateTime.Now;
        //                            bankAccResp.LAST_UPDATE_DATE = DateTime.Now;
        //                            bankAccResp.CREATED_BY = Convert.ToInt32(AdminUserId);
        //                            var updateResult = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccResp);
        //                            Console.WriteLine("DB updated for API error: " + updateResult);
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        string apitriggerAgainResponse = await _refreshBAD.CallBankApiWithRetryAsync(bankApprovalConfig, BankInquiryjson, signature);

        //                        if (File.Exists(strFolder + "\\Log.txt") == false)
        //                        {
        //                            using (System.IO.StreamWriter sw = File.CreateText(strFolder + "\\Log.txt"))
        //                            {

        //                                sw.Write("\n");
        //                                sw.WriteLine("---------------------Success-----------------------------------------" + "\n");
        //                                sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                                sw.WriteLine(System.DateTime.Now + "\n");
        //                                sw.WriteLine(apitriggerAgainResponse + ex.Message + "\n");
        //                                sw.WriteLine("---------------------Success-----------------------------------------" + "\n");


        //                            }
        //                        }
        //                        else
        //                        {
        //                            using (System.IO.StreamWriter sw = File.AppendText(strFolder + "\\Log.txt"))
        //                            {
        //                                sw.Write("\n");
        //                                sw.WriteLine("--------------------------------------------------------------" + "\n");
        //                                sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                                sw.WriteLine(System.DateTime.Now + "\n");
        //                                sw.WriteLine(apitriggerAgainResponse + ex.Message + "\n");
        //                                sw.WriteLine("--------------------------------------------------------------" + "\n");



        //                            }
        //                        }

        //                        Console.WriteLine("Exception during API call: " + ex.Message);
        //                        // DB error log
        //                        bankAccResp = await _refreshBAD.GetAccountBalanceDetailsByMkeyAsync(Accountbalancemkey);
        //                        bankAccResp.ResponseData = apitriggerAgainResponse + "Exception Message" + ex.Message;
        //                        bankAccResp.Status = "Error";
        //                        bankAccResp.ResponseTime = DateTime.Now;
        //                        bankAccResp.LAST_UPDATE_DATE = DateTime.Now;
        //                        bankAccResp.CREATED_BY = Convert.ToInt32(AdminUserId);
        //                        var updateResult = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccResp);
        //                        Console.WriteLine("DB updated for API error: " + updateResult);
        //                    }

        //                    Console.WriteLine("Accountbalancemkey" + Accountbalancemkey);
        //                    Console.WriteLine("Post response" + response);
        //                    if (response.IsSuccessStatusCode)
        //                    {
        //                        string jsonContent = await response.Content.ReadAsStringAsync();
        //                        Console.WriteLine("jsonContent" + jsonContent);

        //                        if (File.Exists(strFolder + "\\Log.txt") == false)
        //                        {
        //                            using (System.IO.StreamWriter sw = File.CreateText(strFolder + "\\Log.txt"))
        //                            {

        //                                sw.Write("\n");
        //                                sw.WriteLine("---------------------Success-----------------------------------------" + "\n");
        //                                sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                                sw.WriteLine(System.DateTime.Now + "\n");
        //                                sw.WriteLine(jsonContent + "\n");
        //                                sw.WriteLine("---------------------Success-----------------------------------------" + "\n");


        //                            }
        //                        }
        //                        else
        //                        {
        //                            using (System.IO.StreamWriter sw = File.AppendText(strFolder + "\\Log.txt"))
        //                            {
        //                                sw.Write("\n");
        //                                sw.WriteLine("--------------------------------------------------------------" + "\n");
        //                                sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                                sw.WriteLine(System.DateTime.Now + "\n");
        //                                sw.WriteLine(jsonContent + "\n");
        //                                sw.WriteLine("--------------------------------------------------------------" + "\n");



        //                            }
        //                        }

        //                        Console.WriteLine(" Start Deserilized The JsonContent :", jsonContent);

        //                        JsonDocument doc = JsonDocument.Parse(jsonContent);
        //                        Console.WriteLine("JsonDocument doc jsonContent", jsonContent);
        //                        //JsonElement bodyElement = doc.RootElement
        //                        //                             .GetProperty("Response")
        //                        //                             .GetProperty("body");

        //                        Console.WriteLine("Deserilized Start for Body Model");

        //                        RootResponse bodyObj = System.Text.Json.JsonSerializer.Deserialize<RootResponse>(jsonContent);

        //                        if (bodyObj.Response.metadata != null)
        //                        {
        //                            Console.WriteLine("Deserilized Completed for Body Model");
        //                            Console.WriteLine("Encrypt Data: " + bodyObj.Response.body.encryptData);
        //                            Console.WriteLine("Account Status: " + bodyObj.Response.body.accountDetails.status);
        //                            Console.WriteLine("Branch Code: " + bodyObj.Response.body.customerAccountDetails.accountDetails.branchCode);
        //                            if (bodyObj.Response.body.encryptData != null)
        //                            {
        //                                string decryptedSucessApiresponse = Jose.JWT.Decode(bodyObj.Response.body.encryptData, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
        //                                Console.WriteLine("Decrypted bodyObj.encryptData", decryptedSucessApiresponse);
        //                                var AccountBalance = System.Text.Json.JsonSerializer.Deserialize<AccountBalanceModel>(decryptedSucessApiresponse);
        //                                Console.WriteLine("Start Update Method Execution");
        //                                var updatestatusresponse = _refreshBAD.UpdateBankDetailsHdr(AccountBalance.currentBalance, DateTime.Now, AccountBalance.acctNumber, AccountBalance.unclearFunds, AccountBalance.netBalance, AccountBalance.balAvailable, AccountBalance.holdAmount, AccountBalance.overdraft, AccountBalance.customerName);
        //                                if (updatestatusresponse.Contains("Update successful for AccountNo:"))
        //                                {
        //                                    Console.WriteLine("Update successful in Bank_Details_Hdr Table");
        //                                }
        //                                else
        //                                {
        //                                    Console.WriteLine("Update Failed in Bank_Details_Hdr Table");
        //                                }
        //                                Console.WriteLine("Deserilized the JsonContent Completed");
        //                                Console.WriteLine("jsonContent Data pass into AccountBalanceModel");
        //                                bankAccResp = await _refreshBAD.GetAccountBalanceDetailsByMkeyAsync(Accountbalancemkey);
        //                                bankAccResp.acctNumber = AccountBalance.acctNumber;
        //                                bankAccResp.currentBalance = Convert.ToDecimal(AccountBalance.currentBalance);
        //                                bankAccResp.unclearFunds = Convert.ToDecimal(AccountBalance.unclearFunds);
        //                                bankAccResp.netBalance = Convert.ToDecimal(AccountBalance.netBalance);
        //                                bankAccResp.balAvailable = Convert.ToDecimal(AccountBalance.balAvailable);
        //                                bankAccResp.holdAmount = Convert.ToDecimal(AccountBalance.holdAmount);
        //                                bankAccResp.overdraft = Convert.ToDecimal(AccountBalance.overdraft);
        //                                bankAccResp.customerName = AccountBalance.customerName;
        //                                bankAccResp.customerID = AccountBalance.customerID;
        //                                bankAccResp.Status = "Success";
        //                                bankAccResp.ResponseData = jsonContent;
        //                                bankAccResp.ResponseTime = DateTime.Now;
        //                                bankAccResp.result = bodyObj.Response.metadata.status.result;
        //                                bankAccResp.BankStatus = bodyObj.Response.body.accountDetails.status;
        //                                bankAccResp.relationship = bodyObj.Response.body.accountDetails.relationship;
        //                                bankAccResp.chequeBookFacility = bodyObj.Response.body.accountDetails.chequeBookFacility;
        //                                bankAccResp.minBalance = Convert.ToDecimal(bodyObj.Response.body.accountDetails.minBalance);
        //                                bankAccResp.dateString = bodyObj.Response.body.accountDetails.openingDate.dateString;
        //                                bankAccResp.branchCode = bodyObj.Response.body.customerAccountDetails.accountDetails.branchCode;
        //                                bankAccResp.acctTypeCode = bodyObj.Response.body.customerAccountDetails.accountDetails.acctTypeCode;
        //                                bankAccResp.currencyDesc = bodyObj.Response.body.customerAccountDetails.accountDetails.currencyDesc;
        //                                bankAccResp.currencyCode = bodyObj.Response.body.customerAccountDetails.accountDetails.currencyCode;
        //                                bankAccResp.accountType = bodyObj.Response.body.customerAccountDetails.accountDetails.accountType;

        //                                bankAccResp.CREATED_BY = Convert.ToInt32(AdminUserId);
        //                                bankAccResp.LAST_UPDATE_DATE = DateTime.Now;
        //                                Console.WriteLine("UpdateAccountBalanceDetailsAsync Method Start");
        //                                string updateResponse = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccResp);
        //                                Console.WriteLine("UpdateAccountBalanceDetailsAsync Method Closed");
        //                                if (updateResponse.Contains("Success"))
        //                                {
        //                                    Console.WriteLine($"{bankAccResp.customerID}  Data Update Successfully");
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            Console.WriteLine("==========================================================================================");
        //                            Console.WriteLine("Api Response Failed");
        //                            string encryptData = string.Empty;
        //                            //var doc = JsonDocument.Parse(jsonContent);
        //                            using (doc = JsonDocument.Parse(jsonContent))
        //                            {
        //                                encryptData = doc.RootElement
        //                                                       .GetProperty("Response")
        //                                                       .GetProperty("body")
        //                                                       .GetProperty("encryptData")
        //                                                       .GetString();

        //                                Console.WriteLine("Encrypted Data: " + encryptData);

        //                            }
        //                            Console.WriteLine("jsonContent Decrypt");
        //                            Console.WriteLine("encryptData for Find the error Cause Start");
        //                            var decryptedFailedApiresponse = Jose.JWT.Decode(encryptData, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);
        //                            Console.WriteLine("encryptData for Find the error Cause Completed");

        //                            bankAccResp = await _refreshBAD.GetAccountBalanceDetailsByMkeyAsync(Accountbalancemkey);

        //                            bankAccResp.ResponseData = decryptedFailedApiresponse;
        //                            //bankAccResp.acctNumber = null;
        //                            bankAccResp.currentBalance = null;
        //                            bankAccResp.unclearFunds = null;
        //                            bankAccResp.netBalance = null;
        //                            bankAccResp.balAvailable = null;
        //                            bankAccResp.holdAmount = null;
        //                            bankAccResp.overdraft = null;
        //                            bankAccResp.customerName = null;
        //                            //bankAccResp.customerID = AccountBalance.customerID;
        //                            bankAccResp.ResponseTime = DateTime.Now;
        //                            bankAccResp.Status = "Error";
        //                            bankAccResp.LAST_UPDATE_DATE = DateTime.Now;
        //                            bankAccResp.CREATED_BY = Convert.ToInt32(AdminUserId);
        //                            var updateresponses = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccResp);
        //                            if (updateresponses.Contains("Success"))
        //                            {
        //                                Console.WriteLine($"{bankAccResp.customerID}  Data Update Successfully");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("==========================================================================================");
        //                        Console.WriteLine("Api Response Failed Then Start Decrypt Error Response");
        //                        string jsonContent = await response.Content.ReadAsStringAsync();
        //                        Console.WriteLine("jsonContent Start To Decrypt");

        //                        string encryptData = string.Empty;
        //                        var doc = JsonDocument.Parse(jsonContent);
        //                        using (doc = JsonDocument.Parse(jsonContent))
        //                        {
        //                            encryptData = doc.RootElement
        //                                                   .GetProperty("Response")
        //                                                   .GetProperty("body")
        //                                                   .GetProperty("encryptData")
        //                                                   .GetString();

        //                            Console.WriteLine("Encrypted Data: " + encryptData);
        //                        }
        //                        Console.WriteLine("jsonContent Decrypt");
        //                        Console.WriteLine("encryptData for Find the error Cause Start");

        //                        var decryptedFailedApiresponse = Jose.JWT.Decode(encryptData, aes1.Key, JweAlgorithm.A256KW, JweEncryption.A128CBC_HS256);

        //                        Console.WriteLine("encryptData for Find the error Cause Completed");

        //                        if (File.Exists(strFolder + "\\Log.txt") == false)
        //                        {
        //                            using (System.IO.StreamWriter sw = File.CreateText(strFolder + "\\Log.txt"))
        //                            {

        //                                sw.Write("\n");
        //                                sw.WriteLine("--------------------Failed------------------------------------------" + "\n");
        //                                sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                                sw.WriteLine(System.DateTime.Now + "\n");
        //                                sw.WriteLine(jsonContent + "\n");
        //                                sw.WriteLine("--------------------------------------------------------------" + "\n");


        //                            }
        //                        }
        //                        else
        //                        {
        //                            using (System.IO.StreamWriter sw = File.AppendText(strFolder + "\\Log.txt"))
        //                            {
        //                                sw.Write("\n");
        //                                sw.WriteLine("------------------------Failed--------------------------------------" + "\n");
        //                                sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                                sw.WriteLine(System.DateTime.Now + "\n");
        //                                sw.WriteLine(jsonContent + "\n");
        //                                sw.WriteLine("--------------------------------------------------------------" + "\n");

        //                            }
        //                        }



        //                        Console.WriteLine("jsonContent Decrypted");
        //                        bankAccResp = await _refreshBAD.GetAccountBalanceDetailsByMkeyAsync(Accountbalancemkey);
        //                        bankAccResp.ResponseData = decryptedFailedApiresponse;
        //                        //bankAccResp.acctNumber = null;
        //                        bankAccResp.currentBalance = null;
        //                        bankAccResp.unclearFunds = null;
        //                        bankAccResp.netBalance = null;
        //                        bankAccResp.balAvailable = null;
        //                        bankAccResp.holdAmount = null;
        //                        bankAccResp.overdraft = null;
        //                        bankAccResp.customerName = null;
        //                        //bankAccResp.customerID = AccountBalance.customerID;
        //                        bankAccResp.ResponseTime = DateTime.UtcNow;
        //                        bankAccResp.Status = "Error";
        //                        bankAccResp.LAST_UPDATE_DATE = DateTime.UtcNow;
        //                        bankAccResp.CREATED_BY = Convert.ToInt32(AdminUserId);


        //                    }
        //                    Console.WriteLine("UpdateAccountBalanceDetailsAsync Method Start");
        //                    var updateresponse = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccResp);
        //                    if (updateresponse.Contains("Success"))
        //                    {
        //                        Console.WriteLine($"{bankAccResp.customerID}  Data Update Successfully");
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine("==========================================================================================");
        //                    Console.WriteLine("Error Exception Response Start");



        //                    if (File.Exists(strFolder + "\\Log.txt") == false)
        //                    {
        //                        using (System.IO.StreamWriter sw = File.CreateText(strFolder + "\\Log.txt"))
        //                        {

        //                            sw.Write("\n");
        //                            sw.WriteLine("--------------------Failed------------------------------------------" + "\n");
        //                            sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                            sw.WriteLine(System.DateTime.Now + "\n");
        //                            sw.WriteLine(ex.Message + "\n");
        //                            sw.WriteLine("--------------------------------------------------------------" + "\n");
        //                        }
        //                    }
        //                    else
        //                    {
        //                        using (System.IO.StreamWriter sw = File.AppendText(strFolder + "\\Log.txt"))
        //                        {
        //                            sw.Write("\n");
        //                            sw.WriteLine("------------------------Failed--------------------------------------" + "\n");
        //                            sw.WriteLine(bankAcc.AcctNumber + "\n");
        //                            sw.WriteLine(System.DateTime.Now + "\n");
        //                            sw.WriteLine(ex.Message + "\n");
        //                            sw.WriteLine("--------------------------------------------------------------" + "\n");
        //                        }
        //                    }


        //                    bankAccResp = await _refreshBAD.GetAccountBalanceDetailsByMkeyAsync(bankAccResDMkey);
        //                    if (ex.InnerException != null)
        //                    {
        //                        bankAccResp.ResponseData = ex.InnerException.InnerException.Message + " " + ex.Message;
        //                    }
        //                    else
        //                    {
        //                        //bankAccResp.ResponseData = "Exception:" + " " + ex.Message;
        //                    }

        //                    //bankAccResp.acctNumber = null;
        //                    bankAccResp.currentBalance = null;
        //                    bankAccResp.unclearFunds = null;
        //                    bankAccResp.netBalance = null;
        //                    bankAccResp.balAvailable = null;
        //                    bankAccResp.holdAmount = null;
        //                    bankAccResp.overdraft = null;
        //                    bankAccResp.customerName = null;
        //                    //bankAccResp.customerID = AccountBalance.customerID;
        //                    bankAccResp.ResponseTime = DateTime.UtcNow;
        //                    bankAccResp.Status = "Error";
        //                    bankAccResp.LAST_UPDATE_DATE = DateTime.UtcNow;
        //                    bankAccResp.CREATED_BY = Convert.ToInt32(AdminUserId);
        //                    var updateresponse = await _refreshBAD.UpdateAccountBalanceDetailsAsync(bankAccResp);
        //                    if (updateresponse != null)
        //                    {
        //                        Console.WriteLine($" Bank Account Response Update At Mkey :{bankAccResDMkey} Successfully ");
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine($" Bank Account Response Failed ANd Mkey :{bankAccResDMkey}");
        //                    }
        //                    Console.WriteLine("Error Exception Response Closed");
        //                }
        //            }

        //            Console.WriteLine("Foreach Loop End");
        //            Console.WriteLine($"\n🔓 Bank Account Approval Console Application Completed ");
        //        }
        //        else
        //        {
        //            Console.WriteLine("No AsymetricKey Present in The Authkey Files");
        //        }

        //        //}
        //        //catch(Exception ex)
        //        //{
        //        //    bankAccResp = await GetAccountBalanceDetailsByMkeyAsync(bankAccResDMkey);
        //        //    bankAccResp.ResponseData = ex.InnerException.InnerException.Message + ex.Message;
        //        //    bankAccResp.currentBalance = null;
        //        //    bankAccResp.unclearFunds = null;
        //        //    bankAccResp.netBalance = null;
        //        //    bankAccResp.balAvailable = null;
        //        //    bankAccResp.holdAmount = null;
        //        //    bankAccResp.overdraft = null;
        //        //    bankAccResp.customerName = null;
        //        //    //bankAccResp.customerID = AccountBalance.customerID;
        //        //    bankAccResp.ResponseTime = DateTime.UtcNow;
        //        //    bankAccResp.Status = "Error";
        //        //    bankAccResp.LAST_UPDATE_DATE = DateTime.UtcNow;
        //        //    bankAccResp.CREATED_BY = Convert.ToInt32(AdminUserId);
        //        //    var updateresponse = await UpdateAccountBalanceDetailsAsync(bankAccResp);
        //        //    if (updateresponse != null)
        //        //    {
        //        //        Console.WriteLine($" Bank Account Response Update At Mkey :{bankAccResDMkey} Successfully ");
        //        //    }
        //        //    else
        //        //    {
        //        //        Console.WriteLine($" Bank Account Response Failed ANd Mkey :{bankAccResDMkey}");
        //        //    }
        //        //        Console.WriteLine(ex.ToString());
        //        //}

        //    }
        //    catch (Exception ex)
        //    {
        //        responseObject.Status = "Error";
        //        responseObject.Message = "An error occurred during Refreshing Bank Account Details by Subsidiary ID";
        //        responseObject.Data = new { error = ex.Message };
        //        return Ok(responseObject);
        //    }
        //}

        #endregion
        #endregion
    }
}
