using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Web.Configuration;


namespace MimoHitoma2.Models
{
    public class CommonUsedModel
    {
        protected MySqlConnection ConnectToDatabase(string connectingDBStr)
        {
            MySqlConnection conn = null;

            try
            {
                ConnectionStringSettingsCollection connectionStrings = WebConfigurationManager.ConnectionStrings as ConnectionStringSettingsCollection;
                string connStr = connectionStrings[connectingDBStr].ToString();
                conn = new MySqlConnection(connStr);
                conn.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return conn;
        }

        protected void CloseConnectionWithDatabase(MySqlConnection conn, MySqlDataReader DataReader, MySqlCommand cmd)
        {
            if (DataReader != null)
            {
                DataReader.Close();
                DataReader.Dispose();
            }
            if (cmd != null)
            {
                cmd.Dispose();
            }
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public string GetSubFolderNames(string fileName)
        {
            // 以前4個字母，作為前兩個subfolder的名字 (例如: ab12cd34.jpg --> ab/12/ab12cd34.jpg)
            string s = "";
            s += fileName.Substring(0, 2) + "\\" + fileName.Substring(2, 2) + "\\";
            return s;
        }

        public string CreateRandomCode(int number)
        {
            string allChar = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z";
            string[] allCharArray = allChar.Split(',');
            string randomCode = "";
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < number; i++)
            {
                randomCode += allCharArray[rand.Next(0, allCharArray.Length)];
            }
            return randomCode;
        }

        public void DisposeDataReaderAndCommand(MySqlDataReader DataReader, MySqlCommand cmd)
        {
            if (DataReader != null)
            {
                DataReader.Close();
                DataReader.Dispose();
            }
            if (cmd != null)
                cmd.Dispose();
        }
    }
}