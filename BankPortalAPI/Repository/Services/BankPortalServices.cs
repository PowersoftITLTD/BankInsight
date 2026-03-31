using BankPortalAPI.Model;
using BankPortalAPI.Repository.Iservices;
using Dapper;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Transactions;

namespace BankPortalAPI.Repository.Services
{
    public class BankPortalServices : IBankPortalServices
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;
        //private readonly Msg91Settings _settings;
        // private readonly SmtpSettings _smtpSettings;
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;
       // private readonly IBankAcc_Summary_StrBuilder _bankAcc_Summary_StrBuilder;
        public BankPortalServices(IConfiguration configuration, HttpClient httpClient, SqlConnection sqlConnection)  //IUserService userService,
        {
            //_userService = userService;
            _configuration = configuration;
            _connection = sqlConnection;
            _httpClient = httpClient;
           // _bankAcc_Summary_StrBuilder = bankAcc_Summary_StrBuilder;
            // _settings = settings.Value;
            // Set default headers
            // _httpClient.DefaultRequestHeaders.Add("authkey", _settings.AuthKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
          
        }
        public Task<BankPortal_model> GetBankDetails()
        {
            throw new NotImplementedException();
        }
        public async Task<List<BankPortal_model>> GetAllBankDetails(CommonListParameters commonListParameters) // int userId, int businessGroupId
        {
            var result = new List<BankPortal_model>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("usp_DemoGetAllBankDetails", conn))    // usp_GetAllBankDetails 
            {
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@UserId", userId);
                //cmd.Parameters.AddWithValue("@BusinessGroupId", businessGroupId);

                cmd.Parameters.AddWithValue("@UserId", commonListParameters.UserId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BusinessGroupId", commonListParameters.BusinessGroupId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Entity", commonListParameters.Entity ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Project", commonListParameters.Project ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Building", commonListParameters.Building ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Bank", commonListParameters.Bank ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Account", commonListParameters.Account ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BalanceRange", commonListParameters.BalanceRange ?? (object)DBNull.Value);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new BankPortal_model
                        {
                            Mkey = reader.GetInt32(reader.GetOrdinal("Mkey")),
                            EntryDateTime = reader.GetDateTime(reader.GetOrdinal("EntryDateTime")),
                            LegalEntityId = reader.GetInt32(reader.GetOrdinal("LegalEntityId")),
                            LegalEntityName = reader["LegalEntityName"]?.ToString(),
                            ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                            ProjectName = reader["ProjectName"]?.ToString(),
                            BuildingId = reader.GetInt32(reader.GetOrdinal("BuildingId")),
                            BuildingName = reader["BuildingName"]?.ToString(),
                            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                            AccountName = reader["AccountName"]?.ToString(),
                            AccountNo = reader["AccountNo"]?.ToString(),
                            IFSCCode = reader["IFSCCode"]?.ToString(),
                            BankId = reader.GetInt32(reader.GetOrdinal("BankId")),
                            BankName = reader["BankName"]?.ToString(),
                            BranchName = reader["BranchName"]?.ToString(),
                            AccountType = reader["AccountType"]?.ToString(),
                            TagType = reader["TagType"]?.ToString(),
                            CustId = reader["CustId"]?.ToString(),
                            LastBalance = reader.GetDecimal(reader.GetOrdinal("LastBalance")),
                            unclearFunds = reader.GetDecimal(reader.GetOrdinal("unclearFunds")),
                            netBalance = reader.GetDecimal(reader.GetOrdinal("netBalance")),
                            balAvailable = reader.GetDecimal(reader.GetOrdinal("balAvailable")),
                            holdAmount = reader.GetDecimal(reader.GetOrdinal("holdAmount")),
                            overdraft = reader.GetDecimal(reader.GetOrdinal("overdraft")),
                            LastTransactionDatetime = reader.IsDBNull(reader.GetOrdinal("LastTransactionDatetime"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("LastTransactionDatetime")),
                            ActiveFlag = reader["ActiveFlag"]?.ToString(),
                            Status = reader["Status"]?.ToString(),
                            Process_Flag = reader["Process_Flag"]?.ToString(),
                            ATTRIBUTE1 = reader["ATTRIBUTE1"]?.ToString(),
                            ATTRIBUTE2 = reader["ATTRIBUTE2"]?.ToString(),
                            ATTRIBUTE3 = reader["ATTRIBUTE3"]?.ToString(),
                            ATTRIBUTE4 = reader["ATTRIBUTE4"]?.ToString(),
                            ATTRIBUTE5 = reader["ATTRIBUTE5"]?.ToString(),
                            CREATED_BY = reader.GetDecimal(reader.GetOrdinal("CREATED_BY")),
                            CREATION_DATE = reader.GetDateTime(reader.GetOrdinal("CREATION_DATE")),
                            LAST_UPDATED_BY = reader.IsDBNull(reader.GetOrdinal("LAST_UPDATED_BY"))
                                ? (decimal?)null
                                : reader.GetDecimal(reader.GetOrdinal("LAST_UPDATED_BY")),
                            LAST_UPDATE_DATE = reader.IsDBNull(reader.GetOrdinal("LAST_UPDATE_DATE"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("LAST_UPDATE_DATE")),
                            DELETE_FLAG = Convert.ToChar(reader["DELETE_FLAG"])
                        });
                    }
                }
            }

