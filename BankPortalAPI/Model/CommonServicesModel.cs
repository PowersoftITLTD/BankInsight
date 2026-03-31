namespace BankPortalAPI.Model
{
    public class CommonServicesModel<T>
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public T Data { get; set; }
    }

    public class CommoninputResponse
    {
        public int? UserId { get; set; }
        public int? BusinessGroupId { get; set; }
    }

    public class CommonBankAccountDetailResponse_ByAccount
    {
        public int? UserId { get; set; }
        public int? BusinessGroupId { get; set; }
        public string AccountNo { get; set;}
    }

    public class CommonInputAprovalStatus
    {
        public int? UserId { get; set; }
        public int? BusinessGroupId { get; set; }
        public char? activeFlag { get; set;}
    }

    public class ViewAllBankAccountDetails
    {
        
            public int? UserId { get; set; }
            public int? BusinessGroupId { get; set; }
            public int? BankId { get; set; }
            public string BankName { get; set; }
        
    }







    public class HostEnvironment
    {
        public string env { get; set; }
    }
    public class FileSettings
    {
        public string FilePath { get; set; }
    }

    public class jsonEncryptModel
    {
        public string jsonEncrypt { get; set; }
    }
}
