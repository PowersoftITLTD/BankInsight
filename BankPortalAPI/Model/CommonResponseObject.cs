namespace BankPortalAPI.Model
{
    public class CommonResponseObject
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; } // You can make Data generic if needed
    }
    public class ProcessResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class InsertResponse
    {
        public int Mkey { get; set; }
        public string Message { get; set; }
    }

    public class InsertBankAcc_SummResponse
    {
        public int Mkey { get; set; }
        public int SrNo { get; set; }
        public string Message { get; set; }
    }
}