            return result;
        }

        public async Task<CommonResponseObject> GetAllBankDetails_Ps(CommonListParameters commonListParameters)
        {
            var response = new CommonResponseObject();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();

                    parameters.Add("@UserId", commonListParameters.UserId);
                    parameters.Add("@BusinessGroupId", commonListParameters.BusinessGroupId);
                    parameters.Add("@Entity", commonListParameters.Entity);
                    parameters.Add("@Project", commonListParameters.Project);
                    parameters.Add("@Building", commonListParameters.Building);
                    parameters.Add("@Bank", commonListParameters.Bank);
                    parameters.Add("@Account", commonListParameters.Account);
                    parameters.Add("@BalanceRange", commonListParameters.BalanceRange);

                    var result = (await conn.QueryAsync<BankPortal_model>(
                        "usp_DemoGetAllBankDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    )).ToList();

                    if (!result.Any())
                    {
                        response.Status = "NoData";
                        response.Message = "No bank details found";
                        response.Data = new List<BankPortal_model>();
                    }
                    else
                    {
                        response.Status = "Success";
                        response.Message = "Records fetched successfully";
                        response.Data = result;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = "Something went wrong while fetching bank details";
                response.Data = null;

                // Log ex if required
            }

            return response;
        }

        public async Task<string> InsertOrUpdateBankApprovalAsync(Bank_Approval_Hdr_Model model)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var responseMessage = string.Empty;

                if (model.Mkey > 0) // Update case
                {

                    var bankApproval = await GetBankApprovalByUserId(model.Mkey, model.UserId, model.BusinessGroupId);
                    string bankapprovalhistory = await InsertBank_Approval_H(bankApproval);
                    if (bankapprovalhistory.Contains("Success"))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@Mkey", model.Mkey);
                        parameters.Add("@Amount", model.Amount);
                        parameters.Add("@ActionCode", model.ActionCode);
                        parameters.Add("@ActionTime", model.ActionTime);
                        //parameters.Add("@ApproverName", model.ApproverName);
                        parameters.Add("@UserID", model.UserId);
                        parameters.Add("@BusinessGroupId", model.BusinessGroupId);
                        parameters.Add("@LAST_UPDATED_BY", model.LAST_UPDATED_BY);
                        parameters.Add("@LAST_UPDATED_BY_Name", model.LAST_UPDATED_BY_Name);
                        parameters.Add("@LAST_UPDATE_DATE", model.LAST_UPDATE_DATE ?? DateTime.Now);
                        parameters.Add("@responseMessage", dbType: DbType.String, size: 250, direction: ParameterDirection.Output);

                        await connection.ExecuteAsync("uspUpdateBankApprovalHdr", parameters, commandType: CommandType.StoredProcedure);
                        responseMessage = parameters.Get<string>("@responseMessage");
                    }
                    else
                    {
                        responseMessage = string.Empty;
                        responseMessage = "Insert failed In Bank Approval Hdr History Table";
                    }


                }
                else // Insert case
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@EntryDateTime", model.EntryDateTime ?? DateTime.Now);
                    parameters.Add("@LegalEntityId", model.LegalEntityId);
                    parameters.Add("@LegalEntityName", model.LegalEntityName);
                    parameters.Add("@ProjectId", model.ProjectId);
                    parameters.Add("@ProjectName", model.ProjectName);
                    parameters.Add("@BuildingId", model.BuildingId);
                    parameters.Add("@BuildingName", model.BuildingName);
                    parameters.Add("@TransactionType", model.TransactionType);
                    parameters.Add("@Amount", model.Amount);
                    parameters.Add("@DisplayText", model.DisplayText);
                    parameters.Add("@NS_Internal_ID", model.NS_Internal_ID);
                    parameters.Add("@NS_User_ID", model.NS_User_ID);
                    parameters.Add("@NS_User_Name", model.NS_User_Name);
                    parameters.Add("@LastTransactionDatetime", model.LastTransactionDatetime);
                    parameters.Add("@AppoverID", model.AppoverID);
                    parameters.Add("@ApproverName", model.ApproverName);
                    parameters.Add("@RequestedBy", model.RequestedBy);
                    parameters.Add("@RequestedByName", model.RequestedByName);
                    parameters.Add("@ActionCode", model.ActionCode);
                    parameters.Add("@ActionTime", model.ActionTime);
                    parameters.Add("@ActiveFlag", model.ActiveFlag);
                    parameters.Add("@Status", model.Status);
                    parameters.Add("@Process_Flag", model.Process_Flag);
                    parameters.Add("@UserID", model.UserId);
                    parameters.Add("@@BusinessGroupId", model.BusinessGroupId);
                    parameters.Add("@ATTRIBUTE1", model.ATTRIBUTE1);
                    parameters.Add("@ATTRIBUTE2", model.ATTRIBUTE2);
                    parameters.Add("@ATTRIBUTE3", model.ATTRIBUTE3);
                    parameters.Add("@ATTRIBUTE4", model.ATTRIBUTE4);
                    parameters.Add("@ATTRIBUTE5", model.ATTRIBUTE5);
                    parameters.Add("@CREATED_BY", model.CREATED_BY);
                    parameters.Add("@CREATED_BY_Name", model.CREATED_BY_Name);
                    parameters.Add("@CREATION_DATE", model.CREATION_DATE);
                    parameters.Add("@LAST_UPDATED_BY", model.LAST_UPDATED_BY);
                    parameters.Add("@LAST_UPDATED_BY_Name", model.LAST_UPDATED_BY_Name);
                    parameters.Add("@LAST_UPDATE_DATE", model.LAST_UPDATE_DATE);
                    parameters.Add("@DELETE_FLAG", model.DELETE_FLAG);
                    parameters.Add("@responseMessage", dbType: DbType.String, size: 250, direction: ParameterDirection.Output);
                    await connection.ExecuteAsync("uspInsertBankApprovalHdr", parameters, commandType: CommandType.StoredProcedure);
                    responseMessage = parameters.Get<string>("@responseMessage");
                }

                return responseMessage;
            }
        }

        public async Task<Bank_Approval_Hdr_Model> GetBankApprovalByUserId(int mkey, int? userId, int? BusinessGroupId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var storeprocedure = string.Empty;
                    storeprocedure = "uspGetBankApprovalDetails";
                    var parameters = new DynamicParameters();
                    parameters.Add("@Mkey", mkey);
                    parameters.Add("@UserId", userId);
                    parameters.Add("@BusinessGroupId", BusinessGroupId);
                    var result = await connection.QueryFirstOrDefaultAsync<Bank_Approval_Hdr_Model>(
                                storeprocedure,
                                parameters,
                                commandType: CommandType.StoredProcedure
                     );
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CommonTransactionsModel>> GetApprovedBankApproval_HdrList(int? userId, int? BusinessGroupId)
        {
            try
            {
                var transaction = new CommonTransactionsModel();
                var transactionlist = new List<CommonTransactionsModel>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var storeprocedure = "Sp_GetActiveBankApprovalHdr";

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@BusinessGroupId", BusinessGroupId);

                    // QueryAsync returns IEnumerable<T>
                    var result = await connection.QueryAsync<Bank_Approval_Hdr_Model>(
                        storeprocedure,
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                    foreach (var item in result)
                    {
                        transaction = new CommonTransactionsModel
                        {
                            amount = item.Amount,
                            legalEntityName = item.LegalEntityName,
                            ProjectName = item.ProjectName,
                            from = item.RequestedByName,
                            to = item.ApproverName,
                            status=item.Status
                        };
                        transactionlist.Add(transaction);
                    }
                    return transactionlist.ToList();
                }
            }
            catch (Exception ex)
            {
                // Log if needed, then rethrow
                throw;
            }
        }


        public async Task<List<CommonTransactionsModel>> GetPendingBankApproval_HdrList(int? userId, int? BusinessGroupId)
        {
            try
            {
                var transaction = new CommonTransactionsModel();
                var transactionlist = new List<CommonTransactionsModel>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var storeprocedure = string.Empty;
                    storeprocedure = "Sp_GetPendingBankApprovalHdr";
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@BusinessGroupId", BusinessGroupId);
                    var result = await connection.QueryAsync<Bank_Approval_Hdr_Model> (
                                storeprocedure,
                                parameters,
                                commandType: CommandType.StoredProcedure
                     );
                    //return result.ToList();

                    foreach (var item in result)
                    {
                        transaction = new CommonTransactionsModel
                        {
                            amount = item.Amount,
                            legalEntityName = item.LegalEntityName,
                            ProjectName = item.ProjectName,
                            from = item.RequestedByName,
                            to = item.ApproverName
                        };
                        transactionlist.Add(transaction);
                    }
                    return transactionlist.ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CommonTransactionsModel>> GetRejectedBankApproval_HdrList(int? userId, int? BusinessGroupId)
        {
            try
            {
                var transaction = new CommonTransactionsModel();
                var transactionlist = new List<CommonTransactionsModel>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var storeprocedure = string.Empty;
                    storeprocedure = "Sp_GetRejectedBankApprovalHdr";
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@BusinessGroupId", BusinessGroupId);
                    var result = await connection.QueryAsync<Bank_Approval_Hdr_Model>(
                                storeprocedure,
                                parameters,
                                commandType: CommandType.StoredProcedure
                     );
                    foreach (var item in result)
                    {
                        transaction = new CommonTransactionsModel
                        {
                            amount = item.Amount,
                            legalEntityName = item.LegalEntityName,
                            ProjectName = item.ProjectName,
                            from = item.RequestedByName,
                            to = item.ApproverName
                        };
                        transactionlist.Add(transaction);
                    }
                    return transactionlist.ToList();
                    //return transactionlist.ToList();
                    //return result.ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<CommonTransactionsModel>> GetCompletedBankApproval_HdrList(int? userId, int? BusinessGroupId)
        {
            try
            {
                var transaction = new CommonTransactionsModel();
                var transactionlist = new List<CommonTransactionsModel>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var storeprocedure = string.Empty;
                    storeprocedure = "Sp_GetCompletedBankApprovalHdr";
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    parameters.Add("@BusinessGroupId", BusinessGroupId);
                    var result = await connection.QueryAsync<Bank_Approval_Hdr_Model>(
                                storeprocedure,
                                parameters,
                                commandType: CommandType.StoredProcedure
                     );
                    //return result.ToList();

                    foreach(var item in result)
                    {
                        transaction = new CommonTransactionsModel
                        {
                            amount = item.Amount,
                            legalEntityName = item.LegalEntityName,
                            ProjectName = item.ProjectName,
                            from = item.RequestedByName,
                            to = item.ApproverName
                        };
                        transactionlist.Add(transaction);
                    }
                    return transactionlist.ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> InsertBank_Approval_H(Bank_Approval_Hdr_Model model)
        {
            try
            {
                var bankapprovalHdrHistory = new Bank_Approval_Hdr_H_Model
                {

                    HIST_DATE = DateTime.UtcNow,
                    Mkey = model.Mkey,
                    EntryDateTime = model.EntryDateTime,
                    LegalEntityId = model.NS_Internal_ID,
                    LegalEntityName = model.LegalEntityName,
                    ProjectId = model.ProjectId,
                    ProjectName = model.ProjectName,
                    BuildingId = model.BuildingId,
                    BuildingName = model.BuildingName,
                    TransactionType = model.TransactionType,
                    LastTransactionDatetime = DateTime.UtcNow,
                    Amount = model.Amount,
                    DisplayText = model.DisplayText,
                    NS_Internal_ID = model.NS_Internal_ID,
                    NS_User_ID = model.NS_User_ID,
                    NS_User_Name = model.NS_User_Name,
                    AppoverID = model.AppoverID,
                    ApproverName = model.ApproverName,
                    RequestedBy = model.RequestedBy,
                    RequestedByName = model.RequestedByName,
                    ActionCode = model.ActionCode,
                    ActionTime = model.ActionTime,
                    ActiveFlag = model.ActiveFlag,
                    Status = model.Status,
                    Process_Flag = model.Process_Flag,
                    ATTRIBUTE1 = model.ATTRIBUTE1,
                    ATTRIBUTE2 = model.ATTRIBUTE2,
                    ATTRIBUTE3 = model.ATTRIBUTE3,
                    ATTRIBUTE4 = model.ATTRIBUTE4,
                    ATTRIBUTE5 = model.ATTRIBUTE5,
                    CREATED_BY = model.CREATED_BY,
                    CREATION_DATE = model.CREATION_DATE,
                    CREATED_BY_Name = model.CREATED_BY_Name,
                    LAST_UPDATED_BY = model.LAST_UPDATED_BY,
                    LAST_UPDATED_BY_Name = model.LAST_UPDATED_BY_Name,
                    LAST_UPDATE_DATE = model.LAST_UPDATE_DATE,
                    DELETE_FLAG = Convert.ToChar(model.DELETE_FLAG)
                };



                string responseMessage;

                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("uspInsertBankApprovalHdrH", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Add parameters from model
                    //cmd.Parameters.AddWithValue("@HISTSEQ_NO", model.HISTSEQ_NO);
                    cmd.Parameters.AddWithValue("@HIST_DATE", bankapprovalHdrHistory.HIST_DATE);
                    cmd.Parameters.AddWithValue("@Mkey", model.Mkey);
                    cmd.Parameters.AddWithValue("@EntryDateTime", (object?)model.EntryDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LegalEntityId", (object?)model.LegalEntityId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LegalEntityName", (object?)model.LegalEntityName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProjectId", (object?)model.ProjectId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProjectName", (object?)model.ProjectName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BuildingId", (object?)model.BuildingId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BuildingName", (object?)model.BuildingName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TransactionType", (object?)model.TransactionType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Amount", (object?)model.Amount ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DisplayText", (object?)model.DisplayText ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NS_Internal_ID", (object?)model.NS_Internal_ID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NS_User_ID", (object?)model.NS_User_ID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NS_User_Name", (object?)model.NS_User_Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastTransactionDatetime", (object?)model.LastTransactionDatetime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppoverID", (object?)model.AppoverID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApproverName", (object?)model.ApproverName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RequestedBy", (object?)model.RequestedBy ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RequestedByName", (object?)model.RequestedByName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionCode", (object?)model.ActionCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionTime", (object?)model.ActionTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActiveFlag", (object?)model.ActiveFlag ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (object?)model.Status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Process_Flag", model.Process_Flag);
                    cmd.Parameters.AddWithValue("@ATTRIBUTE1", (object?)model.ATTRIBUTE1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ATTRIBUTE2", (object?)model.ATTRIBUTE2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ATTRIBUTE3", (object?)model.ATTRIBUTE3 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ATTRIBUTE4", (object?)model.ATTRIBUTE4 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ATTRIBUTE5", (object?)model.ATTRIBUTE5 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CREATED_BY", model.CREATED_BY);
                    cmd.Parameters.AddWithValue("@CREATED_BY_Name", (object?)model.CREATED_BY_Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CREATION_DATE", model.CREATION_DATE);
                    cmd.Parameters.AddWithValue("@LAST_UPDATED_BY", (object?)model.LAST_UPDATED_BY ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LAST_UPDATED_BY_Name", (object?)model.LAST_UPDATED_BY_Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LAST_UPDATE_DATE", (object?)model.LAST_UPDATE_DATE ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DELETE_FLAG", model.DELETE_FLAG);
                    cmd.Parameters.AddWithValue("@UserID", (object?)model.UserId ?? DBNull.Value); // Not inserted
                    cmd.Parameters.AddWithValue("@BusinessGroupId", (object?)model.BusinessGroupId ?? DBNull.Value);
                    // Output parameter
                    SqlParameter outputParam = new SqlParameter("@responseMessage", SqlDbType.NVarChar, 250)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    responseMessage = outputParam.Value?.ToString() ?? "No response";
                }
                return responseMessage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Bank_Approval_Hdr_Model>> GetBankApprovalHdrList(char activeFlag, int? userId, int? businessGroupId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ActiveFlag", activeFlag, DbType.AnsiStringFixedLength, ParameterDirection.Input, 1);
                    parameters.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@BusinessGroupId", businessGroupId, DbType.Int32, ParameterDirection.Input);

                    var result = await conn.QueryAsync<Bank_Approval_Hdr_Model>(
                        "USP_GetBankApprovalHdrList",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<BankApprovalQuery>> GetBankApprovalQueryListAsync(int? userId = null, int? businessGroupId = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@BusinessGroupId", businessGroupId, DbType.Int32, ParameterDirection.Input);

                var result = await conn.QueryAsync<BankApprovalQuery>(
                    "USP_GetBankApprovalList",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }

        public async Task<List<EntityBalance_Model>> GetEntityBankAccountBalanceAsync(int? userId = null, int? businessGroupId = null)
        {
            var result = new List<EntityBalance_Model>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId, DbType.Int32);
                    parameters.Add("@BusinessGroupId", businessGroupId, DbType.Int32);

                    var queryResult = await connection.QueryAsync<EntityBalance_Model>(
                        "SP_Get_Entity_Balance",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    result = queryResult.ToList();
                }
                catch (SqlException sqlEx)
                {
                    // Log SQL-related errors here
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");
                    throw; // Re-throw or return custom error if needed
                }
                catch (Exception ex)
                {
                    // Log general errors
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    throw; // Or handle as per your application requirement
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        await connection.CloseAsync();
                }
            }

            return result;
        }

        public async Task<List<BankTopProjectbyBalance>> GetBankTopbyProjectBalanceAsync(int? userId = null, int? businessGroupId = null)
        {
            var result = new List<BankTopProjectbyBalance>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId, DbType.Int32);
                    parameters.Add("@BusinessGroupId", businessGroupId, DbType.Int32);

                    var queryResult = await connection.QueryAsync<BankTopProjectbyBalance>(
                        "sp_Get_Project_Balance",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    result = queryResult.ToList();
                }
                catch (SqlException sqlEx)
                {
                    // Log SQL-related errors here
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");
                    throw; // Re-throw or return custom error if needed
                }
                catch (Exception ex)
                {
                    // Log general errors
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    throw; // Or handle as per your application requirement
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        await connection.CloseAsync();
                }
            }

            return result;
        }

        public async Task<List<AccountType_BalanceModel>> GetAccountTypeBalanceAsync(int? userId = null, int? businessGroupId = null)
        {
            var result = new List<AccountType_BalanceModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId, DbType.Int32);
                    parameters.Add("@BusinessGroupId", businessGroupId, DbType.Int32);

                    var queryResult = await connection.QueryAsync<AccountType_BalanceModel>(
                        "sp_Get_AccountType_Balance",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    result = queryResult.ToList();
                }
                catch (SqlException sqlEx)
                {
                    // Log SQL-related errors here
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");
                    throw; // Re-throw or return custom error if needed
                }
                catch (Exception ex)
                {
                    // Log general errors
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    throw; // Or handle as per your application requirement
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        await connection.CloseAsync();
                }
            }

            return result;
        }


        public async Task<List<EntityModel>> GetEntityListAsync(CommonListParameters commonListParameters)
        {
            var result = new List<EntityModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", commonListParameters.UserId, DbType.Int32);
                    parameters.Add("@BusinessGroupId", commonListParameters.BusinessGroupId, DbType.Int32);
                    parameters.Add("@Entity", commonListParameters.Entity, DbType.String);
                    parameters.Add("@Project", commonListParameters.Project, DbType.String);
                    parameters.Add("@Building", commonListParameters.Building, DbType.String);
                    parameters.Add("@Bank", commonListParameters.Bank, DbType.String);
                    parameters.Add("@Account", commonListParameters.Account, DbType.String);
                    parameters.Add("@BalanceRange", commonListParameters.BalanceRange, DbType.String);
                    var queryResult = await connection.QueryAsync<EntityModel>(
                        "SP_Get_DemoEntity",
                        parameters,
                        commandType: CommandType.StoredProcedure);    //SP_Get_EntityList

                    result = queryResult.ToList();
                }
                catch (SqlException sqlEx)
                {
                    // Log SQL-related errors here
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");
                    throw; // Re-throw or return custom error if needed
                }
                catch (Exception ex)
                {
                    // Log general errors
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    throw; // Or handle as per your application requirement
                }
                //finally
                //{
                //    if (connection.State != ConnectionState.Closed)
                //        await connection.CloseAsync();
                //}
            }

            return result;
        }

        public async Task<List<ProjectModel>> GetProjectListAsync(CommonListParameters commonListParameters)   //int? userId = null, int? businessGroupId = null
        {
            var result = new List<ProjectModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var parameters = new DynamicParameters();
                    //parameters.Add("@UserId", userId, DbType.Int32);
                    //parameters.Add("@BusinessGroupId", businessGroupId, DbType.Int32);
                    await connection.OpenAsync();
                    parameters.Add("@UserId", commonListParameters.UserId, DbType.Int32);
                    parameters.Add("@BusinessGroupId", commonListParameters.BusinessGroupId, DbType.Int32);
                    parameters.Add("@Entity", commonListParameters.Entity, DbType.String);
                    parameters.Add("@Project", commonListParameters.Project, DbType.String);
                    parameters.Add("@Building", commonListParameters.Building, DbType.String);
                    parameters.Add("@Bank", commonListParameters.Bank, DbType.String);
                    parameters.Add("@Account", commonListParameters.Account, DbType.String);
                    parameters.Add("@BalanceRange", commonListParameters.BalanceRange, DbType.String);
                    var queryResult = await connection.QueryAsync<ProjectModel>(
                        "SP_Get_DemoProject",
                        parameters,
                        commandType: CommandType.StoredProcedure);   //SP_Get_ProjectList

                    result = queryResult.ToList();
                }
                catch (SqlException sqlEx)
                {
                    // Log SQL-related errors here
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");
                    throw; // Re-throw or return custom error if needed
                }
                catch (Exception ex)
                {
                    // Log general errors
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    throw; // Or handle as per your application requirement
                }
                //finally
                //{
                //    if (connection.State != ConnectionState.Closed)
                //        await connection.CloseAsync();
                //}
            }

            return result;
        }


        public async Task<List<BankAccountDetailsModel>> GetBankAccountDetailsAsync(int? userId = null, int? businessGroupId = null)
        {
            var result = new List<BankAccountDetailsModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId, DbType.Int32);
                    parameters.Add("@BusinessGroupId", businessGroupId, DbType.Int32);

                    var queryResult = await connection.QueryAsync<BankAccountDetailsModel>(
                        "SP_Get_BankAcoount_Details",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    result = queryResult.ToList();
                }
                catch (SqlException sqlEx)
                {
                    // Log SQL-related errors here
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");
                    throw; // Re-throw or return custom error if needed
                }
                catch (Exception ex)
                {
                    // Log general errors
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    throw; // Or handle as per your application requirement
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        await connection.CloseAsync();
                }
            }

            return result;
        }

        public async Task<List<BankDetailByCard>> GetBankDetailsByCardAsync(int? userId = null, int? businessGroupId = null)
        {
            var result = new List<BankDetailByCard>();


            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId, DbType.Int32);
                    parameters.Add("@BusinessGroupId", businessGroupId, DbType.Int32);

                    var queryResult = await connection.QueryAsync<BankDetailByCard>(
                        "SP_GetBankDetail_Card",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    result = queryResult.ToList();
                }
                catch (SqlException sqlEx)
                {
                    // Log SQL-related errors here
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");
                    throw; // Re-throw or return custom error if needed
                }
                catch (Exception ex)
                {
                    // Log general errors
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    throw; // Or handle as per your application requirement
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        await connection.CloseAsync();
                }
            }

            return result;
        }


        public async Task<List<BankPortal_model>> GetALLBankDetailsByCardAsync(int? userId = null, int? businessGroupId = null, int? BankId = null, string BankName = null)
        {
            var result = new List<BankPortal_model>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId, DbType.Int32);
                    parameters.Add("@BusinessGroupId", businessGroupId, DbType.Int32);
                    parameters.Add("@Bank_Id", BankId, DbType.Int32);
                    parameters.Add("@Bank_Name", BankName, DbType.String); // ✅ FIXED: Changed to DbType.String

                    var queryResult = await connection.QueryAsync<BankPortal_model>(
                        "SP_GetBankDetails",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    result = queryResult.ToList();
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    throw;
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        await connection.CloseAsync();
                }
            }

            return result;
        }

        public async Task<IEnumerable<BankPortal_model>> GetAdditionalInformationAsync(CommonSpParameters commonSp)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", commonSp.UserId, DbType.Int32);
                    parameters.Add("@BusinessGroupId", commonSp.BusinessGroupId, DbType.Int32);
                    parameters.Add("@Attribute1", commonSp.Attribute1, DbType.String);
                    parameters.Add("@Attribute2", commonSp.Attribute2, DbType.String);
                    parameters.Add("@Attribute3", commonSp.Attribute3, DbType.String);
                    parameters.Add("@Attribute4", commonSp.Attribute4, DbType.String);

                    var result = await conn.QueryAsync<BankPortal_model>("Sp_GetAdditionalInformation",parameters,commandType: CommandType.StoredProcedure);
                    return result ?? Enumerable.Empty<BankPortal_model>();
                }
            }
            catch (Exception ex)
            {
                // Log exception here if you have a logger
                Console.WriteLine($"Error fetching additional information: {ex.Message}");
                throw; // Rethrow so higher layer can handle it
            }
        }

        // Add more methods as needed

        //public async Task<CommonResponseObject> TriggerBank_Acc_Subsidiary_Summ_NSAsync_L1()
        //{
        //    var responseObject = new CommonResponseObject();
        //    string consumerKey = _consumerKey;      //ConfigurationManager.AppSettings["consumer_key"];
        //    string consumerSecret = _consumer_secret;                //ConfigurationManager.AppSettings["consumer_secret"];
        //    string accessToken = _access_token;                       //ConfigurationManager.AppSettings["access_token"];
        //    string tokenSecret = _token_secret;                        //ConfigurationManager.AppSettings["token_secret"];
        //    string realm = _Realm;                                             //ConfigurationManager.AppSettings["Realm"];
        //    string url = _Url;                                             //ConfigurationManager.AppSettings["Url"];

        //    var authenticator = OAuth1Authenticator.ForAccessToken(
        //        consumerKey,
        //        consumerSecret,
        //        accessToken,
        //        tokenSecret,
        //        OAuthSignatureMethod.HmacSha256
        //    );

        //    // ✅ Realm goes in Authorization (NOT header)
        //    authenticator.Realm = realm;

        //    var client = new RestClient(new RestClientOptions
        //    {
        //        Authenticator = authenticator,
        //        ThrowOnAnyError = false
        //    });

        //    var request = new RestRequest(url, Method.Post);

        //    // ✅ Same headers as curl
        //    request.AddHeader("Prefer", "transient");
        //    request.AddHeader("Content-Type", "application/json");

        //    string payload = _bankAcc_Summary_StrBuilder.GetBank_Acc_Subsidiary_Summ_NSQuery_L1();
        //    request.AddStringBody(payload, DataFormat.Json);
        //    Console.WriteLine(payload);
        //    var response = client.Execute(request);

        //    if (response.IsSuccessful)
        //    {
        //        Console.WriteLine("✅ API Success");
        //        Console.WriteLine(response.Content);
        //        var nsResponse = JsonConvert.DeserializeObject<Bank_Acc_Subsidiary_Summ_NSResponse>(response.Content);
        //        //var result = nsResponse.items.Select(x => new BankDetails_By_Subsidiary
        //        //{
        //        //    Subsidiary = x.Subsidiary,
        //        //    Project = string.IsNullOrWhiteSpace(x.Project) ? null : x.Project.Trim(),
        //        //    AccountType = string.IsNullOrWhiteSpace(x.AccountType) ? null : x.AccountType.Trim(),
        //        //    Account_Bal = string.IsNullOrWhiteSpace(x.Account_Bal.ToString()) ? null : x.Account_Bal,
        //        //    DisplayNameWithHierarchy = string.IsNullOrWhiteSpace(x.DisplayNameWithHierarchy) ? null : x.DisplayNameWithHierarchy.Trim(),
        //        //    LastRecoDate = string.IsNullOrWhiteSpace(x.LastRecoDate) ? null : x.LastRecoDate.Trim(),
        //        //    SubId = int.Parse(x.SubId.ToString()),
        //        //    Closing_Balance_As_Per_Bank_Statement = ParseNullableDecimal(x.Closing_Balance_As_Per_Bank_Statement.ToString()),
        //        //    Current_Account_Balance_As_Per_Bank_Book = ParseNullableDecimal(x.Current_Account_Balance_As_Per_Bank_Book.ToString()),
        //        //    //banktotal = ParseNullableDecimal(x.banktotal),
        //        //    //notcleardbanktotal = ParseNullableDecimal(x.notcleardbanktotal),

        //        //}).ToList();
        //        var result = nsResponse.items.Select(x => new Bank_Acc_Subsidiary_Summ_NS
        //        {
        //            subsidiary = x.Subsidiary,
        //            subid = Convert.ToInt32(x.SubId),
        //            // Closing_Bal = string.IsNullOrEmpty(x.Closing_Bal) ? null : x.Closing_Bal,
        //            // closing_balance_as_per_bank_statement = string.IsNullOrEmpty(x.closing_balance_as_per_bank_statement) ? null : x.closing_balance_as_per_bank_statement.Trim(),
        //            current_account_balance_as_per_bank_book = Convert.ToDecimal(x.Current_Account_Balance_As_Per_Bank_Book) > 0 ? Convert.ToDecimal(x.Current_Account_Balance_As_Per_Bank_Book) : 0,
        //            closing_balance_as_per_bank_statement = Convert.ToDecimal(x.Closing_Balance_As_Per_Bank_Statement) > 0 ? Convert.ToDecimal(x.Closing_Balance_As_Per_Bank_Statement) : 0,
        //        }).ToList();

        //        responseObject.Status = "Success";
        //        responseObject.Message = " API Get SuccessFully";
        //        responseObject.Data = result;
        //    }
        //    else
        //    {
        //        Console.WriteLine("❌ API Failed");
        //        Console.WriteLine($"Status: {response.StatusCode}");
        //        Console.WriteLine(response.Content);
        //        responseObject.Status = "Error";
        //        responseObject.Message = $"Status: {response.StatusCode}, Content: {response.Content}";
        //        responseObject.Data = null;
        //    }
        //    return responseObject;
        //}

        //public async Task<CommonResponseObject> TriggerBankSummary_BY_NetSuiteQlAsync_L2()
        //{
        //    var responseObject = new CommonResponseObject();
        //    //string consumerKey = ConfigurationManager.AppSettings["consumer_key"];
        //    //string consumerSecret = ConfigurationManager.AppSettings["consumer_secret"];
        //    //string accessToken = ConfigurationManager.AppSettings["access_token"];
        //    //string tokenSecret = ConfigurationManager.AppSettings["token_secret"];
        //    //string realm = ConfigurationManager.AppSettings["Realm"];
        //    //string url = ConfigurationManager.AppSettings["Url"];
        //    string consumerKey = _consumerKey;      //ConfigurationManager.AppSettings["consumer_key"];
        //    string consumerSecret = _consumer_secret;                //ConfigurationManager.AppSettings["consumer_secret"];
        //    string accessToken = _access_token;                       //ConfigurationManager.AppSettings["access_token"];
        //    string tokenSecret = _token_secret;                        //ConfigurationManager.AppSettings["token_secret"];
        //    string realm = _Realm;                                             //ConfigurationManager.AppSettings["Realm"];
        //    string url = _Url;                                             //ConfigurationManager.AppSettings["Url"];

        //    var authenticator = OAuth1Authenticator.ForAccessToken(
        //        consumerKey,
        //        consumerSecret,
        //        accessToken,
        //        tokenSecret,
        //        OAuthSignatureMethod.HmacSha256
        //    );

        //    // ✅ Realm goes in Authorization (NOT header)
        //    authenticator.Realm = realm;

        //    var client = new RestClient(new RestClientOptions
        //    {
        //        Authenticator = authenticator,
        //        ThrowOnAnyError = false
        //    });

        //    var request = new RestRequest(url, Method.Post);

        //    // ✅ Same headers as curl
        //    request.AddHeader("Prefer", "transient");
        //    request.AddHeader("Content-Type", "application/json");

        //    string payload = GetSummaryStringQuery_L2();
        //    request.AddStringBody(payload, DataFormat.Json);
        //    Console.WriteLine(payload);
        //    var response = client.Execute(request);

        //    if (response.IsSuccessful)
        //    {
        //        Console.WriteLine("✅ API Success");
        //        Console.WriteLine(response.Content);
        //        var nsResponse = JsonConvert.DeserializeObject<NetSuiteSuiteQlResponse>(response.Content);
        //        var result = nsResponse.items.Select(x => new Bank_Acc_Summ_NS
        //        {
        //            Subsidiary = x.subsidiary,
        //            Project = string.IsNullOrWhiteSpace(x.project) ? null : x.project.Trim(),
        //            accounttype = string.IsNullOrWhiteSpace(x.accounttype) ? null : x.accounttype.Trim(),
        //            custrecord_htl_bank_account_number = string.IsNullOrWhiteSpace(x.custrecord_htl_bank_account_number) ? null : x.custrecord_htl_bank_account_number.Trim(),
        //            displaynamewithhierarchy = string.IsNullOrWhiteSpace(x.displaynamewithhierarchy) ? null : x.displaynamewithhierarchy.Trim(),
        //            lastrecodate = string.IsNullOrWhiteSpace(x.lastrecodate) ? null : x.lastrecodate.Trim(),
        //            SubId = int.Parse(x.subid),
        //            Closing_Balance_As_Per_Bank_Statement = string.IsNullOrEmpty(x.closing_balance_as_per_bank_statement) ? null : x.closing_balance_as_per_bank_statement.Trim(),
        //            Current_Account_Balance_As_Per_Bank_Book = string.IsNullOrEmpty(x.current_account_balance_as_per_bank_book) ? null : x.current_account_balance_as_per_bank_book.Trim(),
        //            account_bal = string.IsNullOrEmpty(x.account_bal) ? null : x.account_bal.Trim(),
        //            banktotal = string.IsNullOrEmpty(x.banktotal) ? null : x.banktotal.Trim(),
        //            notcleardbanktotal = string.IsNullOrEmpty(x.notcleardbanktotal) ? null : x.notcleardbanktotal.Trim(),
        //            projectid = Convert.ToInt32(x.projectId) > 0 ? Convert.ToInt32(x.projectId) : 0

        //        }).ToList();

        //        responseObject.Status = "Success";
        //        responseObject.Message = " API Get SuccessFully";
        //        responseObject.Data = result;
        //    }
        //    else
        //    {
        //        Console.WriteLine("❌ API Failed");
        //        Console.WriteLine($"Status: {response.StatusCode}");
        //        Console.WriteLine(response.Content);
        //        responseObject.Status = "Error";
        //        responseObject.Message = $"Status: {response.StatusCode}, Content: {response.Content}";
        //        responseObject.Data = null;
        //    }
        //    return responseObject;
        //}

        //public static async Task<CommonResponseObject> TriggerBankDetail_By_SusidiaryidAsync_L3(int Subsidiaryid)
        //{
        //    var responseObject = new CommonResponseObject();
        //    string consumerKey = ConfigurationManager.AppSettings["consumer_key"];
        //    string consumerSecret = ConfigurationManager.AppSettings["consumer_secret"];
        //    string accessToken = ConfigurationManager.AppSettings["access_token"];
        //    string tokenSecret = ConfigurationManager.AppSettings["token_secret"];
        //    string realm = ConfigurationManager.AppSettings["Realm"];
        //    string url = ConfigurationManager.AppSettings["Url"];

        //    var authenticator = OAuth1Authenticator.ForAccessToken(
        //        consumerKey,
        //        consumerSecret,
        //        accessToken,
        //        tokenSecret,
        //        OAuthSignatureMethod.HmacSha256
        //    );

        //    // ✅ Realm goes in Authorization (NOT header)
        //    authenticator.Realm = realm;

        //    var client = new RestClient(new RestClientOptions
        //    {
        //        Authenticator = authenticator,
        //        ThrowOnAnyError = false
        //    });

        //    var request = new RestRequest(url, Method.Post);

        //    // ✅ Same headers as curl
        //    request.AddHeader("Prefer", "transient");
        //    request.AddHeader("Content-Type", "application/json");

        //    string payload = GetBankAccountDetailbySubsidiaryid_Query_L3(Subsidiaryid);
        //    request.AddStringBody(payload, DataFormat.Json);
        //    Console.WriteLine(payload);
        //    var response = client.Execute(request);

        //    if (response.IsSuccessful)
        //    {
        //        Console.WriteLine("✅ API Success");
        //        Console.WriteLine(response.Content);
        //        var nsResponse = JsonConvert.DeserializeObject<SubsidiaryNetSuiteQlResponse>(response.Content);
        //        //var result = nsResponse.items.Select(x => new BankDetails_By_Subsidiary
        //        //{
        //        //    Subsidiary = x.Subsidiary,
        //        //    Project = string.IsNullOrWhiteSpace(x.Project) ? null : x.Project.Trim(),
        //        //    AccountType = string.IsNullOrWhiteSpace(x.AccountType) ? null : x.AccountType.Trim(),
        //        //    Account_Bal = string.IsNullOrWhiteSpace(x.Account_Bal.ToString()) ? null : x.Account_Bal,
        //        //    DisplayNameWithHierarchy = string.IsNullOrWhiteSpace(x.DisplayNameWithHierarchy) ? null : x.DisplayNameWithHierarchy.Trim(),
        //        //    LastRecoDate = string.IsNullOrWhiteSpace(x.LastRecoDate) ? null : x.LastRecoDate.Trim(),
        //        //    SubId = int.Parse(x.SubId.ToString()),
        //        //    Closing_Balance_As_Per_Bank_Statement = ParseNullableDecimal(x.Closing_Balance_As_Per_Bank_Statement.ToString()),
        //        //    Current_Account_Balance_As_Per_Bank_Book = ParseNullableDecimal(x.Current_Account_Balance_As_Per_Bank_Book.ToString()),
        //        //    //banktotal = ParseNullableDecimal(x.banktotal),
        //        //    //notcleardbanktotal = ParseNullableDecimal(x.notcleardbanktotal),

        //        //}).ToList();
        //        var result = nsResponse.items.Select(x => new BankDetails_By_Subsidiary
        //        {
        //            Subsidiary = x.Subsidiary,
        //            Project = string.IsNullOrWhiteSpace(x.Project) ? null : x.Project.Trim(),
        //            AccountType = string.IsNullOrWhiteSpace(x.AccountType) ? null : x.AccountType.Trim(),
        //            Account_Bal = string.IsNullOrEmpty(x.Account_Bal) ? null : x.Account_Bal.Trim(),   //ParseNullableDecimal(x.Account_Bal),
        //            DisplayNameWithHierarchy = string.IsNullOrWhiteSpace(x.DisplayNameWithHierarchy) ? null : x.DisplayNameWithHierarchy.Trim(),
        //            LastRecoDate = string.IsNullOrWhiteSpace(x.LastRecoDate) ? null : x.LastRecoDate.Trim(),
        //            SubId = ParseNullableInt(x.SubId),
        //            Closing_Bal = string.IsNullOrEmpty(x.Closing_Bal) ? null : x.Closing_Bal,
        //            Closing_Balance_As_Per_Bank_Statement = string.IsNullOrEmpty(x.Closing_Balance_As_Per_Bank_Statement) ? null : x.Closing_Balance_As_Per_Bank_Statement.Trim(),
        //            Current_Account_Balance_As_Per_Bank_Book = string.IsNullOrEmpty(x.Current_Account_Balance_As_Per_Bank_Book) ? null : x.Current_Account_Balance_As_Per_Bank_Book.Trim(),
        //            Custrecord_Htl_Bank_Account_Number = string.IsNullOrEmpty(x.Custrecord_Htl_Bank_Account_Number) ? null : x.Custrecord_Htl_Bank_Account_Number.Trim(),
        //            projectid = Convert.ToInt32(x.projectid) > 0 ? Convert.ToInt32(x.projectid) : 0
        //        }).ToList();

        //        responseObject.Status = "Success";
        //        responseObject.Message = " API Get SuccessFully";
        //        responseObject.Data = result;
        //    }
        //    else
        //    {
        //        Console.WriteLine("❌ API Failed");
        //        Console.WriteLine($"Status: {response.StatusCode}");
        //        Console.WriteLine(response.Content);
        //        responseObject.Status = "Error";
        //        responseObject.Message = $"Status: {response.StatusCode}, Content: {response.Content}";
        //        responseObject.Data = null;
        //    }
        //    return responseObject;
        //}

        public async Task<IEnumerable<BankAccountSummaryModel>> GetBankAccountSummaryActiveAsync()
        {
            IEnumerable<BankAccountSummaryModel> response;

            try
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    // No input parameters required for this SP

                    response = await db.QueryAsync<BankAccountSummaryModel>(
                        "Sp_GetBankAccountSummary_Active",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                }

                return response;
            }
            catch (SqlException sqlEx)
            {
                // SQL related exception (SP error, timeout, connection issue)
                // Log sqlEx here if logger exists

                throw new Exception(
                    $"SQL Error in GetBankAccountSummaryActiveAsync: {sqlEx.Message}",
                    sqlEx
                );
            }
            catch (Exception ex)
            {
                // General exception
                // Log ex here if logger exists

                throw new Exception(
                    $"Error in GetBankAccountSummaryActiveAsync: {ex.Message}",
                    ex
                );
            }
        }

        public async Task<IEnumerable<BankAccountSummary_Ns_Model>> GetBankAccountSummaryBySubIdAsync(int? subId)
        {
            IEnumerable<BankAccountSummary_Ns_Model> response;
            try
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@SubId", subId, DbType.Int32);

                    response = await db.QueryAsync<BankAccountSummary_Ns_Model>(
                       "Sp_GetBankAccountSummary_BySubId", parameters, commandType: CommandType.StoredProcedure
                   );

                    return response.ToList();
                }
            }
            catch (SqlException sqlEx)
            {
                // SQL error (SP failure, timeout, etc.)
                throw new Exception(
                    $"SQL error while fetching Bank Account Summary for SubId {subId}",
                    sqlEx
                );
            }
            catch (Exception ex)
            {
                // General error
                throw new Exception(
                    $"Error while fetching Bank Account Summary for SubId {subId}",
                    ex
                );
            }
        }

        public async Task<IEnumerable<Bank_Acc_Details_NS_Model>> GetBankAccountSummaryBySubId_ProjectIdAsync(int? subId , int? projectid)
        {
            IEnumerable<Bank_Acc_Details_NS_Model> response;
            try
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@SubId", subId, DbType.Int32);
                    parameters.Add("@Projectid", projectid, DbType.Int32);

                    response = await db.QueryAsync<Bank_Acc_Details_NS_Model>(
                       "Sp_GetBankAccDetails_BySubId_Project", parameters, commandType: CommandType.StoredProcedure
                   );

                    return response.ToList();
                }
            }
            catch (SqlException sqlEx)
            {
                // SQL error (SP failure, timeout, etc.)
                throw new Exception(
                    $"SQL error while fetching Bank Account Summary for SubId {subId}",
                    sqlEx
                );
            }
            catch (Exception ex)
            {
                // General error
                throw new Exception(
                    $"Error while fetching Bank Account Summary for SubId {subId}",
                    ex
                );
            }
        }


        public async Task<BankAccountSummary_ResponseModel> GetMapBankAccountSummary_IntoBankAccountSummary_BySubId(BankAccountSummaryModel bankAccountSummary)
        {
            try
            {
                var BNkAccSum_response = new BankAccountSummary_ResponseModel
                {
                    Mkey= bankAccountSummary.Mkey,
                    subsidiary= bankAccountSummary.subsidiary,
                    custrecord_htl_bank_account_number = bankAccountSummary.custrecord_htl_bank_account_number,
                    description = bankAccountSummary.description,
                    displaynamewithhierarchy = bankAccountSummary.displaynamewithhierarchy,
                    accounttype = bankAccountSummary.accounttype,
                    account_bal = bankAccountSummary.account_bal,
                    banktotal = bankAccountSummary.banktotal,
                    notcleardbanktotal = bankAccountSummary.notcleardbanktotal,
                    subid = bankAccountSummary.subid,
                    project = bankAccountSummary.project,
                    closing_balance_as_per_bank_statement = bankAccountSummary.closing_balance_as_per_bank_statement,
                    current_account_balance_as_per_bank_book = bankAccountSummary.current_account_balance_as_per_bank_book,
                    ATTRIBUTE1 = bankAccountSummary.ATTRIBUTE1,
                    ATTRIBUTE2 = bankAccountSummary.ATTRIBUTE2,
                    ATTRIBUTE3 = bankAccountSummary.ATTRIBUTE3,
                    ATTRIBUTE4 = bankAccountSummary.ATTRIBUTE4,
                    ATTRIBUTE5 = bankAccountSummary.ATTRIBUTE5,
                    CREATED_BY_Name = bankAccountSummary.CREATED_BY_Name,
                    CREATION_DATE = bankAccountSummary.CREATION_DATE,
                    LAST_UPDATED_BY = bankAccountSummary.LAST_UPDATED_BY,
                    LAST_UPDATED_BY_Name = bankAccountSummary.LAST_UPDATED_BY_Name,
                    LAST_UPDATE_DATE = bankAccountSummary.LAST_UPDATE_DATE,
                    DELETE_FLAG = bankAccountSummary.DELETE_FLAG,
                    LastBalance=bankAccountSummary.LastBalance,
                    unclearFunds=bankAccountSummary.unclearFunds,
                    netBalance=bankAccountSummary.netBalance,
                    balAvailable=bankAccountSummary.balAvailable,
                    holdAmount=bankAccountSummary.holdAmount,
                    overdraft=bankAccountSummary.overdraft,
                    customerName=bankAccountSummary.customerName,
                    LastTransactionDatetime=bankAccountSummary.LastTransactionDatetime,

                };
                return BNkAccSum_response;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        //public async Task<BankAccountSummary_ResponseModel> GetMapBankAccountDetails_IntoBankAccountSummary_BySubId(Bank_Acc_Details_NS_Model bank_Acc_Details_NS_)
        //{
        //    try
        //    {
        //        var BNkAccSum_response = new BankAccountSummary_ResponseModel
        //        {
        //            Mkey = bankAccountSummary.Mkey,
        //            subsidiary = bankAccountSummary.subsidiary,
        //            custrecord_htl_bank_account_number = bankAccountSummary.custrecord_htl_bank_account_number,
        //            description = bankAccountSummary.description,
        //            displaynamewithhierarchy = bankAccountSummary.displaynamewithhierarchy,
        //            accounttype = bankAccountSummary.accounttype,
        //            account_bal = bankAccountSummary.account_bal,
        //            banktotal = bankAccountSummary.banktotal,
        //            notcleardbanktotal = bankAccountSummary.notcleardbanktotal,
        //            subid = bankAccountSummary.subid,
        //            project = bankAccountSummary.project,
        //            closing_balance_as_per_bank_statement = bankAccountSummary.closing_balance_as_per_bank_statement,
        //            current_account_balance_as_per_bank_book = bankAccountSummary.current_account_balance_as_per_bank_book,
        //            ATTRIBUTE1 = bankAccountSummary.ATTRIBUTE1,
        //            ATTRIBUTE2 = bankAccountSummary.ATTRIBUTE2,
        //            ATTRIBUTE3 = bankAccountSummary.ATTRIBUTE3,
        //            ATTRIBUTE4 = bankAccountSummary.ATTRIBUTE4,
        //            ATTRIBUTE5 = bankAccountSummary.ATTRIBUTE5,
        //            CREATED_BY_Name = bankAccountSummary.CREATED_BY_Name,
        //            CREATION_DATE = bankAccountSummary.CREATION_DATE,
        //            LAST_UPDATED_BY = bankAccountSummary.LAST_UPDATED_BY,
        //            LAST_UPDATED_BY_Name = bankAccountSummary.LAST_UPDATED_BY_Name,
        //            LAST_UPDATE_DATE = bankAccountSummary.LAST_UPDATE_DATE,
        //            DELETE_FLAG = bankAccountSummary.DELETE_FLAG,
        //        };
        //        return BNkAccSum_response;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}











    }
}
