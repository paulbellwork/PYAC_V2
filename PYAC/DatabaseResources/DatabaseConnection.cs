using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;


namespace PYAC.DatabaseResources
{
    public class DatabaseConnection
    {
        //public OracleConnection GetConnection()
        //{
        //    string connection = System.Configuration.ConfigurationManager.AppSettings["connectionString"].ToString();

        //    return new OracleConnection(connection);
        //}


        string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();
        OracleConnection _connection;
        
            
        public DatabaseConnection()
        {
            _connection = new OracleConnection();
            _connection.ConnectionString = connectionString;
            OracleCommand cmd = new OracleCommand();
            _connection.Open();

            //cmd.CommandText = "INSERT INTO TC_LIST VALUES('3', '5123505')";
            //cmd.Connection = _connection;
            //cmd.ExecuteNonQuery();
            _connection.Close();




        }

    }
}
