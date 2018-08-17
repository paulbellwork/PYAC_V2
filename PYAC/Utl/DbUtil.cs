using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Oracle.DataAccess.Client;

namespace Utl
{
    public class DbUtil
    {
     
        DbConnection cn;
        DbProviderFactory provider;
        String connectionName = "";

        public DbUtil()
        {
            connectionName = "DB";
            init();
         
        }
        public void init()
        {
            try
            {

          
            string dbProvider = getProvider();
            DataTable dt = DbProviderFactories.GetFactoryClasses();
            provider = DbProviderFactories.GetFactory(dbProvider);
            int i = dt.Rows.Count;
            cn = provider.CreateConnection();
            cn.ConnectionString = getConnectionString(connectionName);
             }
            catch (Exception e)
            {
                Writer.writeAll(e.ToString());
            }
        }
        public DbUtil(String ConnectionName)
        {
            connectionName = ConnectionName;
            init();
          
        }
        private static String getConnectionString(String connectionName)
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
        }
        private string getProvider()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["DB"].ProviderName;
        }
        public void OpenConnection()
        {
            try
            {
                cn.Open();
            }
            catch (Exception e)
            {
                Writer.writeAll(e.ToString());
            }
        }
       

        public DataTable executeQuery(String sql)
        {

            if (cn.State != ConnectionState.Open)
            {
                OpenConnection();
            }
            DataTable dt = new DataTable();
            Writer.writeAll(sql);
            try
            {
                DbCommand command = provider.CreateCommand();
                command.Connection = cn;
                command.CommandText = sql;

                DbDataAdapter adapter = provider.CreateDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(dt);
            }
            catch (Exception e)
            {
                Writer.writeAll(e.ToString());
                Writer.writeAll(sql);
            }
            finally
            {
                cn.Close();
            }

            // if (cn.State != ConnectionState.Closed)

            return dt;
        }
      
      

        public int Execute(string sql)
        {
            if (cn.State != ConnectionState.Open)
            {
                OpenConnection();
            }
            int iAffected = 0;
            DbCommand command = provider.CreateCommand();
            try
            {
               

                command.Connection = cn;
                command.CommandText = sql;
                command.Transaction = cn.BeginTransaction(IsolationLevel.ReadCommitted);

                DbParameter id = provider.CreateParameter();
                id.ParameterName = ":retid";
                id.Direction = ParameterDirection.InputOutput;
                id.DbType = DbType.Int32;
        //        command.Parameters.Add(id);

                iAffected = command.ExecuteNonQuery();
                command.Transaction.Commit();
       //         iAffected = Int32.Parse(id.Value.ToString());
            }
            catch (DbException dbe)
            {
                command.Transaction.Rollback();
                Writer.writeAll(dbe.ToString());
                Writer.writeAll(sql);
                iAffected = 0;
            }
            catch (Exception e)
            {
                command.Transaction.Rollback();
                Writer.writeAll(e.ToString());
                Writer.writeAll(sql);
                iAffected = 0;
            }
            finally
            {
                cn.Close();
            }
            //if (cn.State != ConnectionState.Closed)
            //    cn.Close();

            return iAffected;
        }
    }
}
