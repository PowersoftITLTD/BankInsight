namespace BankPortalAPI.Model
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class UserLoginModel
    {
        public int UserId { get; set; }

        public string LoginName { get; set; }

        public string? PasswordHash { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }


        //public bool IsActive { get; set; }

        //public DateTime CreatedDate { get; set; }

        //public string? ModifiedBy { get; set; }

        //public DateTime? ModifiedDate { get; set; }  
    }
    public class ResponseObject
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; } // You can make Data generic if needed
    }
    public class UserMasterLoginModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        //public byte[] LOGIN_PASSWORD { get; set; }  // Stored as varbinary
        //public long CONTACT_NO { get; set; }
    }

}
