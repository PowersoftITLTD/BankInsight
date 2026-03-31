using BankPortalAPI.Model;
using BankPortalAPI.Repository.Iservices;
//using BankPortalAPI.Repository_DbConnection;
using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BankPortalAPI.Repository.Services
{
    public class RefreshAll_Bank_Acc_DetailsServices : IRefreshAll_Bank_Acc_DetailsServices
    {
        private readonly IConfiguration _configuration;
        private readonly Iservices.IDapperDbConnection _dapperDbConnection;
        private readonly SqlConnection _connection;
        //private readonly Msg91Settings _settings;
        // private readonly SmtpSettings _smtpSettings;
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;
        // private readonly IBankAcc_Summary_StrBuilder _bankAcc_Summary_StrBuilder;
        public RefreshAll_Bank_Acc_DetailsServices(IConfiguration configuration, HttpClient httpClient, SqlConnection sqlConnection, IOptions<NetSuiteBankDetails> options, IDapperDbConnection dapperDbConnection)  //IUserService userService,
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
            GlobalNetSuiteConfig.Settings = options.Value;
            _dapperDbConnection = dapperDbConnection;
        }

        public string GetBank_Acc_Subsidiary_Summ_NSQuery_L1()
        {
            Console.WriteLine(" Generating Summary String Query...");

            string sql = @"
            select 
                subsidiary,
                subid,
                to_char(nvl(sum(total),0)) Closing_Balance_As_per_Bank_Statement,
                to_char(nvl(sum(account_bal),0)) Current_Account_Balance_as_per_Bank_Book
            from
            (
                select
                    total account_bal,
                    notcleardbanktotal,
                    banktotal,
                    displayNameWithHierarchy,
                    nvl(
                        (
                            select custrecord_date
                            from customrecordbank_reconcililation_balance bk
                            where bk.custrecord_bank = a.id
                              and bk.custrecord_sub = a.subsidiary
                              and bk.custrecord_date =
                              (
                                  select max(ibk.custrecord_date)
                                  from customrecordbank_reconcililation_balance ibk
                                  where ibk.custrecord_bank = a.id
                                    and ibk.custrecord_sub = a.subsidiary
                                    and ibk.custrecord_date <= CURRENT_DATE
                              )
                        ),
                        ''
                    ) lastrecodate,
                    (
                        select custrecord_closing_bal
                        from customrecordbank_reconcililation_balance bk
                        where bk.custrecord_bank = a.id
                          and bk.custrecord_sub = a.subsidiary
                          and bk.custrecord_date =
                          (
                              select max(ibk.custrecord_date)
                              from customrecordbank_reconcililation_balance ibk
                              where ibk.custrecord_bank = a.id
                                and ibk.custrecord_sub = a.subsidiary
                                and ibk.custrecord_date <= CURRENT_DATE
                          )
                    ) total,
                    BUILTIN.DF(a.subsidiary) subsidiary,
                    a.custrecord_htl_bank_account_number,
                    a.description,
                    accounttype,
                    a.subsidiary subid
                from
                (
                    select
                        AccountSubsidiaryMap.subsidiary,
                        account.id,
                        account.custrecord_htl_bank_account_number,
                        account.description,
                        displayNameWithHierarchy,
                        sum(TransactionAccountingLine.debit) debitamt,
                        sum(TransactionAccountingLine.credit) creditamt,
                        sum(
                            nvl(TransactionAccountingLine.debit, 0)
                            - nvl(TransactionAccountingLine.credit, 0)
                        ) total,
                        max(cleareddate) lastrecodate,
                        sum(
                            case
                                when cleareddate is null
                                then nvl(TransactionAccountingLine.debit, 0)
                                else 0
                            end
                            -
                            case
                                when cleareddate is null
                                then nvl(TransactionAccountingLine.credit, 0)
                                else 0
                            end
                        ) banktotal,
                        sum(
                            case
                                when cleareddate is not null
                                then nvl(TransactionAccountingLine.debit, 0)
                                else 0
                            end
                            -
                            case
                                when cleareddate is not null
                                then nvl(TransactionAccountingLine.credit, 0)
                                else 0
                            end
                        ) notcleardbanktotal,
                        account.custrecord_htl_bank_account_type accounttype
                    from account
                    left join TransactionAccountingLine
                           on account.id = TransactionAccountingLine.account
                    left join transaction
                           on TransactionAccountingLine.transaction = transaction.id
                    left join transactionline
                           on transactionline.transaction = TransactionAccountingLine.transaction
                          and transactionline.id = TransactionAccountingLine.transactionline
                    left join AccountSubsidiaryMap
                           on AccountSubsidiaryMap.account = account.id
                    where accttype = 'Bank'
                      and TO_DATE(transaction.trandate) <= TO_DATE(CURRENT_DATE)
                      and recordtype <> 'periodendjournal'
                    group by
                        account.id,
                        displayNameWithHierarchy,
                        AccountSubsidiaryMap.subsidiary,
                        account.custrecord_htl_bank_account_number,
                        account.description,
                        account.custrecord_htl_bank_account_type
                ) a
            )
            where subsidiary != 'Test Subsidiary'
            group by subsidiary, subid
            order by 1";

            // ✅ Wrap SQL in JSON (MANDATORY for NetSuite SuiteQL)
            return $@"{{ ""q"": ""{sql.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ")}"" }}";
        }

        public string GetSummaryStringQuery_L2()
        {
            Console.WriteLine(" Generating Summary String Query...");

            string sql = @"
             select 
                 subid,
                 subsidiary,
                 nvl(custrecordbank_project,' ') project,
                 projectid,
                 to_char(nvl(sum(total),0)) closing_balance_as_per_bank_statement,
                 to_char(nvl(sum(account_bal),0)) current_account_balance_as_per_bank_book
             from 
             (
                 select 
                     subid,
                     total account_bal,
                     notcleardbanktotal,
                     banktotal,
                     displayNameWithHierarchy,
                     nvl(
                         (
                             select custrecord_date
                             from customrecordbank_reconcililation_balance bk
                             where bk.custrecord_bank = a.id
                               and bk.custrecord_sub = a.subsidiary
                               and bk.custrecord_date = 
                               (
                                   select max(ibk.custrecord_date)
                                   from customrecordbank_reconcililation_balance ibk
                                   where ibk.custrecord_bank = a.id
                                     and ibk.custrecord_sub = a.subsidiary
                                     and ibk.custrecord_date <= CURRENT_DATE
                               )
                         ),
                         ''
                     ) lastrecodate,
                     (
                         select custrecord_closing_bal
                         from customrecordbank_reconcililation_balance bk
                         where bk.custrecord_bank = a.id
                           and bk.custrecord_sub = a.subsidiary
                           and bk.custrecord_date = 
                           (
                               select max(ibk.custrecord_date)
                               from customrecordbank_reconcililation_balance ibk
                               where ibk.custrecord_bank = a.id
                                 and ibk.custrecord_sub = a.subsidiary
                                 and ibk.custrecord_date <= CURRENT_DATE
                           )
                     ) total,
                     BUILTIN.DF(a.subsidiary) subsidiary,
                     a.custrecord_htl_bank_account_number,
                     a.description,
                     accounttype,
                     custrecordbank_project custrecordbank_project,
                     projectid
                 from 
                 (
                     select 
                         transactionline.subsidiary subid,
                         AccountSubsidiaryMap.subsidiary,
                         account.id,
                         account.custrecord_htl_bank_account_number,
                         account.description,
                         displayNameWithHierarchy,
                         sum(TransactionAccountingLine.debit) debitamt,
                         sum(TransactionAccountingLine.credit) creditamt,
                         sum(
                             nvl(TransactionAccountingLine.debit,0) -
                             nvl(TransactionAccountingLine.credit,0)
                         ) total,
                         max(cleareddate) lastrecodate,
                         sum(
                             case when cleareddate is null
                                  then nvl(TransactionAccountingLine.debit,0)
                                  else 0 end
                             -
                             case when cleareddate is null
                                  then nvl(TransactionAccountingLine.credit,0)
                                  else 0 end
                         ) banktotal,
                         sum(
                             case when cleareddate is not null
                                  then nvl(TransactionAccountingLine.debit,0)
                                  else 0 end
                             -
                             case when cleareddate is not null
                                  then nvl(TransactionAccountingLine.credit,0)
                                  else 0 end
                         ) notcleardbanktotal,
                         account.custrecord_htl_bank_account_type accounttype,
                         BUILTIN.DF(account.custrecordbank_project) custrecordbank_project,
                         account.custrecordbank_project projectid
                     from account
                     left join TransactionAccountingLine
                            on account.id = TransactionAccountingLine.account
                     left join transaction
                            on TransactionAccountingLine.transaction = transaction.id
                     left join transactionline
                            on transactionline.transaction = TransactionAccountingLine.transaction
                           and transactionline.id = TransactionAccountingLine.transactionline
                     left join AccountSubsidiaryMap
                            on AccountSubsidiaryMap.account = account.id
                     where accttype = 'Bank'
                       and TO_DATE(transaction.trandate) <= TO_DATE(CURRENT_DATE)
                       and recordtype <> 'periodendjournal'
                     group by 
                         account.id,
                         displayNameWithHierarchy,
                         AccountSubsidiaryMap.subsidiary,
                         account.custrecord_htl_bank_account_number,
                         account.description,
                         account.custrecord_htl_bank_account_type,
                         account.custrecordbank_project,
                         transactionline.subsidiary,
                         BUILTIN.DF(account.custrecordbank_project)
                 ) a
             )
             group by subsidiary, custrecordbank_project, subid, projectid
             order by subid";

            // ✅ Wrap SQL in JSON (MANDATORY for NetSuite)
            return $@"{{ ""q"": ""{sql.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ")}"" }}";
        }

        //public static string GetSummaryStringQuery()
        //{
        //    Console.WriteLine(" Generating Summary String Query...");

        //    string sql = @"
        //select subid, subsidiary,
        //       nvl(custrecordbank_project,' ') project,
        //       sum(total) closing_balance_as_per_bank_statement,
        //       sum(account_bal) current_account_balance_as_per_bank_book
        //from (
        //    select subid,
        //           total account_bal,
        //           notcleardbanktotal,
        //           banktotal,
        //           displayNameWithHierarchy,
        //           nvl((
        //               select custrecord_date
        //               from customrecordbank_reconcililation_balance bk
        //               where bk.custrecord_bank=a.id
        //                 and bk.custrecord_sub=a.subsidiary
        //                 and bk.custrecord_date=(
        //                     select max(ibk.custrecord_date)
        //                     from customrecordbank_reconcililation_balance ibk
        //                     where ibk.custrecord_bank=a.id
        //                       and ibk.custrecord_sub=a.subsidiary
        //                       and ibk.custrecord_date<=CURRENT_DATE
        //                 )
        //           ),'') lastrecodate,
        //           (
        //               select custrecord_closing_bal
        //               from customrecordbank_reconcililation_balance bk
        //               where bk.custrecord_bank=a.id
        //                 and bk.custrecord_sub=a.subsidiary
        //                 and bk.custrecord_date=(
        //                     select max(ibk.custrecord_date)
        //                     from customrecordbank_reconcililation_balance ibk
        //                     where ibk.custrecord_bank=a.id
        //                       and ibk.custrecord_sub=a.subsidiary
        //                       and ibk.custrecord_date<=CURRENT_DATE
        //                 )
        //           ) total,
        //           BUILTIN.DF(a.subsidiary) subsidiary,
        //           a.custrecord_htl_bank_account_number,
        //           a.description,
        //           accounttype,
        //           custrecordbank_project custrecordbank_project
        //    from (
        //        select transactionline.subsidiary subid,
        //               AccountSubsidiaryMap.subsidiary,
        //               account.id,
        //               account.custrecord_htl_bank_account_number,
        //               account.description,
        //               displayNameWithHierarchy,
        //               sum(TransactionAccountingLine.debit) debitamt,
        //               sum(TransactionAccountingLine.credit) creditamt,
        //               sum(nvl(TransactionAccountingLine.debit,0) - nvl(TransactionAccountingLine.credit,0)) total,
        //               max(cleareddate) lastrecodate,
        //               sum(
        //                   case when cleareddate is null then nvl(TransactionAccountingLine.debit,0) else 0 end -
        //                   case when cleareddate is null then nvl(TransactionAccountingLine.credit,0) else 0 end
        //               ) banktotal,
        //               sum(
        //                   case when cleareddate is not null then nvl(TransactionAccountingLine.debit,0) else 0 end -
        //                   case when cleareddate is not null then nvl(TransactionAccountingLine.credit,0) else 0 end
        //               ) notcleardbanktotal,
        //               account.custrecord_htl_bank_account_type accounttype,
        //               BUILTIN.DF(account.custrecordbank_project) custrecordbank_project
        //        from account
        //        left join TransactionAccountingLine on Account.id = TransactionAccountingLine.account
        //        left join transaction on TransactionAccountingLine.transaction = transaction.id
        //        left join transactionline on transactionline.transaction = TransactionAccountingLine.transaction
        //                                and transactionline.id = TransactionAccountingLine.transactionline
        //        left join AccountSubsidiaryMap on AccountSubsidiaryMap.account = account.id
        //        where accttype = 'Bank'
        //          and TO_DATE(transaction.trandate) <= TO_DATE(CURRENT_DATE)
        //          and recordtype <> 'periodendjournal'
        //        group by account.id,
        //                 displayNameWithHierarchy,
        //                 AccountSubsidiaryMap.subsidiary,
        //                 account.custrecord_htl_bank_account_number,
        //                 account.description,
        //                 account.custrecord_htl_bank_account_type,
        //                 account.custrecordbank_project,
        //                 transactionline.subsidiary,
        //                 BUILTIN.DF(account.custrecordbank_project)
        //    ) a
        //)
        //group by subsidiary, custrecordbank_project, subid
        //order by subid";

        //    // ✅ Wrap SQL in JSON (MANDATORY)
        //    return $@"{{ ""q"": ""{sql.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ")}"" }}";
        //}

        public string GetBankAccountDetailbySubsidiaryid_Query_L3(int subsidiaryId)
        {
            Console.WriteLine(" Generating Bank Account Detail Query L3...");

            string sql = $@"
             SELECT
                 BUILTIN.DF(a.subsidiary) AS subsidiary,
                 a.subsidiary AS subid,
                 a.custrecordbank_project,
                 a.projectid,
                 a.displaynamewithhierarchy,
                 a.custrecord_htl_bank_account_number,
                 a.accounttype,
                 a.lastrecodate,
                 TO_CHAR(NVL(a.total, 0)) AS account_bal,
                 TO_CHAR(
                     NVL(
                         (
                             SELECT custrecord_closing_bal
                             FROM customrecordbank_reconcililation_balance bk
                             WHERE bk.custrecord_bank = a.id
                               AND bk.custrecord_sub = a.subsidiary
                               AND bk.custrecord_date = (
                                   SELECT MAX(ibk.custrecord_date)
                                   FROM customrecordbank_reconcililation_balance ibk
                                   WHERE ibk.custrecord_bank = a.id
                                     AND ibk.custrecord_sub = a.subsidiary
                                     AND ibk.custrecord_date <= CURRENT_DATE
                               )
                         ),
                         0
                     )
                 ) AS closing_bal
             FROM (
                 SELECT
                     AccountSubsidiaryMap.subsidiary,
                     account.id,
                     account.custrecord_htl_bank_account_number,
                     account.description,
                     displayNameWithHierarchy,
                     SUM(TransactionAccountingLine.debit) AS debitamt,
                     SUM(TransactionAccountingLine.credit) AS creditamt,
                     SUM(
                         NVL(TransactionAccountingLine.debit, 0)
                         - NVL(TransactionAccountingLine.credit, 0)
                     ) AS total,
                     MAX(cleareddate) AS lastrecodate,
                     custrecord_account_account_title AS accounttype,
                     BUILTIN.DF(custrecordbank_project) AS custrecordbank_project,
                     custrecordbank_project AS projectid
                 FROM account
                 LEFT JOIN TransactionAccountingLine
                     ON account.id = TransactionAccountingLine.account
                 LEFT JOIN transaction
                     ON transaction.id = TransactionAccountingLine.transaction
                 LEFT JOIN transactionline
                     ON transactionline.transaction = transaction.id
                    AND transactionline.id = TransactionAccountingLine.transactionline
                 LEFT JOIN AccountSubsidiaryMap
                     ON AccountSubsidiaryMap.account = account.id
                 WHERE accttype = 'Bank'
                   AND TO_DATE(transaction.trandate) <= TO_DATE(CURRENT_DATE)
                   AND recordtype <> 'periodendjournal'
                 GROUP BY
                     account.id,
                     displayNameWithHierarchy,
                     AccountSubsidiaryMap.subsidiary,
                     account.custrecord_htl_bank_account_number,
                     account.description,
                     custrecord_account_account_title,
                     BUILTIN.DF(custrecordbank_project),
                     custrecordbank_project
             ) a
             WHERE a.subsidiary = {subsidiaryId}
             ORDER BY 1, 3";

            // ✅ Wrap SQL in JSON (MANDATORY for SuiteQL)
            return $@"{{ ""q"": ""{sql.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ")}"" }}";
        }
        public async Task<CommonResponseObject> TriggerBankSummary_BY_NetSuiteQlAsync_L2()
        {
            var responseObject = new CommonResponseObject();
            string consumerKey = GlobalNetSuiteConfig.Settings.consumer_key;    //ConfigurationManager.AppSettings["consumer_key"];
            string consumerSecret = GlobalNetSuiteConfig.Settings.consumer_secret;
            string accessToken = GlobalNetSuiteConfig.Settings.access_token;
            string tokenSecret = GlobalNetSuiteConfig.Settings.token_secret;
            string realm = GlobalNetSuiteConfig.Settings.Realm;
            string url = GlobalNetSuiteConfig.Settings.ApiBaseUrl;

            var authenticator = OAuth1Authenticator.ForAccessToken(
                consumerKey,
                consumerSecret,
                accessToken,
                tokenSecret,
                OAuthSignatureMethod.HmacSha256
            );

            // ✅ Realm goes in Authorization (NOT header)
            authenticator.Realm = realm;

            var client = new RestClient(new RestClientOptions
            {
                Authenticator = authenticator,
                ThrowOnAnyError = false
            });

            var request = new RestRequest(url, Method.Post);

            // ✅ Same headers as curl
            request.AddHeader("Prefer", "transient");
            request.AddHeader("Content-Type", "application/json");

            string payload = GetSummaryStringQuery_L2();
            request.AddStringBody(payload, DataFormat.Json);
            Console.WriteLine(payload);
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                Console.WriteLine("✅ API Success");
                Console.WriteLine(response.Content);
                var nsResponse = JsonConvert.DeserializeObject<NetSuiteSuiteQlResponse>(response.Content);
                var result = nsResponse.items.Select(x => new Bank_Acc_Summ_NS
                {
                    Subsidiary = x.subsidiary,
                    Project = string.IsNullOrWhiteSpace(x.project) ? null : x.project.Trim(),
                    accounttype = string.IsNullOrWhiteSpace(x.accounttype) ? null : x.accounttype.Trim(),
                    custrecord_htl_bank_account_number = string.IsNullOrWhiteSpace(x.custrecord_htl_bank_account_number) ? null : x.custrecord_htl_bank_account_number.Trim(),
                    displaynamewithhierarchy = string.IsNullOrWhiteSpace(x.displaynamewithhierarchy) ? null : x.displaynamewithhierarchy.Trim(),
                    lastrecodate = string.IsNullOrWhiteSpace(x.lastrecodate) ? null : x.lastrecodate.Trim(),
                    SubId = int.Parse(x.subid),
                    Closing_Balance_As_Per_Bank_Statement = string.IsNullOrEmpty(x.closing_balance_as_per_bank_statement) ? null : x.closing_balance_as_per_bank_statement.Trim(),
                    Current_Account_Balance_As_Per_Bank_Book = string.IsNullOrEmpty(x.current_account_balance_as_per_bank_book) ? null : x.current_account_balance_as_per_bank_book.Trim(),
                    account_bal = string.IsNullOrEmpty(x.account_bal) ? null : x.account_bal.Trim(),
                    banktotal = string.IsNullOrEmpty(x.banktotal) ? null : x.banktotal.Trim(),
                    notcleardbanktotal = string.IsNullOrEmpty(x.notcleardbanktotal) ? null : x.notcleardbanktotal.Trim(),
                    projectid = Convert.ToInt32(x.projectId) > 0 ? Convert.ToInt32(x.projectId) : 0,
                    LastBalance = Convert.ToInt32(x.LastBalance) > 0 ? Convert.ToInt32(x.LastBalance) : 0,
                    unclearFunds = Convert.ToInt32(x.unclearFunds) > 0 ? Convert.ToInt32(x.unclearFunds) : 0,
                    netBalance = Convert.ToInt32(x.unclearFunds) > 0 ? Convert.ToInt32(x.unclearFunds) : 0,
                    balAvailable = Convert.ToInt32(x.balAvailable) > 0 ? Convert.ToInt32(x.balAvailable) : 0,
                    holdAmount = Convert.ToInt32(x.holdAmount) > 0 ? Convert.ToInt32(x.holdAmount) : 0,
                    overdraft = Convert.ToInt32(x.overdraft) > 0 ? Convert.ToInt32(x.overdraft) : 0,
                    customerName = string.IsNullOrWhiteSpace(x.customerName) ? null : x.customerName.Trim(),
                    LastTransactionDatetime = string.IsNullOrWhiteSpace(x.LastTransactionDatetime) ? null : x.LastTransactionDatetime.Trim(),

                }).ToList();

                responseObject.Status = "Success";
                responseObject.Message = " API Get SuccessFully";
                responseObject.Data = result;
            }
            else
            {
                Console.WriteLine("❌ API Failed");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine(response.Content);
                responseObject.Status = "Error";
                responseObject.Message = $"Status: {response.StatusCode}, Content: {response.Content}";
                responseObject.Data = null;
            }
            return responseObject;
        }

        public async Task<CommonResponseObject> TriggerBankDetail_By_SusidiaryidAsync_L3(int Subsidiaryid)
        {
            var responseObject = new CommonResponseObject();
            //string consumerKey = ConfigurationManager.AppSettings["consumer_key"];
            //string consumerSecret = ConfigurationManager.AppSettings["consumer_secret"];
            //string accessToken = ConfigurationManager.AppSettings["access_token"];
            //string tokenSecret = ConfigurationManager.AppSettings["token_secret"];
            //string realm = ConfigurationManager.AppSettings["Realm"];
            //string url = ConfigurationManager.AppSettings["Url"];

            string consumerKey = GlobalNetSuiteConfig.Settings.consumer_key;    //ConfigurationManager.AppSettings["consumer_key"];
            string consumerSecret = GlobalNetSuiteConfig.Settings.consumer_secret;
            string accessToken = GlobalNetSuiteConfig.Settings.access_token;
            string tokenSecret = GlobalNetSuiteConfig.Settings.token_secret;
            string realm = GlobalNetSuiteConfig.Settings.Realm;
            string url = GlobalNetSuiteConfig.Settings.ApiBaseUrl;

            var authenticator = OAuth1Authenticator.ForAccessToken(
                consumerKey,
                consumerSecret,
                accessToken,
                tokenSecret,
                OAuthSignatureMethod.HmacSha256
            );

            // ✅ Realm goes in Authorization (NOT header)
            authenticator.Realm = realm;

            var client = new RestClient(new RestClientOptions
            {
                Authenticator = authenticator,
                ThrowOnAnyError = false
            });

            var request = new RestRequest(url, Method.Post);

            // ✅ Same headers as curl
            request.AddHeader("Prefer", "transient");
            request.AddHeader("Content-Type", "application/json");

            string payload = GetBankAccountDetailbySubsidiaryid_Query_L3(Subsidiaryid);
            request.AddStringBody(payload, DataFormat.Json);
            Console.WriteLine(payload);
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                Console.WriteLine("✅ API Success");
                Console.WriteLine(response.Content);
                var nsResponse = JsonConvert.DeserializeObject<SubsidiaryNetSuiteQlResponse>(response.Content);
                //var result = nsResponse.items.Select(x => new BankDetails_By_Subsidiary
                //{
                //    Subsidiary = x.Subsidiary,
                //    Project = string.IsNullOrWhiteSpace(x.Project) ? null : x.Project.Trim(),
                //    AccountType = string.IsNullOrWhiteSpace(x.AccountType) ? null : x.AccountType.Trim(),
                //    Account_Bal = string.IsNullOrWhiteSpace(x.Account_Bal.ToString()) ? null : x.Account_Bal,
                //    DisplayNameWithHierarchy = string.IsNullOrWhiteSpace(x.DisplayNameWithHierarchy) ? null : x.DisplayNameWithHierarchy.Trim(),
                //    LastRecoDate = string.IsNullOrWhiteSpace(x.LastRecoDate) ? null : x.LastRecoDate.Trim(),
                //    SubId = int.Parse(x.SubId.ToString()),
                //    Closing_Balance_As_Per_Bank_Statement = ParseNullableDecimal(x.Closing_Balance_As_Per_Bank_Statement.ToString()),
                //    Current_Account_Balance_As_Per_Bank_Book = ParseNullableDecimal(x.Current_Account_Balance_As_Per_Bank_Book.ToString()),
                //    //banktotal = ParseNullableDecimal(x.banktotal),
                //    //notcleardbanktotal = ParseNullableDecimal(x.notcleardbanktotal),

                //}).ToList();
                var result = nsResponse.items.Select(x => new BankDetails_By_Subsidiary
                {
                    Subsidiary = x.Subsidiary,
                    Project = string.IsNullOrWhiteSpace(x.custrecordbank_project) ? null : x.custrecordbank_project.Trim(),
                    AccountType = string.IsNullOrWhiteSpace(x.AccountType) ? null : x.AccountType.Trim(),
                    Account_Bal = string.IsNullOrEmpty(x.Account_Bal) ? null : x.Account_Bal.Trim(),   //ParseNullableDecimal(x.Account_Bal),
                    DisplayNameWithHierarchy = string.IsNullOrWhiteSpace(x.DisplayNameWithHierarchy) ? null : x.DisplayNameWithHierarchy.Trim(),
                    LastRecoDate = string.IsNullOrWhiteSpace(x.LastRecoDate) ? null : x.LastRecoDate.Trim(),
                    SubId = ParseNullableInt(x.SubId),
                    Closing_Bal = string.IsNullOrEmpty(x.Closing_Bal) ? null : x.Closing_Bal,
                    Closing_Balance_As_Per_Bank_Statement = string.IsNullOrEmpty(x.Closing_Balance_As_Per_Bank_Statement) ? null : x.Closing_Balance_As_Per_Bank_Statement.Trim(),
                    Current_Account_Balance_As_Per_Bank_Book = string.IsNullOrEmpty(x.Current_Account_Balance_As_Per_Bank_Book) ? null : x.Current_Account_Balance_As_Per_Bank_Book.Trim(),
                    Custrecord_Htl_Bank_Account_Number = string.IsNullOrEmpty(x.Custrecord_Htl_Bank_Account_Number) ? null : x.Custrecord_Htl_Bank_Account_Number.Trim(),
                    projectid = Convert.ToInt32(x.projectid) > 0 ? Convert.ToInt32(x.projectid) : 0,
                    LastBalance = Convert.ToInt32(x.LastBalance) > 0 ? Convert.ToInt32(x.LastBalance) : 0,
                    netBalance = Convert.ToInt32(x.netBalance) > 0 ? Convert.ToInt32(x.netBalance) : 0,
                    unclearFunds = Convert.ToInt32(x.unclearFunds) > 0 ? Convert.ToInt32(x.unclearFunds) : 0,
                    balAvailable = Convert.ToInt32(x.balAvailable) > 0 ? Convert.ToInt32(x.balAvailable) : 0,
                    holdAmount = Convert.ToInt32(x.holdAmount) > 0 ? Convert.ToInt32(x.holdAmount) : 0,
                    overdraft = Convert.ToInt32(x.overdraft) > 0 ? Convert.ToInt32(x.overdraft) : 0,
                    customerName = string.IsNullOrEmpty(x.customerName) ? null : x.customerName,
                    LastTransactionDatetime = string.IsNullOrEmpty(x.LastTransactionDatetime) ? null : x.LastTransactionDatetime,

                }).ToList();

                responseObject.Status = "Success";
                responseObject.Message = " API Get SuccessFully";
                responseObject.Data = result;
            }
            else
            {
                Console.WriteLine("❌ API Failed");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine(response.Content);
                responseObject.Status = "Error";
                responseObject.Message = $"Status: {response.StatusCode}, Content: {response.Content}";
                responseObject.Data = null;
            }
            return responseObject;
        }

        public async Task<CommonResponseObject> TriggerBank_Acc_Subsidiary_Summ_NSAsync_L1()
        {
            var responseObject = new CommonResponseObject();
            //string consumerKey = ConfigurationManager.AppSettings["consumer_key"];
            //string consumerSecret = ConfigurationManager.AppSettings["consumer_secret"];
            //string accessToken = ConfigurationManager.AppSettings["access_token"];
            //string tokenSecret = ConfigurationManager.AppSettings["token_secret"];
            //string realm = ConfigurationManager.AppSettings["Realm"];
            //string url = ConfigurationManager.AppSettings["Url"];
            string consumerKey = GlobalNetSuiteConfig.Settings.consumer_key;    //ConfigurationManager.AppSettings["consumer_key"];
            string consumerSecret = GlobalNetSuiteConfig.Settings.consumer_secret;
            string accessToken = GlobalNetSuiteConfig.Settings.access_token;
            string tokenSecret = GlobalNetSuiteConfig.Settings.token_secret;
            string realm = GlobalNetSuiteConfig.Settings.Realm;
            string url = GlobalNetSuiteConfig.Settings.ApiBaseUrl;

            var authenticator = OAuth1Authenticator.ForAccessToken(
                consumerKey,
                consumerSecret,
                accessToken,
                tokenSecret,
                OAuthSignatureMethod.HmacSha256
            );

            // ✅ Realm goes in Authorization (NOT header)
            authenticator.Realm = realm;

            var client = new RestClient(new RestClientOptions
            {
                Authenticator = authenticator,
                ThrowOnAnyError = false
            });

            var request = new RestRequest(url, Method.Post);

            // ✅ Same headers as curl
            request.AddHeader("Prefer", "transient");
            request.AddHeader("Content-Type", "application/json");

            string payload = GetBank_Acc_Subsidiary_Summ_NSQuery_L1();
            request.AddStringBody(payload, DataFormat.Json);
            Console.WriteLine(payload);
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                Console.WriteLine("✅ API Success");
                Console.WriteLine(response.Content);
                var nsResponse = JsonConvert.DeserializeObject<Bank_Acc_Subsidiary_Summ_NSResponse>(response.Content);
                //var result = nsResponse.items.Select(x => new BankDetails_By_Subsidiary
                //{
                //    Subsidiary = x.Subsidiary,
                //    Project = string.IsNullOrWhiteSpace(x.Project) ? null : x.Project.Trim(),
                //    AccountType = string.IsNullOrWhiteSpace(x.AccountType) ? null : x.AccountType.Trim(),
                //    Account_Bal = string.IsNullOrWhiteSpace(x.Account_Bal.ToString()) ? null : x.Account_Bal,
                //    DisplayNameWithHierarchy = string.IsNullOrWhiteSpace(x.DisplayNameWithHierarchy) ? null : x.DisplayNameWithHierarchy.Trim(),
                //    LastRecoDate = string.IsNullOrWhiteSpace(x.LastRecoDate) ? null : x.LastRecoDate.Trim(),
                //    SubId = int.Parse(x.SubId.ToString()),
                //    Closing_Balance_As_Per_Bank_Statement = ParseNullableDecimal(x.Closing_Balance_As_Per_Bank_Statement.ToString()),
                //    Current_Account_Balance_As_Per_Bank_Book = ParseNullableDecimal(x.Current_Account_Balance_As_Per_Bank_Book.ToString()),
                //    //banktotal = ParseNullableDecimal(x.banktotal),
                //    //notcleardbanktotal = ParseNullableDecimal(x.notcleardbanktotal),

                //}).ToList();
                var result = nsResponse.items.Select(x => new Bank_Acc_Subsidiary_Summ_NS
                {
                    subsidiary = x.Subsidiary,
                    subid = Convert.ToInt32(x.SubId),
                    // Closing_Bal = string.IsNullOrEmpty(x.Closing_Bal) ? null : x.Closing_Bal,
                    // closing_balance_as_per_bank_statement = string.IsNullOrEmpty(x.closing_balance_as_per_bank_statement) ? null : x.closing_balance_as_per_bank_statement.Trim(),
                    current_account_balance_as_per_bank_book = Convert.ToDecimal(x.Current_Account_Balance_As_Per_Bank_Book) > 0 ? Convert.ToDecimal(x.Current_Account_Balance_As_Per_Bank_Book) : 0,
                    closing_balance_as_per_bank_statement = Convert.ToDecimal(x.Closing_Balance_As_Per_Bank_Statement) > 0 ? Convert.ToDecimal(x.Closing_Balance_As_Per_Bank_Statement) : 0,
                    LastBalance = Convert.ToDecimal(x.LastBalance) > 0 ? Convert.ToDecimal(x.LastBalance) : 0,
                    unclearFunds = Convert.ToDecimal(x.unclearFunds) > 0 ? Convert.ToDecimal(x.unclearFunds) : 0,
                    netBalance = Convert.ToDecimal(x.netBalance) > 0 ? Convert.ToDecimal(x.netBalance) : 0,
                    balAvailable = Convert.ToDecimal(x.balAvailable) > 0 ? Convert.ToDecimal(x.balAvailable) : 0,
                    holdAmount = Convert.ToDecimal(x.holdAmount) > 0 ? Convert.ToDecimal(x.holdAmount) : 0,
                    overdraft = Convert.ToDecimal(x.overdraft) > 0 ? Convert.ToDecimal(x.overdraft) : 0,
                    customerName = string.IsNullOrEmpty(x.customerName) ? null : x.customerName,
                    LastTransactionDatetime = string.IsNullOrEmpty(x.LastTransactionDatetime) ? null : x.LastTransactionDatetime,
                }).ToList();

                responseObject.Status = "Success";
                responseObject.Message = " API Get SuccessFully";
                responseObject.Data = result;
            }
            else
            {
                Console.WriteLine("❌ API Failed");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine(response.Content);
                responseObject.Status = "Error";
                responseObject.Message = $"Status: {response.StatusCode}, Content: {response.Content}";
                responseObject.Data = null;
            }
            return responseObject;
        }

        private int? ParseNullableInt(object value)
        {
            if (value == null) return null;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return null;
        }

        public async Task<ProcessResponse> ProcessBankAccSubsidiarySummaryMakeHistoryAsync_L2(decimal? userId, int? businessGrp)
        {

            string responseMessage = string.Empty;
            try
            {
                using (var connection = _dapperDbConnection.CreateConnection())
                {
                     connection.Open();

                    var parameters = new DynamicParameters();
                    parameters.Add("@SessionUserId", userId);
                    parameters.Add("@BusinessGroupId", businessGrp);
                    parameters.Add("@responseMessage", dbType: DbType.String,
                                  direction: ParameterDirection.Output,
                                   size: 500);

                    await connection.ExecuteAsync(
                        "dbo.Sp_Make_History",
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 300 // ✅ IMPORTANT

                    );
                    responseMessage = parameters.Get<string>("@responseMessage");
                    if (responseMessage.Contains("Success"))
                    {
                        return new ProcessResponse()
                        {
                            IsSuccess = true,
                            Message = "Success"

                        };
                    }
                    else
                    {
                        return new ProcessResponse()
                        {
                            IsSuccess = false,
                            Message = "No rows inserted"

                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ProcessResponse
                {
                    IsSuccess = false,
                    Message = "Error: " + ex.Message
                };
            }
        }

        public async Task<ProcessResponse> ProcessBankAccSubsidiarySummaryDeleteDetailsAsync(decimal? userId, int? businessGrp)
        {
            try
            {
                using (var connection = _dapperDbConnection.CreateConnection())
                {
                     connection.Open();

                    var parameters = new DynamicParameters();
                    parameters.Add("@SessionUserId", userId?.ToString());
                    parameters.Add("@BusinessGroupId", businessGrp?.ToString());
                    parameters.Add("@responseMessage",
                                   dbType: DbType.String,
                                   direction: ParameterDirection.Output,
                                   size: 250);

                    await connection.ExecuteAsync(
                        "dbo.Sp_DeleteBank_Acc_Summ_ByDeleteFlag",
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 300 // ✅ IMPORTANT
                    );

                    var responseMessage = parameters.Get<string>("@responseMessage");

                    return new ProcessResponse
                    {
                        IsSuccess = responseMessage == "Success",
                        Message = responseMessage
                    };
                }
            }
            catch (Exception ex)
            {
                return new ProcessResponse
                {
                    IsSuccess = false,
                    Message = "Error: " + ex.Message
                };
            }
        }


        public async Task<InsertResponse> AddBank_Acc_Summ_Async_L2(Bank_Acc_Summ_NS log ,string userid)
        {
            try
            {
                string strUserId = userid;   //ConfigurationManager.AppSettings["AdminUserId"];
                int userId = Convert.ToInt32(strUserId);

                using (var connection = _dapperDbConnection.CreateConnection()) // ✅ Use global connection helper
                {
                    connection.Open();

                    string storedProcedureName = "[dbo].[Sp_Insert_BankAcc_Summ_NS]";
                    var parameters = new DynamicParameters();

                    //parameters.Add("@pStatus", string.IsNullOrEmpty(log.Status) ? null : log.Status);
                    // parameters.Add("@pMessage", string.IsNullOrEmpty(log.Message) ? null : log.Message);
                    //parameters.Add("@pActionName", string.IsNullOrEmpty(log.ActionName) ? null : log.ActionName);
                    // parameters.Add("@pMethodName", string.IsNullOrEmpty(log.MethodName) ? null : log.MethodName);
                    parameters.Add("@subsidiary", string.IsNullOrEmpty(log.Subsidiary) ? null : log.Subsidiary);
                    parameters.Add("@custrecord_htl_bank_account_number", string.IsNullOrEmpty(log.custrecord_htl_bank_account_number) ? null : log.custrecord_htl_bank_account_number);
                    parameters.Add("@description", string.IsNullOrEmpty(log.description) ? null : log.description);
                    parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@Attribute1", string.IsNullOrEmpty(log.Attribute1) ? null : log.Attribute1);
                    parameters.Add("@Attribute2", string.IsNullOrEmpty(log.Attribute2) ? null : log.Attribute2);
                    parameters.Add("@Attribute3", string.IsNullOrEmpty(log.Attribute3) ? null : log.Attribute3);
                    parameters.Add("@Attribute4", string.IsNullOrEmpty(log.Attribute4) ? null : log.Attribute4);
                    parameters.Add("@Attribute5", string.IsNullOrEmpty(log.Attribute5) ? null : log.Attribute5);
                    parameters.Add("@CREATION_DATE", DateTime.UtcNow);
                    parameters.Add("@CreatedBy", log.CreatedBy > 0 ? log.CreatedBy : 0);
                    parameters.Add("@LastUpdatedBy", log.LastUpdatedBy > 0 ? log.LastUpdatedBy : 0);
                    parameters.Add("@LastUpdateDate", log.LastUpdateDate);
                    parameters.Add("@DeleteFlag", string.IsNullOrEmpty(log.DeleteFlag) ? "N" : log.DeleteFlag);
                    parameters.Add("@account_bal", string.IsNullOrEmpty(log.account_bal) ? null : log.account_bal);
                    parameters.Add("@accounttype", string.IsNullOrEmpty(log.accounttype) ? null : log.accounttype);
                    // parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@notcleardbanktotal", string.IsNullOrEmpty(log.notcleardbanktotal) ? null : log.notcleardbanktotal);
                    parameters.Add("@banktotal", string.IsNullOrEmpty(log.banktotal) ? null : log.banktotal);   // log.banktotal > 0 ? log.banktotal : 0
                    parameters.Add("@subid", log.SubId > 0 ? log.SubId : 0);
                    parameters.Add("@Project", string.IsNullOrEmpty(log.Project) ? null : log.Project);
                    parameters.Add("@projectid", log.projectid > 0 ? log.projectid : 0);
                    parameters.Add("@Closing_Balance_As_Per_Bank_Statement", string.IsNullOrEmpty(log.Closing_Balance_As_Per_Bank_Statement) ? null : log.Closing_Balance_As_Per_Bank_Statement);
                    parameters.Add("@Current_Account_Balance_As_Per_Bank_Book", string.IsNullOrEmpty(log.Current_Account_Balance_As_Per_Bank_Book) ? null : log.Current_Account_Balance_As_Per_Bank_Book);
                    parameters.Add("@CreatedByName", string.IsNullOrEmpty(log.CreatedByName) ? null : log.CreatedByName);
                    parameters.Add("@LastUpdatedByName", string.IsNullOrEmpty(log.LastUpdatedByName) ? null : log.LastUpdatedByName);
                    parameters.Add("@LastBalance", log.LastBalance > 0 ? log.LastBalance : 0);
                    parameters.Add("@unclearFunds", log.unclearFunds > 0 ? log.unclearFunds : 0);
                    parameters.Add("@netBalance", log.netBalance > 0 ? log.netBalance : 0);
                    parameters.Add("@balAvailable", log.balAvailable > 0 ? log.balAvailable : 0);
                    parameters.Add("@holdAmount", log.holdAmount > 0 ? log.holdAmount : 0);
                    parameters.Add("@overdraft", log.overdraft > 0 ? log.overdraft : 0);
                    parameters.Add("@customerName", string.IsNullOrEmpty(log.customerName) ? null : log.customerName);
                    parameters.Add("@LastTransactionDatetime", string.IsNullOrEmpty(log.LastTransactionDatetime) ? null : log.LastTransactionDatetime);

                    // Output parameter
                    parameters.Add("@Mkey", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    // Execute stored procedure using Dapper
                    await connection.ExecuteAsync(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve the output parameter

                    //string responseMessage = parameters.Get<string>("@responseMessage");

                    return new InsertResponse
                    {
                        Mkey = parameters.Get<int>("@Mkey"),
                        Message = parameters.Get<string>("@responseMessage")
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encountered: {ex.Message}. Please check the connection and data.");
                throw;
            }
        }

        public async Task<InsertBankAcc_SummResponse> AddSubSidiaryBankDetailsSummary_L3(BankDetails_By_Subsidiary log , string userid)
        {
            string responseMessage = string.Empty;
            //IDbTransaction transaction = null;
            try
            {

                string strUserId = userid; //ConfigurationManager.AppSettings["AdminUserId"];
                int userId = Convert.ToInt32(strUserId);

                using (var connection = _dapperDbConnection.CreateConnection()) // ✅ Use global connection helper
                {
                    connection.Open();
                    //transaction= connection.BeginTransaction();
                    string storedProcedureName = "[dbo].[Sp_Insert_SubSidiaryBankAccDetails_Summ_NS]";
                    var parameters = new DynamicParameters();

                    //parameters.Add("@pStatus", string.IsNullOrEmpty(log.Status) ? null : log.Status);
                    // parameters.Add("@pMessage", string.IsNullOrEmpty(log.Message) ? null : log.Message);
                    //parameters.Add("@pActionName", string.IsNullOrEmpty(log.ActionName) ? null : log.ActionName);
                    // parameters.Add("@pMethodName", string.IsNullOrEmpty(log.MethodName) ? null : log.MethodName);
                    parameters.Add("@Mkey", log.Mkey > 0 ? log.Mkey : 0);
                    //parameters.Add("@SrNo", log.SrNo > 0 ? log.SrNo : 0);
                    parameters.Add("@SubId", log.SubId > 0 ? log.SubId : 0);
                    parameters.Add("@subsidiary", string.IsNullOrEmpty(log.Subsidiary) ? null : log.Subsidiary);
                    parameters.Add("@Custrecord_Htl_Bank_Account_Number", string.IsNullOrEmpty(log.Custrecord_Htl_Bank_Account_Number) ? null : log.Custrecord_Htl_Bank_Account_Number);
                    parameters.Add("@DisplayNameWithHierarchy", string.IsNullOrEmpty(log.DisplayNameWithHierarchy) ? null : log.DisplayNameWithHierarchy);
                    parameters.Add("@Project", string.IsNullOrEmpty(log.Project) ? null : log.Project);
                    parameters.Add("@AccountType", string.IsNullOrEmpty(log.AccountType) ? null : log.AccountType);
                    parameters.Add("@Attribute1", string.IsNullOrEmpty(log.Attribute1) ? null : log.Attribute1);
                    parameters.Add("@Attribute2", string.IsNullOrEmpty(log.Attribute2) ? null : log.Attribute2);
                    parameters.Add("@Attribute3", string.IsNullOrEmpty(log.Attribute3) ? null : log.Attribute3);
                    parameters.Add("@Attribute4", string.IsNullOrEmpty(log.Attribute4) ? null : log.Attribute4);
                    parameters.Add("@Attribute5", string.IsNullOrEmpty(log.Attribute5) ? null : log.Attribute5);
                    parameters.Add("@CREATION_DATE", DateTime.UtcNow);
                    parameters.Add("@CreatedBy", log.Created_By > 0 ? log.Created_By : 0);
                    parameters.Add("@projectid", log.projectid > 0 ? log.projectid : 0);
                    parameters.Add("@LastUpdatedBy", log.Last_Updated_By > 0 ? log.Last_Updated_By : 0);
                    parameters.Add("@LastUpdateDate", log.Last_Update_Date);
                    parameters.Add("@DeleteFlag", string.IsNullOrEmpty(log.Delete_Flag) ? "N" : log.Delete_Flag);
                    parameters.Add("@Account_Bal", string.IsNullOrEmpty(log.Account_Bal) ? null : log.Account_Bal);
                    parameters.Add("@Closing_Bal", string.IsNullOrEmpty(log.Closing_Bal) ? null : log.Closing_Bal);
                    // parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@LastRecoDate", string.IsNullOrEmpty(log.LastRecoDate) ? null : log.LastRecoDate);
                    parameters.Add("@Closing_Balance_As_Per_Bank_Statement", string.IsNullOrEmpty(log.Closing_Balance_As_Per_Bank_Statement) ? null : log.Closing_Balance_As_Per_Bank_Statement);
                    parameters.Add("@Current_Account_Balance_As_Per_Bank_Book", string.IsNullOrEmpty(log.Current_Account_Balance_As_Per_Bank_Book) ? null : log.Current_Account_Balance_As_Per_Bank_Book);
                    parameters.Add("@CreatedByName", string.IsNullOrEmpty(log.Created_By_Name) ? null : log.Created_By_Name);
                    parameters.Add("@LastUpdatedByName", string.IsNullOrEmpty(log.Last_Updated_By_Name) ? null : log.Last_Updated_By_Name);
                    parameters.Add("@LastBalance", log.LastBalance > 0 ? log.LastBalance : 0);
                    parameters.Add("@unclearFunds", log.unclearFunds > 0 ? log.unclearFunds : 0);
                    parameters.Add("@netBalance", log.netBalance > 0 ? log.netBalance : 0);
                    parameters.Add("@balAvailable", log.balAvailable > 0 ? log.balAvailable : 0);
                    parameters.Add("@holdAmount", log.holdAmount > 0 ? log.holdAmount : 0);
                    parameters.Add("@overdraft", log.overdraft > 0 ? log.overdraft : 0);
                    parameters.Add("@customerName", string.IsNullOrEmpty(log.customerName) ? null : log.customerName);
                    parameters.Add("@LastTransactionDatetime", string.IsNullOrEmpty(log.LastTransactionDatetime) ? null : log.LastTransactionDatetime);
                    // Output parameter
                    parameters.Add("@responseMkey", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@responseSrNo", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    // Execute stored procedure using Dapper
                    await connection.ExecuteAsync(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve the output parameter
                    // transaction.Commit();
                    return new InsertBankAcc_SummResponse
                    {
                        Mkey = parameters.Get<int>("@responseMkey"),
                        SrNo = parameters.Get<int>("@responseSrNo"),
                        Message = parameters.Get<string>("@responseMessage")
                    };
                    //responseMessage = parameters.Get<string>("@responseMessage");

                    //return responseMessage.Contains("Success") ? "Success" : "No rows inserted";
                }
            }
            catch (Exception ex)
            {
                //if (transaction != null)
                //{
                //    transaction.Rollback();
                //}
                responseMessage = "Error" + " " + ex.Message;
                Console.WriteLine($"Error encountered: {ex.Message}. Please check the connection and data.");
                var responseMessageObj = new InsertBankAcc_SummResponse
                {
                    Mkey = 0,
                    SrNo = 0,
                    Message = responseMessage
                };
                return responseMessageObj;
                // throw;
            }
        }

        public async Task<InsertResponse> AddBank_Acc_Subsidiary_Summ_NSSummary_L1(Bank_Acc_Subsidiary_Summ_NS log , string userid)
        {
            //IDbTransaction transaction = null;
            //string responseMessage = string.Empty;
            try
            {
                string strUserId = userid; //ConfigurationManager.AppSettings["AdminUserId"];
                int userId = Convert.ToInt32(strUserId);

                using (var connection = _dapperDbConnection.CreateConnection()) // ✅ Use global connection helper
                {
                    connection.Open();

                    //transaction = connection.BeginTransaction();

                    string storedProcedureName = "[dbo].[Sp_Insert_Bank_Acc_Subsidiary_Summ_NS_L1]";
                    var parameters = new DynamicParameters();

                    //parameters.Add("@pStatus", string.IsNullOrEmpty(log.Status) ? null : log.Status);
                    // parameters.Add("@pMessage", string.IsNullOrEmpty(log.Message) ? null : log.Message);
                    //parameters.Add("@pActionName", string.IsNullOrEmpty(log.ActionName) ? null : log.ActionName);
                    // parameters.Add("@pMethodName", string.IsNullOrEmpty(log.MethodName) ? null : log.MethodName);
                    //parameters.Add("@Mkey", log.Mkey > 0 ? log.Mkey : 0);
                    //parameters.Add("@SrNo", log.SrNo > 0 ? log.SrNo : 0);
                    //parameters.Add("@SubId", log.SubId > 0 ? log.SubId : 0);
                    parameters.Add("@subsidiary", string.IsNullOrEmpty(log.subsidiary) ? null : log.subsidiary);
                    parameters.Add("@custrecord_htl_bank_account_number", string.IsNullOrEmpty(log.custrecord_htl_bank_account_number) ? null : log.custrecord_htl_bank_account_number);
                    parameters.Add("@description", string.IsNullOrEmpty(log.description) ? null : log.description);
                    parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@accounttype", string.IsNullOrEmpty(log.accounttype) ? null : log.accounttype);
                    parameters.Add("@account_bal", log.account_bal > 0 ? log.account_bal : 0);
                    parameters.Add("@banktotal", log.banktotal > 0 ? log.banktotal : 0);
                    parameters.Add("@notcleardbanktotal", log.notcleardbanktotal > 0 ? log.notcleardbanktotal : 0);
                    // parameters.Add("@banktotal", log.banktotal > 0 ? log.banktotal : 0);
                    parameters.Add("@subid", log.subid > 0 ? log.subid : 0);
                    parameters.Add("@project", string.IsNullOrEmpty(log.project) ? null : log.project);
                    parameters.Add("@closing_balance_as_per_bank_statement", log.closing_balance_as_per_bank_statement > 0 ? log.closing_balance_as_per_bank_statement : 0);
                    parameters.Add("@current_account_balance_as_per_bank_book", log.current_account_balance_as_per_bank_book > 0 ? log.current_account_balance_as_per_bank_book : 0);
                    parameters.Add("@Attribute1", string.IsNullOrEmpty(log.Attribute1) ? null : log.Attribute1);
                    parameters.Add("@Attribute2", string.IsNullOrEmpty(log.Attribute2) ? null : log.Attribute2);
                    parameters.Add("@Attribute3", string.IsNullOrEmpty(log.Attribute3) ? null : log.Attribute3);
                    parameters.Add("@Attribute4", string.IsNullOrEmpty(log.Attribute4) ? null : log.Attribute4);
                    parameters.Add("@Attribute5", string.IsNullOrEmpty(log.Attribute5) ? null : log.Attribute5);
                    parameters.Add("@CREATION_DATE", DateTime.UtcNow);
                    parameters.Add("@CreatedBy", log.Created_By > 0 ? log.Created_By : 0);
                    parameters.Add("@LastUpdatedBy", log.Last_Updated_By > 0 ? log.Last_Updated_By : 0);
                    parameters.Add("@LastUpdateDate", log.Last_Update_Date);
                    parameters.Add("@DeleteFlag", string.IsNullOrEmpty(log.Delete_Flag) ? "N" : log.Delete_Flag);
                    //parameters.Add("@Account_Bal", string.IsNullOrEmpty(log.Account_Bal) ? null : log.Account_Bal);
                    //parameters.Add("@Closing_Bal", string.IsNullOrEmpty(log.Closing_Bal) ? null : log.Closing_Bal);
                    // parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    //parameters.Add("@LastRecoDate", string.IsNullOrEmpty(log.LastRecoDate) ? null : log.LastRecoDate);
                    // parameters.Add("@Closing_Balance_As_Per_Bank_Statement", string.IsNullOrEmpty(log.Closing_Balance_As_Per_Bank_Statement) ? null : log.Closing_Balance_As_Per_Bank_Statement);
                    //parameters.Add("@Current_Account_Balance_As_Per_Bank_Book", string.IsNullOrEmpty(log.Current_Account_Balance_As_Per_Bank_Book) ? null : log.Current_Account_Balance_As_Per_Bank_Book);
                    parameters.Add("@CreatedByName", string.IsNullOrEmpty(log.Created_By_Name) ? null : log.Created_By_Name);
                    parameters.Add("@LastUpdatedByName", string.IsNullOrEmpty(log.Last_Updated_By_Name) ? null : log.Last_Updated_By_Name);
                    parameters.Add("@LastBalance", log.LastBalance > 0 ? log.LastBalance : 0);
                    parameters.Add("@unclearFunds", log.unclearFunds > 0 ? log.unclearFunds : 0);
                    parameters.Add("@netBalance", log.netBalance > 0 ? log.netBalance : 0);
                    parameters.Add("@balAvailable", log.balAvailable > 0 ? log.balAvailable : 0);
                    parameters.Add("@holdAmount", log.holdAmount > 0 ? log.holdAmount : 0);
                    parameters.Add("@overdraft", log.overdraft > 0 ? log.overdraft : 0);
                    parameters.Add("@customerName", string.IsNullOrEmpty(log.customerName) ? null : log.customerName);
                    parameters.Add("@LastTransactionDatetime", string.IsNullOrEmpty(log.LastTransactionDatetime) ? null : log.LastTransactionDatetime);
                    // Output parameter
                    parameters.Add("@Mkey", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    // Execute stored procedure using Dapper
                    await connection.ExecuteAsync(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);

                    // transaction.Commit();
                    return new InsertResponse
                    {
                        Mkey = parameters.Get<int>("@Mkey"),
                        Message = parameters.Get<string>("@responseMessage")
                    };
                    // Retrieve the output parameter
                    // responseMessage = parameters.Get<string>("@responseMessage");
                    //return responseMessage.Contains("Success") ? "Success" : "No rows inserted";
                }
            }
            catch (Exception ex)
            {
                //if (transaction != null)
                //{
                //    transaction.Rollback();
                //}
                var responseMessage = new InsertResponse
                {
                    Mkey = 0,
                    Message = "Error" + " " + ex.Message
                };
                //responseMessage = "Error" + " " + ex.Message;
                Console.WriteLine($"Error encountered: {ex.Message}. Please check the connection and data.");
                return responseMessage;
                // throw;
            }
        }

        public async Task<string> AddBank_Acc_summ_LogAsyncL2(Bank_Acc_Summ_NS_LogModel log, string userid)
        {
            try
            {
                string strUserId = userid;      //ConfigurationManager.AppSettings["AdminUserId"];
                string UserName = "Admin";   // ConfigurationManager.AppSettings["AdminUserName"];
                int userId = Convert.ToInt32(strUserId);
                log.CreatedBy = userId;
                //log.CreatedByName = UserName;
                using (var connection = _dapperDbConnection.CreateConnection()) // ✅ Use global connection helper
                {
                    connection.Open();

                    string storedProcedureName = "[dbo].[Sp_UspInsert_BankAcc_Summ_NS_Log]";
                    var parameters = new DynamicParameters();

                    parameters.Add("@Status", string.IsNullOrEmpty(log.Status) ? null : log.Status);
                    parameters.Add("@Message", string.IsNullOrEmpty(log.Message) ? null : log.Message);
                    parameters.Add("@ActionName", string.IsNullOrEmpty(log.ActionName) ? null : log.ActionName);
                    parameters.Add("@MethodName", string.IsNullOrEmpty(log.MethodName) ? null : log.MethodName);
                    parameters.Add("@Subsidiary", string.IsNullOrEmpty(log.Subsidiary) ? null : log.Subsidiary);
                    parameters.Add("@Project", string.IsNullOrEmpty(log.Project) ? null : log.Project);
                    parameters.Add("@projectid", log.projectid > 0 ? log.projectid : 0);
                    parameters.Add("@SubId", log.SubId > 0 ? log.SubId : 0);
                    parameters.Add("@Closing_Balance_As_Per_Bank_Statement", string.IsNullOrEmpty(log.Closing_Balance_As_Per_Bank_Statement) ? null : log.Closing_Balance_As_Per_Bank_Statement);
                    parameters.Add("@Current_Account_Balance_As_Per_Bank_Book", string.IsNullOrEmpty(log.Current_Account_Balance_As_Per_Bank_Book) ? null : log.Current_Account_Balance_As_Per_Bank_Book);
                    parameters.Add("@accounttype", string.IsNullOrEmpty(log.accounttype) ? null : log.accounttype);
                    parameters.Add("@custrecord_htl_bank_account_number", string.IsNullOrEmpty(log.custrecord_htl_bank_account_number) ? null : log.custrecord_htl_bank_account_number);
                    parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@lastrecodate", string.IsNullOrEmpty(log.lastrecodate) ? null : log.lastrecodate);
                    parameters.Add("@account_bal", string.IsNullOrEmpty(log.account_bal) ? null : log.account_bal);
                    parameters.Add("@banktotal", string.IsNullOrEmpty(log.banktotal) ? null : log.banktotal);
                    parameters.Add("@notcleardbanktotal", string.IsNullOrEmpty(log.notcleardbanktotal) ? null : log.notcleardbanktotal);
                    parameters.Add("@closing_bal", string.IsNullOrEmpty(log.closing_bal) ? null : log.closing_bal);
                    parameters.Add("@description", string.IsNullOrEmpty(log.description) ? null : log.description);
                    parameters.Add("@Attribute1", string.IsNullOrEmpty(log.Attribute1) ? null : log.Attribute1);
                    parameters.Add("@Attribute2", string.IsNullOrEmpty(log.Attribute2) ? null : log.Attribute2);
                    parameters.Add("@Attribute3", string.IsNullOrEmpty(log.Attribute3) ? null : log.Attribute3);
                    parameters.Add("@Attribute4", string.IsNullOrEmpty(log.Attribute4) ? null : log.Attribute4);
                    parameters.Add("@Attribute5", string.IsNullOrEmpty(log.Attribute5) ? null : log.Attribute5);
                    parameters.Add("@CreationDate", DateTime.UtcNow);
                    parameters.Add("@CreatedBy", log.CreatedBy > 0 ? log.CreatedBy : 0);
                    parameters.Add("@LastUpdatedBy", log.LastUpdatedBy > 0 ? log.LastUpdatedBy : 0);
                    parameters.Add("@LastUpdateDate", log.LastUpdateDate);
                    parameters.Add("@DeleteFlag", string.IsNullOrEmpty(log.DeleteFlag) ? "N" : log.DeleteFlag);
                    parameters.Add("@CreatedByName", string.IsNullOrEmpty(log.CreatedByName) ? null : log.CreatedByName);
                    parameters.Add("@LastUpdatedByName", string.IsNullOrEmpty(log.LastUpdatedByName) ? null : log.LastUpdatedByName);

                    // Output parameter
                    parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    // Execute stored procedure using Dapper
                    await connection.ExecuteAsync(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve the output parameter
                    string responseMessage = parameters.Get<string>("@responseMessage");

                    return responseMessage.Contains("Success") ? "Success" : "No rows inserted";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encountered: {ex.Message}. Please check the connection and data.");
                throw;
            }
        }

        public async Task<string> AddBank_Acc_SubSidiary_summ_LogAsyncL1(Bank_Acc_Subsidiary_Summ_NS_LogModel log)
        {
            try
            {
                string strUserId = "13";     //ConfigurationManager.AppSettings["AdminUserId"];
                string UserName = "Admin";           // ConfigurationManager.AppSettings["AdminUserName"];
                int userId = Convert.ToInt32(strUserId);
                log.Created_By = userId;
                log.Created_By_Name = UserName;
                using (var connection = _dapperDbConnection.CreateConnection()) // ✅ Use global connection helper
                {
                    connection.Open();

                    string storedProcedureName = "[dbo].[Sp_UspInsert_BankAcc_Summ_NS_Log]";
                    var parameters = new DynamicParameters();

                    //parameters.Add("@Status", string.IsNullOrEmpty(log.Status) ? null : log.Status);
                    //parameters.Add("@Message", string.IsNullOrEmpty(log.Message) ? null : log.Message);
                    //parameters.Add("@ActionName", string.IsNullOrEmpty(log.ActionName) ? null : log.ActionName);
                    //parameters.Add("@MethodName", string.IsNullOrEmpty(log.MethodName) ? null : log.MethodName);
                    parameters.Add("@subsidiary", string.IsNullOrEmpty(log.subsidiary) ? null : log.subsidiary);
                    parameters.Add("@project", string.IsNullOrEmpty(log.project) ? null : log.project);
                    // parameters.Add("@projectid", log.projectid > 0 ? log.proj : 0);
                    parameters.Add("@subid", log.subid > 0 ? log.subid : 0);
                    parameters.Add("@custrecord_htl_bank_account_number", string.IsNullOrEmpty(log.custrecord_htl_bank_account_number) ? null : log.custrecord_htl_bank_account_number);
                    parameters.Add("@description", string.IsNullOrEmpty(log.description) ? null : log.description);
                    parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@accounttype", string.IsNullOrEmpty(log.accounttype) ? null : log.accounttype);
                    parameters.Add("@account_bal", log.account_bal > 0 ? log.account_bal : 0);
                    parameters.Add("@banktotal", log.banktotal > 0 ? log.banktotal : 0);
                    parameters.Add("@notcleardbanktotal", log.notcleardbanktotal > 0 ? log.notcleardbanktotal : 0);
                    parameters.Add("@subid", log.subid > 0 ? log.subid : 0);
                    parameters.Add("@closing_balance_as_per_bank_statement", log.closing_balance_as_per_bank_statement > 0 ? log.closing_balance_as_per_bank_statement : 0);
                    parameters.Add("@current_account_balance_as_per_bank_book", log.current_account_balance_as_per_bank_book > 0 ? log.current_account_balance_as_per_bank_book : 0);
                    parameters.Add("@Attribute1", string.IsNullOrEmpty(log.Attribute1) ? null : log.Attribute1);
                    parameters.Add("@Attribute2", string.IsNullOrEmpty(log.Attribute2) ? null : log.Attribute2);
                    parameters.Add("@Attribute3", string.IsNullOrEmpty(log.Attribute3) ? null : log.Attribute3);
                    parameters.Add("@Attribute4", string.IsNullOrEmpty(log.Attribute4) ? null : log.Attribute4);
                    parameters.Add("@Attribute5", string.IsNullOrEmpty(log.Attribute5) ? null : log.Attribute5);
                    parameters.Add("@CreationDate", DateTime.UtcNow);
                    parameters.Add("@CreatedBy", log.Created_By > 0 ? log.Created_By : 0);
                    parameters.Add("@LastUpdatedBy", log.Last_Updated_By > 0 ? log.Last_Updated_By : 0);
                    parameters.Add("@LastUpdateDate", log.Last_Update_Date);
                    parameters.Add("@Delete_Flag", string.IsNullOrEmpty(log.Delete_Flag) ? "N" : log.Delete_Flag);
                    parameters.Add("@Created_By_Name", string.IsNullOrEmpty(log.Created_By_Name) ? null : log.Created_By_Name);
                    parameters.Add("@Last_Updated_By_Name", string.IsNullOrEmpty(log.Last_Updated_By_Name) ? null : log.Last_Updated_By_Name);
                    // Output parameter
                    parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    // Execute stored procedure using Dapper
                    await connection.ExecuteAsync(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve the output parameter
                    string responseMessage = parameters.Get<string>("@responseMessage");

                    return responseMessage.Contains("Success") ? "Success" : "No rows inserted";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encountered: {ex.Message}. Please check the connection and data.");
                throw;
            }
        }

        public async Task<string> AddSubSidairy_BankDetailsSummary_LogAsync_L3(BankDetails_By_Subsidiary_LogModel log ,string userid)
        {
            string responseMessage = string.Empty;
            try
            {
                string strUserId = userid; //ConfigurationManager.AppSettings["AdminUserId"];
                int userId = Convert.ToInt32(strUserId);

                using (var connection = _dapperDbConnection.CreateConnection()) // ✅ Use global connection helper
                {
                    connection.Open();
                    string storedProcedureName = "[dbo].[Sp_Insert_SubSidiaryBankAccDetails_Summ_NS]";
                    var parameters = new DynamicParameters();

                    parameters.Add("@Status", string.IsNullOrEmpty(log.Status) ? null : log.Status);
                    parameters.Add("@Message", string.IsNullOrEmpty(log.Message) ? null : log.Message);
                    parameters.Add("@ActionName", string.IsNullOrEmpty(log.ActionName) ? null : log.ActionName);
                    parameters.Add("@MethodName", string.IsNullOrEmpty(log.MethodName) ? null : log.MethodName);
                    parameters.Add("@Mkey", log.Mkey > 0 ? log.Mkey : 0);
                    parameters.Add("@SrNo", log.SrNo > 0 ? log.SrNo : 0);
                    parameters.Add("@SubId", log.SubId > 0 ? log.SubId : 0);
                    parameters.Add("@subsidiary", string.IsNullOrEmpty(log.Subsidiary) ? null : log.Subsidiary);
                    parameters.Add("@Custrecord_Htl_Bank_Account_Number", string.IsNullOrEmpty(log.Custrecord_Htl_Bank_Account_Number) ? null : log.Custrecord_Htl_Bank_Account_Number);
                    parameters.Add("@DisplayNameWithHierarchy", string.IsNullOrEmpty(log.DisplayNameWithHierarchy) ? null : log.DisplayNameWithHierarchy);
                    parameters.Add("@Project", string.IsNullOrEmpty(log.Project) ? null : log.Project);
                    parameters.Add("@AccountType", string.IsNullOrEmpty(log.AccountType) ? null : log.AccountType);
                    parameters.Add("@Attribute1", string.IsNullOrEmpty(log.Attribute1) ? null : log.Attribute1);
                    parameters.Add("@Attribute2", string.IsNullOrEmpty(log.Attribute2) ? null : log.Attribute2);
                    parameters.Add("@Attribute3", string.IsNullOrEmpty(log.Attribute3) ? null : log.Attribute3);
                    parameters.Add("@Attribute4", string.IsNullOrEmpty(log.Attribute4) ? null : log.Attribute4);
                    parameters.Add("@Attribute5", string.IsNullOrEmpty(log.Attribute5) ? null : log.Attribute5);
                    parameters.Add("@CREATION_DATE", DateTime.UtcNow);
                    parameters.Add("@CreatedBy", log.Created_By > 0 ? log.Created_By : 0);
                    parameters.Add("@projectid", log.projectid > 0 ? log.projectid : 0);
                    parameters.Add("@LastUpdatedBy", log.Last_Updated_By > 0 ? log.Last_Updated_By : 0);
                    parameters.Add("@LastUpdateDate", log.Last_Update_Date);
                    parameters.Add("@DeleteFlag", string.IsNullOrEmpty(log.Delete_Flag) ? "N" : log.Delete_Flag);
                    parameters.Add("@Account_Bal", string.IsNullOrEmpty(log.Account_Bal) ? null : log.Account_Bal);
                    parameters.Add("@Closing_Bal", string.IsNullOrEmpty(log.Closing_Bal) ? null : log.Closing_Bal);
                    // parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@LastRecoDate", string.IsNullOrEmpty(log.LastRecoDate) ? null : log.LastRecoDate);
                    parameters.Add("@Closing_Balance_As_Per_Bank_Statement", string.IsNullOrEmpty(log.Closing_Balance_As_Per_Bank_Statement) ? null : log.Closing_Balance_As_Per_Bank_Statement);
                    parameters.Add("@Current_Account_Balance_As_Per_Bank_Book", string.IsNullOrEmpty(log.Current_Account_Balance_As_Per_Bank_Book) ? null : log.Current_Account_Balance_As_Per_Bank_Book);
                    parameters.Add("@CreatedByName", string.IsNullOrEmpty(log.Created_By_Name) ? null : log.Created_By_Name);
                    parameters.Add("@LastUpdatedByName", string.IsNullOrEmpty(log.Last_Updated_By_Name) ? null : log.Last_Updated_By_Name);
                    // Output parameter
                    //parameters.Add("@responseMkey", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    // parameters.Add("@responseSrNo", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    // Execute stored procedure using Dapper
                    await connection.ExecuteAsync(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);

                    parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    // Retrieve the output parameter
                    responseMessage = parameters.Get<string>("@responseMessage");
                    return responseMessage.Contains("Success") ? "Success" : "No rows inserted";
                }
            }
            catch (Exception ex)
            {
                responseMessage = "Error" + " " + ex.Message;
                Console.WriteLine($"Error encountered: {ex.Message}. Please check the connection and data.");

                return responseMessage;
                // throw;
            }
        }

        // Mapping Data for Level Two
        public async Task<Bank_Acc_Log_NS> MapBank_Acc_Log_NS_Model_L3(BankDetails_By_Subsidiary_LogModel bankAccSubsidiarySummNS)
        {
            var logModel = new Bank_Acc_Log_NS
            {
                Status = bankAccSubsidiarySummNS.Status,
                Message = bankAccSubsidiarySummNS.Message,
                ActionName = bankAccSubsidiarySummNS.ActionName,
                MethodName = bankAccSubsidiarySummNS.MethodName,
                subsidiary = bankAccSubsidiarySummNS.Subsidiary,
                custrecord_htl_bank_account_number = bankAccSubsidiarySummNS.Custrecord_Htl_Bank_Account_Number,
                // description = bankAccSubsidiarySummNS.des,
                displaynamewithhierarchy = bankAccSubsidiarySummNS.DisplayNameWithHierarchy,
                accounttype = bankAccSubsidiarySummNS.AccountType,
                account_bal = Convert.ToDecimal(bankAccSubsidiarySummNS.Account_Bal),
                LastRecoDate = bankAccSubsidiarySummNS.LastRecoDate,
                //notcleardbanktotal = bankAccSubsidiarySummNS.n,
                subid = bankAccSubsidiarySummNS.SubId,
                project = bankAccSubsidiarySummNS.Project,
                closing_balance_as_per_bank_statement = Convert.ToDecimal(bankAccSubsidiarySummNS.Closing_Balance_As_Per_Bank_Statement),
                current_account_balance_as_per_bank_book = Convert.ToDecimal(bankAccSubsidiarySummNS.Current_Account_Balance_As_Per_Bank_Book),
                ATTRIBUTE1 = bankAccSubsidiarySummNS.Attribute1,
                ATTRIBUTE2 = bankAccSubsidiarySummNS.Attribute2,
                ATTRIBUTE3 = bankAccSubsidiarySummNS.Attribute3,
                ATTRIBUTE4 = bankAccSubsidiarySummNS.Attribute4,
                ATTRIBUTE5 = bankAccSubsidiarySummNS.Attribute5,
                CREATED_BY = bankAccSubsidiarySummNS.Created_By,
                CREATION_DATE = DateTime.UtcNow,
                LAST_UPDATED_BY = bankAccSubsidiarySummNS.Last_Updated_By,
                LAST_UPDATED_BY_Name = bankAccSubsidiarySummNS.Last_Updated_By_Name,
                CREATED_BY_Name = bankAccSubsidiarySummNS.Created_By_Name,
                LAST_UPDATE_DATE = bankAccSubsidiarySummNS.Last_Update_Date,
                DELETE_FLAG = bankAccSubsidiarySummNS.Delete_Flag,
                LastBalance = bankAccSubsidiarySummNS.LastBalance,
                unclearFunds = bankAccSubsidiarySummNS.unclearFunds,
                netBalance = bankAccSubsidiarySummNS.netBalance,
                balAvailable = bankAccSubsidiarySummNS.balAvailable,
                holdAmount = bankAccSubsidiarySummNS.holdAmount,
                customerName = bankAccSubsidiarySummNS.customerName,
                overdraft = bankAccSubsidiarySummNS.overdraft,
                LastTransactionDatetime = bankAccSubsidiarySummNS.LastTransactionDatetime

            };
            return logModel;
        }


        public async Task<Bank_Acc_Log_NS> MapBank_Acc_Log_NS_Model_L2(Bank_Acc_Summ_NS_LogModel bankAccSubsidiarySummNS)
        {
            var logModel = new Bank_Acc_Log_NS
            {
                Status = bankAccSubsidiarySummNS.Status,
                Message = bankAccSubsidiarySummNS.Message,
                ActionName = bankAccSubsidiarySummNS.ActionName,
                MethodName = bankAccSubsidiarySummNS.MethodName,
                subsidiary = bankAccSubsidiarySummNS.Subsidiary,
                custrecord_htl_bank_account_number = bankAccSubsidiarySummNS.custrecord_htl_bank_account_number,
                description = bankAccSubsidiarySummNS.description,
                displaynamewithhierarchy = bankAccSubsidiarySummNS.displaynamewithhierarchy,
                accounttype = bankAccSubsidiarySummNS.accounttype,
                account_bal = Convert.ToDecimal(bankAccSubsidiarySummNS.account_bal),
                LastRecoDate = bankAccSubsidiarySummNS.lastrecodate,
                notcleardbanktotal = Convert.ToDecimal(bankAccSubsidiarySummNS.notcleardbanktotal),
                subid = bankAccSubsidiarySummNS.SubId,
                project = bankAccSubsidiarySummNS.Project,
                banktotal = Convert.ToDecimal(bankAccSubsidiarySummNS.banktotal),
                projectid = bankAccSubsidiarySummNS.projectid.ToString(),
                closing_balance_as_per_bank_statement = Convert.ToDecimal(bankAccSubsidiarySummNS.Closing_Balance_As_Per_Bank_Statement),
                current_account_balance_as_per_bank_book = Convert.ToDecimal(bankAccSubsidiarySummNS.Current_Account_Balance_As_Per_Bank_Book),
                ATTRIBUTE1 = bankAccSubsidiarySummNS.Attribute1,
                ATTRIBUTE2 = bankAccSubsidiarySummNS.Attribute2,
                ATTRIBUTE3 = bankAccSubsidiarySummNS.Attribute3,
                ATTRIBUTE4 = bankAccSubsidiarySummNS.Attribute4,
                ATTRIBUTE5 = bankAccSubsidiarySummNS.Attribute5,
                CREATED_BY = bankAccSubsidiarySummNS.CreatedBy,
                CREATION_DATE = DateTime.UtcNow,
                LAST_UPDATED_BY = bankAccSubsidiarySummNS.LastUpdatedBy,
                LAST_UPDATED_BY_Name = bankAccSubsidiarySummNS.LastUpdatedByName,
                CREATED_BY_Name = bankAccSubsidiarySummNS.CreatedByName,
                LAST_UPDATE_DATE = bankAccSubsidiarySummNS.LastUpdateDate,
                DELETE_FLAG = bankAccSubsidiarySummNS.DeleteFlag,
                LastBalance = bankAccSubsidiarySummNS.LastBalance,
                unclearFunds = bankAccSubsidiarySummNS.unclearFunds,
                netBalance = bankAccSubsidiarySummNS.netBalance,
                balAvailable = bankAccSubsidiarySummNS.balAvailable,
                holdAmount = bankAccSubsidiarySummNS.holdAmount,
                customerName = bankAccSubsidiarySummNS.customerName,
                overdraft = bankAccSubsidiarySummNS.overdraft,
                LastTransactionDatetime = bankAccSubsidiarySummNS.LastTransactionDatetime

            };
            return logModel;
        }

        public  async Task<Bank_Acc_Subsidiary_Summ_NS_LogModel> MapBank_Acc_Subsidiary_Summ_NS_ToLogModel(Bank_Acc_Subsidiary_Summ_NS bankAccSubsidiarySummNS)
        {
            var logModel = new Bank_Acc_Subsidiary_Summ_NS_LogModel
            {
                //Status = status,
                //Message = message,
                //ActionName = actionName,
                //MethodName = methodName,
                subsidiary = bankAccSubsidiarySummNS.subsidiary,
                custrecord_htl_bank_account_number = bankAccSubsidiarySummNS.custrecord_htl_bank_account_number,
                description = bankAccSubsidiarySummNS.description,
                displaynamewithhierarchy = bankAccSubsidiarySummNS.displaynamewithhierarchy,
                accounttype = bankAccSubsidiarySummNS.accounttype,
                account_bal = bankAccSubsidiarySummNS.account_bal,
                banktotal = bankAccSubsidiarySummNS.banktotal,
                notcleardbanktotal = bankAccSubsidiarySummNS.notcleardbanktotal,
                subid = bankAccSubsidiarySummNS.subid,
                project = bankAccSubsidiarySummNS.project,
                closing_balance_as_per_bank_statement = bankAccSubsidiarySummNS.closing_balance_as_per_bank_statement,
                current_account_balance_as_per_bank_book = bankAccSubsidiarySummNS.current_account_balance_as_per_bank_book,
                Attribute1 = bankAccSubsidiarySummNS.Attribute1,
                Attribute2 = bankAccSubsidiarySummNS.Attribute2,
                Attribute3 = bankAccSubsidiarySummNS.Attribute3,
                Attribute4 = bankAccSubsidiarySummNS.Attribute4,
                Attribute5 = bankAccSubsidiarySummNS.Attribute5,
                Created_By = bankAccSubsidiarySummNS.Created_By,
                Creation_Date = DateTime.UtcNow,
                Last_Updated_By = bankAccSubsidiarySummNS.Last_Updated_By,
                Last_Updated_By_Name = bankAccSubsidiarySummNS.Last_Updated_By_Name,
                Created_By_Name = bankAccSubsidiarySummNS.Created_By_Name,
                Last_Update_Date = bankAccSubsidiarySummNS.Last_Update_Date,
                LastBalance = bankAccSubsidiarySummNS.LastBalance,
                unclearFunds = bankAccSubsidiarySummNS.unclearFunds,
                netBalance = bankAccSubsidiarySummNS.netBalance,
                balAvailable = bankAccSubsidiarySummNS.balAvailable,
                holdAmount = bankAccSubsidiarySummNS.holdAmount,
                customerName = bankAccSubsidiarySummNS.customerName,
                overdraft = bankAccSubsidiarySummNS.overdraft,
                LastTransactionDatetime = bankAccSubsidiarySummNS.LastTransactionDatetime

            };
            return logModel;
        }

        public async Task<Bank_Acc_Log_NS> MapBank_Acc_Log_NS_Model(Bank_Acc_Subsidiary_Summ_NS_LogModel bankAccSubsidiarySummNS)
        {
            var logModel = new Bank_Acc_Log_NS
            {
                Status = bankAccSubsidiarySummNS.Status,
                Message = bankAccSubsidiarySummNS.Message,
                ActionName = bankAccSubsidiarySummNS.ActionName,
                MethodName = bankAccSubsidiarySummNS.MethodName,
                subsidiary = bankAccSubsidiarySummNS.subsidiary,
                custrecord_htl_bank_account_number = bankAccSubsidiarySummNS.custrecord_htl_bank_account_number,
                description = bankAccSubsidiarySummNS.description,
                displaynamewithhierarchy = bankAccSubsidiarySummNS.displaynamewithhierarchy,
                accounttype = bankAccSubsidiarySummNS.accounttype,
                account_bal = bankAccSubsidiarySummNS.account_bal,
                banktotal = bankAccSubsidiarySummNS.banktotal,
                notcleardbanktotal = bankAccSubsidiarySummNS.notcleardbanktotal,
                subid = bankAccSubsidiarySummNS.subid,
                project = bankAccSubsidiarySummNS.project,
                closing_balance_as_per_bank_statement = bankAccSubsidiarySummNS.closing_balance_as_per_bank_statement,
                current_account_balance_as_per_bank_book = bankAccSubsidiarySummNS.current_account_balance_as_per_bank_book,
                ATTRIBUTE1 = bankAccSubsidiarySummNS.Attribute1,
                ATTRIBUTE2 = bankAccSubsidiarySummNS.Attribute2,
                ATTRIBUTE3 = bankAccSubsidiarySummNS.Attribute3,
                ATTRIBUTE4 = bankAccSubsidiarySummNS.Attribute4,
                ATTRIBUTE5 = bankAccSubsidiarySummNS.Attribute5,
                CREATED_BY = bankAccSubsidiarySummNS.Created_By,
                CREATION_DATE = DateTime.UtcNow,
                LAST_UPDATED_BY = bankAccSubsidiarySummNS.Last_Updated_By,
                LAST_UPDATED_BY_Name = bankAccSubsidiarySummNS.Last_Updated_By_Name,
                CREATED_BY_Name = bankAccSubsidiarySummNS.Created_By_Name,
                LAST_UPDATE_DATE = bankAccSubsidiarySummNS.Last_Update_Date,
                DELETE_FLAG = bankAccSubsidiarySummNS.Delete_Flag,
                LastBalance = bankAccSubsidiarySummNS.LastBalance,
                unclearFunds = bankAccSubsidiarySummNS.unclearFunds,
                netBalance = bankAccSubsidiarySummNS.netBalance,
                balAvailable = bankAccSubsidiarySummNS.balAvailable,
                holdAmount = bankAccSubsidiarySummNS.holdAmount,
                customerName = bankAccSubsidiarySummNS.customerName,
                overdraft = bankAccSubsidiarySummNS.overdraft,
                LastTransactionDatetime = bankAccSubsidiarySummNS.LastTransactionDatetime

            };
            return logModel;
        }


        public  async Task<string> AddBank_Acc_Log_NS_Async(Bank_Acc_Log_NS log ,string userid)
        {
            try
            {
                // Get Admin user details
                string strUserId = userid; //ConfigurationManager.AppSettings["AdminUserId"];
                //string userName = "Admin";    //ConfigurationManager.AppSettings["AdminUserName"];
                int userId = Convert.ToInt32(strUserId);

                log.CREATED_BY = userId;
               // log.CREATED_BY_Name = userName;
                log.DELETE_FLAG = string.IsNullOrEmpty(log.DELETE_FLAG) ? "N" : log.DELETE_FLAG;

                using (var connection = _dapperDbConnection.CreateConnection()) // ✅ Global connection helper
                {
                     connection.Open();

                    string storedProcedureName = "[dbo].[SP_Insert_Bank_Acc_Log_NS]";
                    var parameters = new DynamicParameters();

                    // parameters.Add("@Mkey", log.Mkey);
                    parameters.Add("@Status", string.IsNullOrEmpty(log.Status) ? null : log.Status);
                    parameters.Add("@Message", string.IsNullOrEmpty(log.Message) ? null : log.Message);
                    parameters.Add("@ActionName", string.IsNullOrEmpty(log.ActionName) ? null : log.ActionName);
                    parameters.Add("@MethodName", string.IsNullOrEmpty(log.MethodName) ? null : log.MethodName);

                    parameters.Add("@subsidiary", string.IsNullOrEmpty(log.subsidiary) ? null : log.subsidiary);
                    parameters.Add("@custrecord_htl_bank_account_number",
                                   string.IsNullOrEmpty(log.custrecord_htl_bank_account_number) ? null : log.custrecord_htl_bank_account_number);
                    parameters.Add("@description", string.IsNullOrEmpty(log.description) ? null : log.description);
                    parameters.Add("@displaynamewithhierarchy",
                                   string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@accounttype", string.IsNullOrEmpty(log.accounttype) ? null : log.accounttype);

                    parameters.Add("@account_bal", log.account_bal > 0 ? log.account_bal : 0);
                    parameters.Add("@banktotal", log.banktotal > 0 ? log.banktotal : 0);
                    parameters.Add("@notcleardbanktotal", log.notcleardbanktotal > 0 ? log.notcleardbanktotal : 0);
                    parameters.Add("@subid", log.subid > 0 ? log.subid : 0);
                    parameters.Add("@project", string.IsNullOrEmpty(log.project) ? null : log.project);
                    parameters.Add("@projectid", string.IsNullOrEmpty(log.projectid) ? null : log.projectid);

                    parameters.Add("@closing_balance_as_per_bank_statement",
                                   log.closing_balance_as_per_bank_statement > 0 ? log.closing_balance_as_per_bank_statement : 0);
                    parameters.Add("@current_account_balance_as_per_bank_book",
                                   log.current_account_balance_as_per_bank_book > 0 ? log.current_account_balance_as_per_bank_book : 0);

                    parameters.Add("@ATTRIBUTE1", string.IsNullOrEmpty(log.ATTRIBUTE1) ? null : log.ATTRIBUTE1);
                    parameters.Add("@ATTRIBUTE2", string.IsNullOrEmpty(log.ATTRIBUTE2) ? null : log.ATTRIBUTE2);
                    parameters.Add("@ATTRIBUTE3", string.IsNullOrEmpty(log.ATTRIBUTE3) ? null : log.ATTRIBUTE3);
                    parameters.Add("@ATTRIBUTE4", string.IsNullOrEmpty(log.ATTRIBUTE4) ? null : log.ATTRIBUTE4);
                    parameters.Add("@ATTRIBUTE5", string.IsNullOrEmpty(log.ATTRIBUTE5) ? null : log.ATTRIBUTE5);
                    parameters.Add("@LastRecoDate", string.IsNullOrEmpty(log.LastRecoDate) ? null : log.LastRecoDate);
                    parameters.Add("@CREATED_BY", log.CREATED_BY);
                    parameters.Add("@CREATED_BY_Name", log.CREATED_BY_Name);
                    parameters.Add("@DELETE_FLAG", log.DELETE_FLAG);

                    parameters.Add("@LastBalance", log.LastBalance > 0 ? log.LastBalance : 0);
                    parameters.Add("@unclearFunds", log.unclearFunds > 0 ? log.unclearFunds : 0);
                    parameters.Add("@netBalance", log.netBalance > 0 ? log.netBalance : 0);
                    parameters.Add("@balAvailable", log.balAvailable > 0 ? log.balAvailable : 0);
                    parameters.Add("@holdAmount", log.holdAmount > 0 ? log.holdAmount : 0);
                    parameters.Add("@overdraft", log.overdraft > 0 ? log.overdraft : 0);
                    parameters.Add("@customerName", string.IsNullOrEmpty(log.customerName) ? null : log.customerName);
                    parameters.Add("@LastTransactionDatetime", log.LastTransactionDatetime);

                    // OUTPUT parameter
                    parameters.Add("@responseMessage",
                                   dbType: DbType.String,
                                   direction: ParameterDirection.Output,
                                   size: 500);

                    await connection.ExecuteAsync(
                        storedProcedureName,
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    string responseMessage = parameters.Get<string>("@responseMessage");

                    return responseMessage != null && responseMessage.Contains("SUCCESS")
                           ? "Success"
                           : responseMessage ?? "No rows inserted";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encountered: {ex.Message}");
                throw;
            }
        }

        public async Task<BankDetails_By_Subsidiary_LogModel> MapBank_Details_By_Subsidiary_ToLogModel(BankDetails_By_Subsidiary bankDetailsBySubsidiary)
        {
            var logModel = new BankDetails_By_Subsidiary_LogModel
            {
                Mkey = bankDetailsBySubsidiary.Mkey,
                SrNo = bankDetailsBySubsidiary.SrNo,
                SubId = bankDetailsBySubsidiary.SubId,
                Subsidiary = bankDetailsBySubsidiary.Subsidiary,
                Custrecord_Htl_Bank_Account_Number = bankDetailsBySubsidiary.Custrecord_Htl_Bank_Account_Number,
                DisplayNameWithHierarchy = bankDetailsBySubsidiary.DisplayNameWithHierarchy,
                Project = bankDetailsBySubsidiary.Project,
                projectid = bankDetailsBySubsidiary.projectid,
                Account_Bal = bankDetailsBySubsidiary.Account_Bal,
                Closing_Bal = bankDetailsBySubsidiary.Closing_Bal,
                Closing_Balance_As_Per_Bank_Statement = bankDetailsBySubsidiary.Closing_Balance_As_Per_Bank_Statement,
                Current_Account_Balance_As_Per_Bank_Book = bankDetailsBySubsidiary.Current_Account_Balance_As_Per_Bank_Book,
                LastRecoDate = bankDetailsBySubsidiary.LastRecoDate,
                AccountType = bankDetailsBySubsidiary.AccountType,
                Attribute1 = bankDetailsBySubsidiary.Attribute1,
                Attribute2 = bankDetailsBySubsidiary.Attribute2,
                Attribute3 = bankDetailsBySubsidiary.Attribute3,
                Attribute4 = bankDetailsBySubsidiary.Attribute4,
                Attribute5 = bankDetailsBySubsidiary.Attribute5,
                Created_By = bankDetailsBySubsidiary.Created_By,
                Creation_Date = DateTime.UtcNow,
                Last_Updated_By = bankDetailsBySubsidiary.Last_Updated_By,
                Last_Update_Date = bankDetailsBySubsidiary.Last_Update_Date,
                Last_Updated_By_Name = bankDetailsBySubsidiary.Last_Updated_By_Name,
                Created_By_Name = bankDetailsBySubsidiary.Created_By_Name,
                LastBalance = bankDetailsBySubsidiary.LastBalance,
                unclearFunds = bankDetailsBySubsidiary.unclearFunds,
                netBalance = bankDetailsBySubsidiary.netBalance,
                balAvailable = bankDetailsBySubsidiary.balAvailable,
                holdAmount = bankDetailsBySubsidiary.holdAmount,
                customerName = bankDetailsBySubsidiary.customerName,
                overdraft = bankDetailsBySubsidiary.overdraft,
                LastTransactionDatetime = bankDetailsBySubsidiary.LastTransactionDatetime

            };
            return logModel;
        }


        public  async Task<ProcessResponse> ProcessBankAccSubsidiarySummaryRevertHistoryAsync_L2(decimal? userId, int? businessGrp)
        {
            try
            {
                using (var connection = _dapperDbConnection.CreateConnection())
                {
                     connection.Open();

                    var parameters = new DynamicParameters();
                    parameters.Add("@SessionUserId", userId?.ToString());
                    parameters.Add("@BusinessGroupId", businessGrp?.ToString());
                    parameters.Add("@responseMessage",
                                   dbType: DbType.String,
                                   direction: ParameterDirection.Output,
                                   size: 250);

                    await connection.ExecuteAsync(
                        "dbo.Sp_Revert_History",
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 300 // ✅ IMPORTANT
                    );

                    var responseMessage = parameters.Get<string>("@responseMessage");

                    return new ProcessResponse
                    {
                        IsSuccess = responseMessage == "Success",
                        Message = responseMessage
                    };
                }
            }
            catch (Exception ex)
            {
                return new ProcessResponse
                {
                    IsSuccess = false,
                    Message = "Error: " + ex.Message
                };
            }
        }


        public  async Task<Bank_Acc_Summ_NS_LogModel> MapBank_Acc_Summ_NS_ToLogModel(Bank_Acc_Summ_NS bankAccSummNS)
        {
            var logModel = new Bank_Acc_Summ_NS_LogModel
            {
                //Status = status,
                //Message = message,
                //ActionName = actionName,
                //MethodName = methodName,
                Subsidiary = bankAccSummNS.Subsidiary,
                Project = bankAccSummNS.Project,
                projectid = bankAccSummNS.projectid,
                SubId = bankAccSummNS.SubId,
                Closing_Balance_As_Per_Bank_Statement = bankAccSummNS.Closing_Balance_As_Per_Bank_Statement,
                Current_Account_Balance_As_Per_Bank_Book = bankAccSummNS.Current_Account_Balance_As_Per_Bank_Book,
                accounttype = bankAccSummNS.accounttype,
                custrecord_htl_bank_account_number = bankAccSummNS.custrecord_htl_bank_account_number,
                displaynamewithhierarchy = bankAccSummNS.displaynamewithhierarchy,
                lastrecodate = bankAccSummNS.lastrecodate,
                account_bal = bankAccSummNS.account_bal,
                banktotal = bankAccSummNS.banktotal,
                notcleardbanktotal = bankAccSummNS.notcleardbanktotal,
                closing_bal = bankAccSummNS.closing_bal,
                description = bankAccSummNS.description,
                Attribute1 = bankAccSummNS.Attribute1,
                Attribute2 = bankAccSummNS.Attribute2,
                Attribute3 = bankAccSummNS.Attribute3,
                Attribute4 = bankAccSummNS.Attribute4,
                Attribute5 = bankAccSummNS.Attribute5,
                LastUpdateDate = DateTime.UtcNow,
                LastBalance = bankAccSummNS.LastBalance,
                unclearFunds = bankAccSummNS.unclearFunds,
                netBalance = bankAccSummNS.netBalance,
                balAvailable = bankAccSummNS.balAvailable,
                holdAmount = bankAccSummNS.holdAmount,
                customerName = bankAccSummNS.customerName,
                overdraft = bankAccSummNS.overdraft,
                LastTransactionDatetime = bankAccSummNS.LastTransactionDatetime
            };
            return logModel;
        }


        #region
        // Write Only Update Method Using Id 

        public async Task<InsertResponse> UpdateBank_Acc_Subsidiary_Summ_NsSummary_L1(Bank_Acc_Subsidiary_Summ_NS log , string userid ,string UserName)
        {
            try
            {
                string strUserId = userid; //ConfigurationManager.AppSettings["AdminUserId"];
                int userId = Convert.ToInt32(strUserId);
                log.Last_Update_Date = DateTime.UtcNow;
                log.Last_Updated_By = userId;
                log.Created_By = userId;
                log.Last_Updated_By_Name = UserName;
                using(var connection= _dapperDbConnection.CreateConnection())
                {
                    connection.Open();
                    string storedProcedureName = "[dbo].[Sp_Update_Bank_Acc_Subsidiary_Summ_NS_L1]";
                    var parameters = new DynamicParameters();
                    //parameters.Add("@pStatus", string.IsNullOrEmpty(log.Status) ? null : log.Status);
                    // parameters.Add("@pMessage", string.IsNullOrEmpty(log.Message) ? null : log.Message);
                    //parameters.Add("@pActionName", string.IsNullOrEmpty(log.ActionName) ? null : log.ActionName);
                    // parameters.Add("@pMethodName", string.IsNullOrEmpty(log.MethodName) ? null : log.MethodName);
                    //parameters.Add("@Mkey", log.Mkey > 0 ? log.Mkey : 0);
                    //parameters.Add("@SrNo", log.SrNo > 0 ? log.SrNo : 0);
                    //parameters.Add("@SubId", log.SubId > 0 ? log.SubId : 0);
                    parameters.Add("@subsidiary", string.IsNullOrEmpty(log.subsidiary) ? null : log.subsidiary);
                    parameters.Add("@custrecord_htl_bank_account_number", string.IsNullOrEmpty(log.custrecord_htl_bank_account_number) ? null : log.custrecord_htl_bank_account_number);
                    parameters.Add("@description", string.IsNullOrEmpty(log.description) ? null : log.description);
                    parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@accounttype", string.IsNullOrEmpty(log.accounttype) ? null : log.accounttype);
                    parameters.Add("@account_bal", log.account_bal > 0 ? log.account_bal : 0);
                    parameters.Add("@banktotal", log.banktotal > 0 ? log.banktotal : 0);
                    parameters.Add("@notcleardbanktotal", log.notcleardbanktotal > 0 ? log.notcleardbanktotal : 0);
                    // parameters.Add("@banktotal", log.banktotal > 0 ? log.banktotal : 0);
                    parameters.Add("@subid", log.subid > 0 ? log.subid : 0);
                    parameters.Add("@project", string.IsNullOrEmpty(log.project) ? null : log.project);
                    parameters.Add("@closing_balance_as_per_bank_statement", log.closing_balance_as_per_bank_statement > 0 ? log.closing_balance_as_per_bank_statement : 0);
                    parameters.Add("@current_account_balance_as_per_bank_book", log.current_account_balance_as_per_bank_book > 0 ? log.current_account_balance_as_per_bank_book : 0);
                    parameters.Add("@Attribute1", string.IsNullOrEmpty(log.Attribute1) ? null : log.Attribute1);
                    parameters.Add("@Attribute2", string.IsNullOrEmpty(log.Attribute2) ? null : log.Attribute2);
                    parameters.Add("@Attribute3", string.IsNullOrEmpty(log.Attribute3) ? null : log.Attribute3);
                    parameters.Add("@Attribute4", string.IsNullOrEmpty(log.Attribute4) ? null : log.Attribute4);
                    parameters.Add("@Attribute5", string.IsNullOrEmpty(log.Attribute5) ? null : log.Attribute5);
                    parameters.Add("@CREATION_DATE", DateTime.UtcNow);
                    parameters.Add("@CreatedBy", log.Created_By > 0 ? log.Created_By : 0);
                    parameters.Add("@LastUpdatedBy", log.Last_Updated_By > 0 ? log.Last_Updated_By : 0);
                   // parameters.Add("@LastUpdateDate", log.Last_Update_Date);
                    parameters.Add("@DeleteFlag", string.IsNullOrEmpty(log.Delete_Flag) ? "N" : log.Delete_Flag);
                    //parameters.Add("@Account_Bal", string.IsNullOrEmpty(log.Account_Bal) ? null : log.Account_Bal);
                    //parameters.Add("@Closing_Bal", string.IsNullOrEmpty(log.Closing_Bal) ? null : log.Closing_Bal);
                    // parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    //parameters.Add("@LastRecoDate", string.IsNullOrEmpty(log.LastRecoDate) ? null : log.LastRecoDate);
                    // parameters.Add("@Closing_Balance_As_Per_Bank_Statement", string.IsNullOrEmpty(log.Closing_Balance_As_Per_Bank_Statement) ? null : log.Closing_Balance_As_Per_Bank_Statement);
                    //parameters.Add("@Current_Account_Balance_As_Per_Bank_Book", string.IsNullOrEmpty(log.Current_Account_Balance_As_Per_Bank_Book) ? null : log.Current_Account_Balance_As_Per_Bank_Book);
                    parameters.Add("@CreatedByName", string.IsNullOrEmpty(log.Created_By_Name) ? null : log.Created_By_Name);
                    parameters.Add("@LastUpdatedByName", string.IsNullOrEmpty(log.Last_Updated_By_Name) ? null : log.Last_Updated_By_Name);
                    parameters.Add("@LastBalance", log.LastBalance > 0 ? log.LastBalance : 0);
                    parameters.Add("@unclearFunds", log.unclearFunds > 0 ? log.unclearFunds : 0);
                    parameters.Add("@netBalance", log.netBalance > 0 ? log.netBalance : 0);
                    parameters.Add("@balAvailable", log.balAvailable > 0 ? log.balAvailable : 0);
                    parameters.Add("@holdAmount", log.holdAmount > 0 ? log.holdAmount : 0);
                    parameters.Add("@overdraft", log.overdraft > 0 ? log.overdraft : 0);
                    parameters.Add("@customerName", string.IsNullOrEmpty(log.customerName) ? null : log.customerName);
                    parameters.Add("@LastTransactionDatetime", string.IsNullOrEmpty(log.LastTransactionDatetime) ? null : log.LastTransactionDatetime);
                    // Output parameter
                    parameters.Add("@Mkey", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    // Execute stored procedure using Dapper
                    await connection.ExecuteAsync(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);

                    // transaction.Commit();
                    return new InsertResponse
                    {
                        Mkey = parameters.Get<int>("@Mkey"),
                        Message = parameters.Get<string>("@responseMessage")
                    };
                    // Retrieve the output parameter
                    // responseMessage = parameters.Get<string>("@responseMessage");
                    //return responseMessage.Contains("Success") ? "Success" : "No rows inserted";

                }

            }
            catch (Exception ex)
            {
                //if (transaction != null)
                //{
                //    transaction.Rollback();
                //}
                var responseMessage = new InsertResponse
                {
                    Mkey = 0,
                    Message = "Error" + " " + ex.Message
                };
                //responseMessage = "Error" + " " + ex.Message;
                Console.WriteLine($"Error encountered: {ex.Message}. Please check the connection and data.");
                return responseMessage;
                // throw;

            }
        }


        public async Task<InsertBankAcc_SummResponse> Update_SubSidiaryBankDetailsSummary_L3(BankDetails_By_Subsidiary log, string userid , string UserName)
        {
            try
            {
                int userId = Convert.ToInt32(userid);
                log.Last_Update_Date = DateTime.UtcNow;
                log.Last_Updated_By = userId;
                log.Created_By = userId;
                log.Last_Updated_By_Name = UserName;
                log.Attribute6 = Convert.ToString(log.Mkey);
                using (var connection = _dapperDbConnection.CreateConnection())
                {
                    connection.Open();

                    var parameters = new DynamicParameters();

                    // ========================
                    // INPUT PARAMETERS
                    // ========================
                    parameters.Add("@subsidiary", log.Subsidiary);
                    parameters.Add("@custrecord_htl_bank_account_number", log.Custrecord_Htl_Bank_Account_Number);
                   // parameters.Add("@description", log.Description);
                    parameters.Add("@displaynamewithhierarchy", log.DisplayNameWithHierarchy);
                    parameters.Add("@accounttype", log.AccountType);

                    parameters.Add("@account_bal", log.Account_Bal);
                    parameters.Add("@Closing_Bal", log.Closing_Bal);
                    //parameters.Add("@notcleardbanktotal", log.NotClearedBankTotal);

                    parameters.Add("@subid", log.SubId);
                    parameters.Add("@projectId", log.projectid);
                    parameters.Add("@project", log.Project);

                    parameters.Add("@closing_balance_as_per_bank_statement", log.Closing_Balance_As_Per_Bank_Statement);
                    parameters.Add("@current_account_balance_as_per_bank_book", log.Current_Account_Balance_As_Per_Bank_Book);

                    parameters.Add("@Attribute1", log.Attribute1);
                    parameters.Add("@Attribute2", log.Attribute2);
                    parameters.Add("@Attribute3", log.Attribute3);
                    parameters.Add("@Attribute4", log.Attribute4);
                    parameters.Add("@Attribute5", log.Attribute5);
                    parameters.Add("@Attribute6", log.Attribute6);

                    parameters.Add("@CREATION_DATE", DateTime.UtcNow);
                    parameters.Add("@CreatedBy", userId);
                    parameters.Add("@LastUpdatedBy", userId);
                    //parameters.Add("@LastUpdateDate", DateTime.UtcNow);

                    parameters.Add("@DeleteFlag", string.IsNullOrEmpty(log.Delete_Flag) ? "N" : log.Delete_Flag);

                    parameters.Add("@CreatedByName", log.Created_By_Name);
                    parameters.Add("@LastUpdatedByName", log.Last_Updated_By_Name);

                    parameters.Add("@LastBalance", log.LastBalance);
                    parameters.Add("@unclearFunds", log.unclearFunds);
                    parameters.Add("@netBalance", log.netBalance);
                    parameters.Add("@balAvailable", log.balAvailable);
                    parameters.Add("@holdAmount", log.holdAmount);
                    parameters.Add("@overdraft", log.overdraft);

                    parameters.Add("@customerName", log.customerName);
                    parameters.Add("@LastTransactionDatetime", log.LastTransactionDatetime);

                    // ========================
                    // OUTPUT PARAMETERS
                    // ========================
                    parameters.Add("@Mkey", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@SrNo", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@responseMessage", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                    await connection.ExecuteAsync(
                        "[dbo].[Sp_Update_Bank_Acc_Subsidiary_Summ_NS_L3]",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    return new InsertBankAcc_SummResponse
                    {
                        Mkey = parameters.Get<int>("@Mkey"),
                        SrNo = parameters.Get<int>("@SrNo"),
                        Message = parameters.Get<string>("@responseMessage")
                    };
                }
            }
            catch (Exception ex)
            {
                return new InsertBankAcc_SummResponse
                {
                    Mkey = 0,
                    SrNo = 0,
                    Message = "Error : " + ex.Message
                };
            }
        }


        public async Task<InsertResponse> Update_Bank_Acc_Summ_Async_L2(Bank_Acc_Summ_NS log, string userid ,string UserName)
        {
            try
            {
                string strUserId = userid;   //ConfigurationManager.AppSettings["AdminUserId"];
                int userId = Convert.ToInt32(strUserId);
                log.LastUpdateDate = DateTime.UtcNow;
                log.LastUpdatedBy = userId;
                log.CreatedBy = userId;
                log.LastUpdatedByName = UserName;
                using (var connection = _dapperDbConnection.CreateConnection()) // ✅ Use global connection helper
                {
                    connection.Open();

                    string storedProcedureName = "[dbo].[Sp_Update_BankAcc_Summ_NS_L2]";
                    var parameters = new DynamicParameters();

                    //parameters.Add("@pStatus", string.IsNullOrEmpty(log.Status) ? null : log.Status);
                    // parameters.Add("@pMessage", string.IsNullOrEmpty(log.Message) ? null : log.Message);
                    //parameters.Add("@pActionName", string.IsNullOrEmpty(log.ActionName) ? null : log.ActionName);
                    // parameters.Add("@pMethodName", string.IsNullOrEmpty(log.MethodName) ? null : log.MethodName);
                    parameters.Add("@subsidiary", string.IsNullOrEmpty(log.Subsidiary) ? null : log.Subsidiary);
                    parameters.Add("@custrecord_htl_bank_account_number", string.IsNullOrEmpty(log.custrecord_htl_bank_account_number) ? null : log.custrecord_htl_bank_account_number);
                    parameters.Add("@description", string.IsNullOrEmpty(log.description) ? null : log.description);
                    parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@Attribute1", string.IsNullOrEmpty(log.Attribute1) ? null : log.Attribute1);
                    parameters.Add("@Attribute2", string.IsNullOrEmpty(log.Attribute2) ? null : log.Attribute2);
                    parameters.Add("@Attribute3", string.IsNullOrEmpty(log.Attribute3) ? null : log.Attribute3);
                    parameters.Add("@Attribute4", string.IsNullOrEmpty(log.Attribute4) ? null : log.Attribute4);
                    parameters.Add("@Attribute5", string.IsNullOrEmpty(log.Attribute5) ? null : log.Attribute5);
                    parameters.Add("@CREATION_DATE", DateTime.UtcNow);
                    parameters.Add("@CreatedBy", log.CreatedBy > 0 ? log.CreatedBy : 0);
                    parameters.Add("@LastUpdatedBy", log.LastUpdatedBy > 0 ? log.LastUpdatedBy : 0);
                    parameters.Add("@LastUpdateDate", log.LastUpdateDate);
                    parameters.Add("@DeleteFlag", string.IsNullOrEmpty(log.DeleteFlag) ? "N" : log.DeleteFlag);
                    parameters.Add("@account_bal", string.IsNullOrEmpty(log.account_bal) ? null : log.account_bal);
                    parameters.Add("@accounttype", string.IsNullOrEmpty(log.accounttype) ? null : log.accounttype);
                    // parameters.Add("@displaynamewithhierarchy", string.IsNullOrEmpty(log.displaynamewithhierarchy) ? null : log.displaynamewithhierarchy);
                    parameters.Add("@notcleardbanktotal", string.IsNullOrEmpty(log.notcleardbanktotal) ? null : log.notcleardbanktotal);
                    parameters.Add("@banktotal", string.IsNullOrEmpty(log.banktotal) ? null : log.banktotal);   // log.banktotal > 0 ? log.banktotal : 0
                    parameters.Add("@subid", log.SubId > 0 ? log.SubId : 0);
                    parameters.Add("@Project", string.IsNullOrEmpty(log.Project) ? null : log.Project);
                    parameters.Add("@projectid", log.projectid > 0 ? log.projectid : 0);
                    parameters.Add("@Closing_Balance_As_Per_Bank_Statement", string.IsNullOrEmpty(log.Closing_Balance_As_Per_Bank_Statement) ? null : log.Closing_Balance_As_Per_Bank_Statement);
                    parameters.Add("@Current_Account_Balance_As_Per_Bank_Book", string.IsNullOrEmpty(log.Current_Account_Balance_As_Per_Bank_Book) ? null : log.Current_Account_Balance_As_Per_Bank_Book);
                    parameters.Add("@CreatedByName", string.IsNullOrEmpty(log.CreatedByName) ? null : log.CreatedByName);
                    parameters.Add("@LastUpdatedByName", string.IsNullOrEmpty(log.LastUpdatedByName) ? null : log.LastUpdatedByName);
                    parameters.Add("@LastBalance", log.LastBalance > 0 ? log.LastBalance : 0);
                    parameters.Add("@unclearFunds", log.unclearFunds > 0 ? log.unclearFunds : 0);
                    parameters.Add("@netBalance", log.netBalance > 0 ? log.netBalance : 0);
                    parameters.Add("@balAvailable", log.balAvailable > 0 ? log.balAvailable : 0);
                    parameters.Add("@holdAmount", log.holdAmount > 0 ? log.holdAmount : 0);
                    parameters.Add("@overdraft", log.overdraft > 0 ? log.overdraft : 0);
                    parameters.Add("@customerName", string.IsNullOrEmpty(log.customerName) ? null : log.customerName);
                    parameters.Add("@LastTransactionDatetime", string.IsNullOrEmpty(log.LastTransactionDatetime) ? null : log.LastTransactionDatetime);

                    // Output parameter
                    parameters.Add("@Mkey", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    // Execute stored procedure using Dapper
                    await connection.ExecuteAsync(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve the output parameter

                    //string responseMessage = parameters.Get<string>("@responseMessage");

                    return new InsertResponse
                    {
                        Mkey = parameters.Get<int>("@Mkey"),
                        Message = parameters.Get<string>("@responseMessage")
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encountered: {ex.Message}. Please check the connection and data.");
                throw;
            }
        }


        public async Task<SubIdValidationResponse> CheckSubIdExistAsync(int subId)
        {
            try
            {
                using (var connection = _dapperDbConnection.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@SubId", subId, DbType.Int32);

                    var result = await connection.QueryFirstOrDefaultAsync<SubIdValidationResponse>(
                        "Sp_Check_SubId_Exist",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return result ?? new SubIdValidationResponse
                    {
                        StatusCode = 0,
                        Message = "No response from database"
                    };
                }
            }
            catch (Exception ex)
            {
                return new SubIdValidationResponse
                {
                    StatusCode = 0,
                    Message = "Error : " + ex.Message
                };
            }
        }

        #endregion
        #region

        // Start Bank Api Method Here To get the details by using Method 

        public async Task<Bank_ApprovalConfig> GetBank_ApprovalConfig(string keytype)
        {
            Bank_ApprovalConfig config = null;
            try
            {
                using (var connection = _dapperDbConnection.CreateConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM ConfigDetails WHERE keytype = @keytype";
                    var response = await connection.QueryFirstOrDefaultAsync<Bank_ApprovalConfig>(query, new { keytype });
                    //using (SqlCommand command = new SqlCommand(query, connection))
                    //{
                    //    command.Parameters.AddWithValue("@keytype", keytype);
                    //    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    //    {
                    //        if (await reader.ReadAsync())
                    //        {
                    //            config = new Bank_ApprovalConfig
                    //            {
                    //                Mkey = reader.GetInt32(reader.GetOrdinal("Mkey")),
                    //                KeyType = reader.IsDBNull(reader.GetOrdinal("KeyType")) ? null : reader.GetString(reader.GetOrdinal("KeyType")),
                    //                AsymKey = reader.IsDBNull(reader.GetOrdinal("AsymKey")) ? null : reader.GetString(reader.GetOrdinal("AsymKey")),
                    //                PubKey = reader.IsDBNull(reader.GetOrdinal("PubKey")) ? null : reader.GetString(reader.GetOrdinal("PubKey")),
                    //                PvtKey = reader.IsDBNull(reader.GetOrdinal("PvtKey")) ? null : reader.GetString(reader.GetOrdinal("PvtKey")),
                    //                clientid = reader.IsDBNull(reader.GetOrdinal("clientid")) ? null : reader.GetString(reader.GetOrdinal("clientid")),
                    //                clientsecret = reader.IsDBNull(reader.GetOrdinal("clientsecret")) ? null : reader.GetString(reader.GetOrdinal("clientsecret")),
                    //                clientcertificate = reader.IsDBNull(reader.GetOrdinal("clientcertificate")) ? null : reader.GetString(reader.GetOrdinal("clientcertificate")),
                    //                apiinteractionid = reader.IsDBNull(reader.GetOrdinal("apiinteractionid")) ? null : reader.GetString(reader.GetOrdinal("apiinteractionid")),
                    //                CallURL = reader.IsDBNull(reader.GetOrdinal("CallURL")) ? null : reader.GetString(reader.GetOrdinal("CallURL")),
                    //                Attribute1 = reader.IsDBNull(reader.GetOrdinal("ATTRIBUTE1")) ? null : reader.GetString(reader.GetOrdinal("ATTRIBUTE1")),
                    //                Attribute2 = reader.IsDBNull(reader.GetOrdinal("ATTRIBUTE2")) ? null : reader.GetString(reader.GetOrdinal("ATTRIBUTE2")),
                    //                Attribute3 = reader.IsDBNull(reader.GetOrdinal("ATTRIBUTE3")) ? null : reader.GetString(reader.GetOrdinal("ATTRIBUTE3")),
                    //                Attribute4 = reader.IsDBNull(reader.GetOrdinal("ATTRIBUTE4")) ? null : reader.GetString(reader.GetOrdinal("ATTRIBUTE4")),
                    //                Attribute5 = reader.IsDBNull(reader.GetOrdinal("ATTRIBUTE5")) ? null : reader.GetString(reader.GetOrdinal("ATTRIBUTE5")),
                    //                CreatedBy = reader.GetDecimal(reader.GetOrdinal("CREATED_BY")),
                    //                CreatedByName = reader.IsDBNull(reader.GetOrdinal("CREATED_BY_Name")) ? null : reader.GetString(reader.GetOrdinal("CREATED_BY_Name")),
                    //                CreationDate = reader.GetDateTime(reader.GetOrdinal("CREATION_DATE")),
                    //                LastUpdatedBy = reader.IsDBNull(reader.GetOrdinal("LAST_UPDATED_BY")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("LAST_UPDATED_BY")),
                    //                LastUpdatedByName = reader.IsDBNull(reader.GetOrdinal("LAST_UPDATED_BY_Name")) ? null : reader.GetString(reader.GetOrdinal("LAST_UPDATED_BY_Name")),
                    //                LastUpdateDate = reader.IsDBNull(reader.GetOrdinal("LAST_UPDATE_DATE")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LAST_UPDATE_DATE")),
                    //                DeleteFlag = reader.GetString(reader.GetOrdinal("DELETE_FLAG"))[0]
                    //            };
                    //        }
                    //    }
                    //}

                    return response;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error encountered: {ex.Message}");
                throw;
            }
        }

        public  async Task<List<BankAccDetails>> GetBankAccDetailsAsync(string accountNo)
        {
            //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = _dapperDbConnection.CreateConnection())
            {
                try
                {
                     connection.Open();
                    var parameter = new DynamicParameters();
                    parameter.Add("@AccountNo", accountNo);
                    var result = await connection.QueryAsync<BankAccDetails>("Get_Active_Bank_Acc_Details",parameter ,commandType: CommandType.StoredProcedure);

                    return result.AsList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching data: " + ex.ToString());
                    return null;
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                }
            }
        }

        public async Task<string> GenerateCustomerRequestJson(BankAccDetails bankAccDetails)
        {
            try
            {
                var model = new CustomerRequestModel
                {
                    Authorization = "Basic" + " " + bankAccDetails.AuthorizationVal,
                    AccountNumber = bankAccDetails.AcctNumber,
                    CustomerID = bankAccDetails.CustomerID,
                    Key = ""
                };
                var option = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                return System.Text.Json.JsonSerializer.Serialize(model, option);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return null;

            }

        }

        public  async Task<AccountBalanceDetails> GetAccountBalanceDetailsByMkeyAsync(int mkey)
        {
            AccountBalanceDetails result = null;
            //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = _dapperDbConnection.CreateConnection())
            {
                try
                {
                     connection.Open();

                    string query = @"SELECT * FROM [dbo].[Bank_Acc_Response_Details] WHERE Mkey = @Mkey";

                    result = await connection.QueryFirstOrDefaultAsync<AccountBalanceDetails>(query, new { Mkey = mkey });
                }
                catch (Exception ex)
                {
                    // Log the error if needed
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }

            return result;
        }

        public async Task<string> CallBankApiWithRetryAsync(Bank_ApprovalConfig requestModel, string jsonPayload, string Signature)
        {
            HttpResponseMessage response = null;
            int maxRetries = 2;
            string lastErrorMessage = string.Empty;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"Attempt {attempt}: Calling API");

                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestModel.CallURL);
                    httpRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    if (!requestModel.CallURL.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("The API call must be made over HTTPS.");

                    // Add headers
                    httpRequest.Headers.Add("x-client-id", requestModel.clientid);
                    httpRequest.Headers.Add("x-client-secret", requestModel.clientsecret);
                    httpRequest.Headers.Add("x-client-certificate", requestModel.clientcertificate);
                    httpRequest.Headers.Add("x-api-interaction-id", requestModel.apiinteractionid);
                    httpRequest.Headers.Add("x-signature", Signature);
                    httpRequest.Headers.Add("x-forwarded-for", "222");
                    httpRequest.Headers.Add("x-timestamp", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString());

                    using (var client = new HttpClient())
                    {
                        response = await client.SendAsync(httpRequest);
                    }

                    // If we get a response, return the content (even if it's an error)
                    if (response != null)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Response received: {response.StatusCode}");
                        return responseContent;
                    }
                }
                catch (Exception ex)
                {
                    lastErrorMessage = $"Attempt {attempt} failed: {ex.Message}";
                    Console.WriteLine(lastErrorMessage);
                }

                await System.Threading.Tasks.Task.Delay(1000); // Optional wait between retries
            }

            // Return exception message or fallback if no response at all
            return !string.IsNullOrEmpty(lastErrorMessage)
                ? (lastErrorMessage + "No response received from API")
                : "No response received from API.";
        }

        public byte[] digest(string asymkey)
        {
            int numChars = asymkey.Length;
            byte[] bytes = new byte[numChars / 2];

            for (int i = 0; i < numChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(asymkey.Substring(i, 2), 16);
            }

            return bytes;
        }

       public string SignData(string data, string privateKeyPEM)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (var reader = new StringReader(privateKeyPEM))
            {
                var pemReader = new PemReader(reader);
                //var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();


                var privateKeyParams = (RsaPrivateCrtKeyParameters)pemReader.ReadObject();
                var rsaParameters = DotNetUtilities.ToRSAParameters(privateKeyParams);


                //var rsaParameters = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaParameters);

                    // Use SHA-256 as the hash algorithm
                    using (var sha256 = SHA256.Create())
                    {
                        byte[] signature = rsa.SignData(dataBytes, sha256);
                        return Convert.ToBase64String(signature);
                    }
                }
            }
        }

       public  bool VerifyData(string data, string signature, string publicKeyPEM)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            using (var reader = new StringReader(publicKeyPEM))
            {
                var pemReader = new PemReader(reader);
                var publicKey = pemReader.ReadObject() as RsaKeyParameters;

                if (publicKey == null)
                {
                    // Handle the case where publicKey is null
                    throw new InvalidOperationException("Public key is null or invalid.");
                }

                var rsaParameters = DotNetUtilities.ToRSAParameters(publicKey);
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaParameters);

                    // Use SHA-256 as the hash algorithm
                    using (var sha256 = SHA256.Create())
                    {
                        return rsa.VerifyData(dataBytes, sha256, signatureBytes);
                    }
                }
            }
        }

        public async Task<int> InsertAccountBalanceDetailsAsync(string acctNumber, string customerID ,string Creadtedby , string createdbyName)
        {
            //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            using (var connection = _dapperDbConnection.CreateConnection() )
            {
                try
                {
                     connection.Open();

                    var parameters = new DynamicParameters();
                    parameters.Add("@acctNumber", acctNumber, DbType.String);
                    parameters.Add("@customerID", customerID, DbType.String);
                    parameters.Add("@CREATED_BY", Creadtedby, DbType.String);
                    parameters.Add("@CREATED_BY_Name", createdbyName, DbType.String);
                    parameters.Add("@Mkey", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await connection.ExecuteAsync("Insert_Account_Balance_Details", parameters, commandType: CommandType.StoredProcedure);

                    int insertedMkey = parameters.Get<int>("@Mkey");

                    return insertedMkey;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error inserting data: " + ex.Message);
                    return -1; // or throw if you prefer to handle it outside
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                    {
                        connection.Close();
                    }
                }
            }
        }

        public async Task<Bank_Acc_Response_Details?> GetBanKAccountDetails(int mkey)
        {
            try
            {
                using (var connection = _dapperDbConnection.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Mkey", mkey, DbType.Int32);

                    var result = await connection.QueryFirstOrDefaultAsync<Bank_Acc_Response_Details>(
                        "usp_Get_Bank_Acc_Response_ByMkey",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return result; // can be null if no record found
                }
            }
            catch (Exception ex)
            {
                // log exception properly
                Console.WriteLine("Error fetching data: " + ex.Message);
                return null; // or throw;
            }
        }


        public async Task<string> GenerateRequestJsonSignature(BankAccDetails bankAccDetails, string encrypt)
        {
            try
            {

                var model = new RootRequestModelPlain
                {
                    Request = new RequestModelPlain
                    {
                        body = new BodyModelPlain
                        {
                            branchCode = bankAccDetails.BranchCode,
                            encryptData = new CustomerRequestModel
                            {
                                Authorization = "Basic" + " " + bankAccDetails.AuthorizationVal,
                                AccountNumber = bankAccDetails.AcctNumber.Trim(),
                                CustomerID = bankAccDetails.CustomerID.Trim(),
                                Key = ""

                            }
                        }
                    }
                };

                var option = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase

                };
                return System.Text.Json.JsonSerializer.Serialize(model, option);

                //string jsonBody = JsonConvert.SerializeObject(model, option);

                //return jsonBody;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return null;

            }
        }


        public async Task<string> GenerateRequestJson(BankAccDetails bankAccDetails, string encrypt)
        {
            try
            {
                var model = new RootRequestModel
                {
                    Request = new RequestModel
                    {
                        body = new BodyModel
                        {
                            branchCode = bankAccDetails.BranchCode,
                            encryptData = encrypt
                            //new CustomerRequestModel
                            //{
                            //    Authorization = "Basic" + bankAccDetails.AuthorizationVal,
                            //    AccountNumber = bankAccDetails.AcctNumber,
                            //    CustomerID = bankAccDetails.CustomerID,
                            //    Key = ""
                            //}
                        }
                    }
                };

                string jsonBody = JsonConvert.SerializeObject(model);

                return jsonBody;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return null;

            }
        }

        public async Task<string> UpdateAccountBalanceDetailsAsync(AccountBalanceDetails model)
        {
            //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string responseMessage = "Unhandled error occurred";
            var connection = _dapperDbConnection.CreateConnection();
            
            try
            {
                 connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@pMkey", model.Mkey);
                parameters.Add("@pacctNumber", model.acctNumber);
                parameters.Add("@pcustomerID", model.customerID);
                parameters.Add("@pkeyVal", model.keyVal);
                parameters.Add("@pResponseData", model.ResponseData);

                // New fields added
                parameters.Add("@presult", model.result);
                parameters.Add("@pcurrency", model.currency);
                parameters.Add("@pBankStatus", model.BankStatus);
                parameters.Add("@prelationship", model.relationship);
                parameters.Add("@pchequeBookFacility", model.chequeBookFacility);
                parameters.Add("@pminBalance", model.minBalance);
                parameters.Add("@pdateString", model.dateString);
                parameters.Add("@pbranchCode", model.branchCode);
                parameters.Add("@pacctTypeCode", model.acctTypeCode);
                parameters.Add("@pcurrencyDesc", model.currencyDesc);
                parameters.Add("@pcurrencyCode", model.currencyCode);
                parameters.Add("@paccountType", model.accountType);
                // Existing fields 
                parameters.Add("@pcurrentBalance", model.currentBalance);
                parameters.Add("@punclearFunds", model.unclearFunds);
                parameters.Add("@pnetBalance", model.netBalance);
                parameters.Add("@pbalAvailable", model.balAvailable);
                parameters.Add("@pholdAmount", model.holdAmount);
                parameters.Add("@poverdraft", model.overdraft);
                parameters.Add("@pcustomerName", model.customerName);
                parameters.Add("@pResponseTime", model.ResponseTime);
                parameters.Add("@pStatus", model.Status);
                parameters.Add("@pATTRIBUTE1", model.ATTRIBUTE1);
                parameters.Add("@pATTRIBUTE2", model.ATTRIBUTE2);
                parameters.Add("@pATTRIBUTE3", model.ATTRIBUTE3);
                parameters.Add("@pATTRIBUTE4", model.ATTRIBUTE4);
                parameters.Add("@pATTRIBUTE5", model.ATTRIBUTE5);
                parameters.Add("@pCREATED_BY", model.CREATED_BY);
                parameters.Add("@pCREATED_BY_Name", model.CREATED_BY_Name);
                parameters.Add("@pCREATION_DATE", model.CREATION_DATE);
                parameters.Add("@pLAST_UPDATED_BY", model.LAST_UPDATED_BY);
                parameters.Add("@pLAST_UPDATED_BY_Name", model.LAST_UPDATED_BY_Name);
                parameters.Add("@pLAST_UPDATE_DATE", model.LAST_UPDATE_DATE);
                parameters.Add("@pDELETE_FLAG", model.DELETE_FLAG);
                parameters.Add("@responseMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("Update_AccountBalanceDetails", parameters, commandType: CommandType.StoredProcedure);

                responseMessage = parameters.Get<string>("@responseMessage");
            }
            catch (Exception ex)
            {
                responseMessage = "Error: " + ex.Message;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return responseMessage;
        }

        //public string UpdateBankDetailsHdr(string lastBalance, DateTime lastTxnDate, string accountNo, string unclearFunds, string netBalance, string balAvailable, string holdAmount, string overdraft, string customerName)
        //{
        //    string responseMessage = string.Empty;
        //    Console.WriteLine("Start UpdateBankDetailsHdr Method");
        //    //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        //    using (var conn = _dapperDbConnection.CreateConnection())
        //    using (SqlCommand cmd = new SqlCommand("Sp_UpdateBank_Details_Hdr", conn))
        //    {
        //        cmd.CommandType = CommandType.StoredProcedure;

        //        cmd.Parameters.AddWithValue("@LastBalance", lastBalance);
        //        cmd.Parameters.AddWithValue("@LastTransactionDatetime", lastTxnDate);
        //        cmd.Parameters.AddWithValue("@AccountNo", accountNo);
        //        cmd.Parameters.AddWithValue("@unclearFunds", unclearFunds);
        //        cmd.Parameters.AddWithValue("@netBalance", netBalance);
        //        cmd.Parameters.AddWithValue("@balAvailable", balAvailable);
        //        cmd.Parameters.AddWithValue("@holdAmount", holdAmount);
        //        cmd.Parameters.AddWithValue("@overdraft", overdraft);
        //        cmd.Parameters.AddWithValue("@customerName", customerName);
        //        // OUTPUT parameter
        //        SqlParameter outputParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 100)
        //        {
        //            Direction = ParameterDirection.Output
        //        };
        //        cmd.Parameters.Add(outputParam);

        //        conn.Open();
        //        cmd.ExecuteNonQuery();

        //        responseMessage = outputParam.Value?.ToString();
        //    }
        //    Console.WriteLine("Completed UpdateBankDetailsHdr Method");
        //    return responseMessage;
        //}


        public string UpdateBankDetailsHdr(string lastBalance,DateTime lastTxnDate,string accountNo,string unclearFunds,string netBalance,string balAvailable,string holdAmount,string overdraft,string customerName)
        {
            Console.WriteLine("Start UpdateBankDetailsHdr Method");
            using (var conn = _dapperDbConnection.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@LastBalance", lastBalance, DbType.String);
                parameters.Add("@LastTransactionDatetime", lastTxnDate, DbType.DateTime);
                parameters.Add("@AccountNo", accountNo, DbType.String);
                parameters.Add("@unclearFunds", unclearFunds, DbType.String);
                parameters.Add("@netBalance", netBalance, DbType.String);
                parameters.Add("@balAvailable", balAvailable, DbType.String);
                parameters.Add("@holdAmount", holdAmount, DbType.String);
                parameters.Add("@overdraft", overdraft, DbType.String);
                parameters.Add("@customerName", customerName, DbType.String);

                // OUTPUT parameter
                parameters.Add("@ResponseMessage",dbType: DbType.String,size: 100,direction: ParameterDirection.Output);
                conn.Execute("Sp_UpdateBank_Details_Hdr",parameters,commandType: CommandType.StoredProcedure);
                Console.WriteLine("Completed UpdateBankDetailsHdr Method");
                return parameters.Get<string>("@ResponseMessage");
            }
        }

        #endregion

        #region
        public async Task<string> InsertBankAccResponseLogAsync(Bank_Acc_Response_log_Model model ,string userid)
        {
            string responseMessage;
            // Get Admin user details
            string strUserId = userid; //ConfigurationManager.AppSettings["AdminUserId"];
                                       //string userName = "Admin";    //ConfigurationManager.AppSettings["AdminUserName"];
            int userId = Convert.ToInt32(strUserId);

            model.CreatedBy = userId;
            model.CreationDate = DateTime.UtcNow;                                                                                                                                                                                                                                                                                                                                                                               
            // log.CREATED_BY_Name = userName;
            model.DeleteFlag = string.IsNullOrEmpty(model.DeleteFlag.ToString()) ? 'N' : model.DeleteFlag;
            try
            {
                using (var connection = _dapperDbConnection.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    // 🔹 Input parameters
                    parameters.Add("@Status", model.Status);
                    parameters.Add("@Message", model.Message);
                    parameters.Add("@ActionName", model.ActionName);
                    parameters.Add("@MethodName", model.MethodName);
                    parameters.Add("@acctNumber", model.AcctNumber);
                    parameters.Add("@customerID", model.CustomerID);
                    parameters.Add("@keyVal", model.KeyVal);
                    parameters.Add("@ResponseData", model.ResponseData);
                    parameters.Add("@result", model.Result);
                    parameters.Add("@currency", model.Currency);
                    parameters.Add("@BankStatus", model.BankStatus);
                    parameters.Add("@relationship", model.Relationship);
                    parameters.Add("@chequeBookFacility", model.ChequeBookFacility);
                    parameters.Add("@minBalance", model.MinBalance);
                    parameters.Add("@dateString", model.DateString);
                    parameters.Add("@branchCode", model.BranchCode);
                    parameters.Add("@acctTypeCode", model.AcctTypeCode);
                    parameters.Add("@currencyDesc", model.CurrencyDesc);
                    parameters.Add("@currencyCode", model.CurrencyCode);
                    parameters.Add("@accountType", model.AccountType);
                    parameters.Add("@currentBalance", model.CurrentBalance);
                    parameters.Add("@unclearFunds", model.UnclearFunds);
                    parameters.Add("@netBalance", model.NetBalance);
                    parameters.Add("@balAvailable", model.BalAvailable);
                    parameters.Add("@holdAmount", model.HoldAmount);
                    parameters.Add("@overdraft", model.Overdraft);
                    parameters.Add("@customerName", model.CustomerName);
                    parameters.Add("@ResponseTime", model.ResponseTime);
                    parameters.Add("@Status1", model.Status1);
                    parameters.Add("@ATTRIBUTE1", model.Attribute1);
                    parameters.Add("@ATTRIBUTE2", model.Attribute2);
                    parameters.Add("@ATTRIBUTE3", model.Attribute3);
                    parameters.Add("@ATTRIBUTE4", model.Attribute4);
                    parameters.Add("@ATTRIBUTE5", model.Attribute5);
                    parameters.Add("@CREATED_BY", model.CreatedBy);
                    parameters.Add("@CREATED_BY_Name", model.CreatedByName);
                    parameters.Add("@LAST_UPDATED_BY", model.LastUpdatedBy);
                    parameters.Add("@LAST_UPDATED_BY_Name", model.LastUpdatedByName);
                    parameters.Add("@DELETE_FLAG", model.DeleteFlag);

                    // 🔥 Output parameter
                    parameters.Add("@responseMessage",dbType: DbType.String,size: 500,direction: ParameterDirection.Output);
                    await connection.ExecuteAsync("usp_Insert_Bank_Acc_Response_log",parameters,commandType: CommandType.StoredProcedure);
                    responseMessage = parameters.Get<string>("@responseMessage");
                }
            }
            catch (Exception ex)
            {
                // Optional: log exception
                responseMessage = "Exception: " + ex.Message;
            }

            return responseMessage;
        }
        #endregion
        public async Task<Bank_Acc_Response_log_Model> MapBank_Acc_Summ_NS_ToLogModel(BankAccDetails model)
        {
            var logModel = new Bank_Acc_Response_log_Model()
            {
                //Status = model.Status,
                //Message = model.Message,
                //ActionName = model.ActionName,
                //MethodName = model.MethodName,
                AcctNumber = model.AcctNumber,
                BranchCode = model.BranchCode,
                CustomerID = model.CustomerID,
                KeyVal = model.KeyVal,
                Attribute1 = model.ATTRIBUTE1,
                Attribute2 = model.ATTRIBUTE2,
                Attribute3 = model.ATTRIBUTE3,
                Attribute4 = model.ATTRIBUTE4,
                Attribute5 = model.ATTRIBUTE5,
                CreatedBy = model.CreatedBy,
                CreationDate = DateTime.UtcNow,
                LastUpdatedBy = model.LastUpdatedBy,
                LastUpdatedByName = model.LastUpdatedByName,
                CreatedByName = model.CreatedByName,
                LastUpdateDate = model.LastUpdateDate,
                DeleteFlag = model.DeleteFlag,
            };
            return logModel;
        }

        public async Task<Bank_Acc_Response_log_Model> MapBank_Acc_Response_Details_ToLogModel(Bank_Acc_Response_Details model)
        {
            var logModel = new Bank_Acc_Response_log_Model()
            {
                //Status = model.Status,
                //Message = model.Message,
                //ActionName = model.ActionName,
                //MethodName = model.MethodName,
                AcctNumber = model.AcctNumber,
                CustomerID = model.CustomerID,
                KeyVal = model.KeyVal,
                ResponseData = model.ResponseData,
                Result = model.Result,
                Currency = model.Currency,
                BankStatus = model.BankStatus,
                Relationship = model.Relationship,
                ChequeBookFacility = model.ChequeBookFacility,
                MinBalance = model.MinBalance,
                DateString = model.DateString,
                BranchCode = model.BranchCode,
                AcctTypeCode = model.AcctTypeCode,
                CurrencyDesc = model.CurrencyDesc,
                CurrencyCode = model.CurrencyCode,
                AccountType = model.AccountType ,
                CurrentBalance = model.CurrentBalance,
                UnclearFunds = model.UnclearFunds,
                NetBalance = model.NetBalance,
                BalAvailable = model.BalAvailable,
                HoldAmount = model.HoldAmount,
                Overdraft = model.Overdraft,
                CustomerName = model.CustomerName,
                ResponseTime = model.ResponseTime,
                //CreatedBy = model.CREATED_BY,
                //CreationDate = model.CREATION_DATE,
                //LastUpdatedBy = model.LAST_UPDATED_BY,
                //LastUpdatedByName = model.LAST_UPDATED_BY_Name,
                CreatedByName = model.CreatedByName,
                //LastUpdateDate = model.LAST_UPDATE_DATE,
                DeleteFlag = 'N',
            };
            return logModel;
        }

        public async Task<Bank_Acc_Response_Details> GetBnakAccDetailResponse_byMkey(int mkey)
        {
            try
            {
                using (var connection = _dapperDbConnection.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Mkey", mkey, DbType.Int32);
                    var result = await connection.QueryFirstOrDefaultAsync<Bank_Acc_Response_Details>("usp_Get_Bank_Acc_Response_ByMkey", parameters, commandType: CommandType.StoredProcedure);
                    return result; // can be null if no record found
                }
            }
            catch (Exception ex)
            {
                // log exception properly
                Console.WriteLine("Error fetching data: " + ex.Message);
                return null; // or throw;
            }





        }


        public  async Task<string> UpdateAccountBalanceDetailsAsync(Bank_Acc_Response_Details model , string userid)
        {
            string responseMessage = "Unhandled error occurred";
            // Get Admin user details
            string strUserId = userid; //ConfigurationManager.AppSettings["AdminUserId"];
                                       //string userName = "Admin";    //ConfigurationManager.AppSettings["AdminUserName"];
            int userId = Convert.ToInt32(strUserId);

            model.CreatedBy = userId;
            model.CreationDate = DateTime.UtcNow;
            // log.CREATED_BY_Name = userName;
            model.DeleteFlag = string.IsNullOrEmpty(model.DeleteFlag.ToString()) ? 'N' : model.DeleteFlag;
            try
            {
              using(var connection = _dapperDbConnection.CreateConnection())
                 {
                      connection.Open();
                     var parameters = new DynamicParameters();
                     parameters.Add("@pMkey", model.Mkey);
                     parameters.Add("@pacctNumber", model.AcctNumber);
                     parameters.Add("@pcustomerID", model.CustomerID);
                     parameters.Add("@pkeyVal", model.KeyVal);
                     parameters.Add("@pResponseData", model.ResponseData);
                 
                     // New fields added
                     parameters.Add("@presult", model.Result);
                     parameters.Add("@pcurrency", model.Currency);
                     parameters.Add("@pBankStatus", model.BankStatus);
                     parameters.Add("@prelationship", model.Relationship);
                     parameters.Add("@pchequeBookFacility", model.ChequeBookFacility);
                     parameters.Add("@pminBalance", model.MinBalance);
                     parameters.Add("@pdateString", model.DateString);
                     parameters.Add("@pbranchCode", model.BranchCode);
                     parameters.Add("@pacctTypeCode", model.AcctTypeCode);
                     parameters.Add("@pcurrencyDesc", model.CurrencyDesc);
                     parameters.Add("@pcurrencyCode", model.CurrencyCode);
                     parameters.Add("@paccountType", model.AccountType);
                     // Existing fields 
                     parameters.Add("@pcurrentBalance", model.CurrentBalance);
                     parameters.Add("@punclearFunds", model.UnclearFunds);
                     parameters.Add("@pnetBalance", model.NetBalance);
                     parameters.Add("@pbalAvailable", model.BalAvailable);
                     parameters.Add("@pholdAmount", model.HoldAmount);
                     parameters.Add("@poverdraft", model.Overdraft);
                     parameters.Add("@pcustomerName", model.CustomerName);
                     parameters.Add("@pResponseTime", model.ResponseTime);
                     parameters.Add("@pStatus", model.Status);
                     parameters.Add("@pATTRIBUTE1", model.Attribute1);
                     parameters.Add("@pATTRIBUTE2", model.Attribute2);
                     parameters.Add("@pATTRIBUTE3", model.Attribute3);
                     parameters.Add("@pATTRIBUTE4", model.Attribute4);
                     parameters.Add("@pATTRIBUTE5", model.Attribute5);
                     parameters.Add("@pCREATED_BY", model.CreatedBy);
                     parameters.Add("@pCREATED_BY_Name", model.CreatedByName);
                     parameters.Add("@pCREATION_DATE", model.CreationDate);
                     parameters.Add("@pLAST_UPDATED_BY", model.LastUpdatedBy);
                     parameters.Add("@pLAST_UPDATED_BY_Name", model.LastUpdatedByName);
                     parameters.Add("@pLAST_UPDATE_DATE", model.LastUpdateDate);
                     parameters.Add("@pDELETE_FLAG", model.DeleteFlag);
                     parameters.Add("@responseMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);
                 
                     await connection.ExecuteAsync("Update_AccountBalanceDetails", parameters, commandType: CommandType.StoredProcedure);
                 
                     responseMessage = parameters.Get<string>("@responseMessage");
               }    
            }
            catch (Exception ex)
            {
                // Optional: log exception
                responseMessage = "Exception: " + ex.Message;
            }

            return responseMessage;
        }
    }
}
