using System.Data;

namespace BankPortalAPI.Repository.Iservices
{
    public interface IDapperDbConnection
    {
        public IDbConnection CreateConnection();
    }
}
