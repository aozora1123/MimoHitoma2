using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Web.Configuration;

namespace MimoHitoma2.Models
{
    public class UserIdentityModel : CommonUsedModel
    {
        public string account { get; set; }
        public string password { get; set; }
        public string connectingDBStr { get; set; }


        public bool IsAuthenticatedUser(string account, string password)
        {
            connectingDBStr = "ConnectToAccountDB";
            MySqlConnection conn = null;
            MySqlDataReader DataReader = null;
            MySqlCommand cmd = null;

            try
            {
                conn = ConnectToDatabase(connectingDBStr);
                // 採參數化查詢
                string queryString = String.Format("select {0} from {1} where {2} = {3} and {4} = {5};",
                    "username", "account_basic_info", "username", "@ptmUserName", "password", "@ptmUserPassword");
                cmd = new MySqlCommand(queryString, conn);
                cmd.Parameters.AddWithValue("@ptmUserName", account);
                cmd.Parameters.AddWithValue("@ptmUserPassword", password);

                DataReader = cmd.ExecuteReader();
                if (DataReader.HasRows)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnectionWithDatabase(conn, DataReader, cmd);
            }
        }
    }
}