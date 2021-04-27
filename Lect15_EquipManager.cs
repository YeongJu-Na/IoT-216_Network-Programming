using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyLibrary;

namespace EquipManager
{
    class SQLDB
    {
        SqlConnection sqlconn = new SqlConnection();
        SqlCommand sqlcmd = new SqlCommand();
        string ConnStr;

        public SQLDB(string str)
        {
            ConnStr = str;
            sqlconn.ConnectionString = ConnStr;
            sqlconn.Open();
            sqlcmd.Connection = sqlconn;
        }
        // 
        public object Run(string sql)
        {
            try
            {
                sqlcmd.CommandText = sql;
                if (mylib.GetToken(0, sql.Trim(), ' ').ToUpper() == "SELECT")
                {
                    SqlDataReader sdr = sqlcmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(sdr);
                    sdr.Close();
                    return dt;
                }
                else
                {
                    return sqlcmd.ExecuteNonQuery();
                }
            }
            catch(Exception e1)
            {
                Console.WriteLine(e1);
                return null;
            }
        }

        public object Get(string sql)
        {
            try
            {
                sqlcmd.CommandText = sql;
                if(mylib.GetToken(0 ,sql.Trim(),' ').ToUpper() == "SELECT")
                {
                    object obj = sqlcmd.ExecuteScalar();
                    return obj;
                }
            }
            catch(Exception e1)
            {
                Console.WriteLine(e1);
            }
            return null;
        }
    }
}
