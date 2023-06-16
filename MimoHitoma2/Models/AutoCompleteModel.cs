using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace MimoHitoma2.Models
{
    public class AutoCompleteModel : CommonUsedModel
    {
        public List<string> AutoComplete_GetImageTags(string prefix)
        {
            List<string> matchTags = new List<string>();
            string connectingDBStr = "ConnectToImageDB";
            MySqlConnection conn = null;
            MySqlDataReader DataReader = null;
            MySqlCommand cmd = null;
            string queryCommand = "";

            try
            {
                conn = ConnectToDatabase(connectingDBStr);
                queryCommand = string.Format("select {0} from {1} where {2} like '{3}%';", "tag_name", "image_tags", "tag_name", prefix);
                cmd = new MySqlCommand(queryCommand, conn);
                DataReader = cmd.ExecuteReader();

                if (DataReader.HasRows)
                {
                    while (DataReader.Read())
                    {
                        matchTags.Add(DataReader["tag_name"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnectionWithDatabase(conn, DataReader, cmd);
            }

            return matchTags;
        }
    }
}