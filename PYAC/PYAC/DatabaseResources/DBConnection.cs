////using CoreService.Domain;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using ACQC.Models;

namespace PYAC.DatabaseResources
{
    public class DBConnection
    {
        private OracleConnection conn;
        private string ConnectionString;

        public DBConnection()
        { }

        //CDSC-6659
        private static string GetCurrentEnvironmentName()
        {
            string environment = string.Empty;
            // NOTE:  cannot use RequestContext when calling this method from Global.asax, set it to machineName, which should be
            // picked up in the 2nd condition of each if/else stmt

            //var currentHostName = HttpContext.Current.Request.Url.Host;
            ////var currentHostName = HttpContext.Current.Server.MachineName;
            var currentHostName = "";

            if (currentHostName == CommonVariables.Environment.HOST_NAME_TEST
                 || Environment.MachineName.Equals(CommonVariables.Environment.TEST_SERVER, StringComparison.CurrentCultureIgnoreCase))
            {
                environment = CommonVariables.Environment.ENVIRONMENT_TEST;
            }
            else if (currentHostName == CommonVariables.Environment.HOST_NAME_PRODUCTION
                 || Environment.MachineName.Equals(CommonVariables.Environment.PROD_SERVER, StringComparison.CurrentCultureIgnoreCase))
            {
                environment = CommonVariables.Environment.ENVIRONMENT_PRODUCTION;
            }
            //default dvlp
            else
            {
                environment = CommonVariables.Environment.ENVIRONMENT_DEVELOPMENT;
            }
            return environment;
        }

        public bool Open()
        {
            bool success = false;
            //CDSC-6659
            switch (GetCurrentEnvironmentName())
            {
                case CommonVariables.Environment.ENVIRONMENT_DEVELOPMENT:
                    ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ACQC_DVLP"].ConnectionString;
                    break;
                case CommonVariables.Environment.ENVIRONMENT_PRODUCTION:
                    ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ACQC_PROD"].ConnectionString;
                    break;
                case CommonVariables.Environment.ENVIRONMENT_TEST:
                    ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ACQC_TEST"].ConnectionString;
                    break;
            }

            if (conn == null || conn.State == System.Data.ConnectionState.Closed)
            {
                /*try
                {*/
                conn = new OracleConnection(ConnectionString);
                conn.Open();
                success = true;
                /*}
                catch(Exception e)
                { }*/
            }
            else
                return success;
            return success;
        }

        public OracleConnection getConnection()
        {
            if (conn.State != System.Data.ConnectionState.Closed)
                return conn;
            else
                Open();
            return conn;
        }

        public void Close()
        {
            conn.Close();
            conn.Dispose();
        }

        public bool ConnIsOpened()
        {
            if (conn.State == System.Data.ConnectionState.Open)
                return true;
            else
                return false;
        }
    }
}