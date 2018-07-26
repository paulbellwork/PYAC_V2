//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.ServiceModel;
//using System.Text;
//using DBADashboard.DatabaseConnectionServiceReference;
//using DatabaseConnectionServiceClient;
//using System.Web;
//namespace PYAC.DatabaseResources
//{
//    public class GetOracleConnection
//    {
//        public static void PrepareOracleConnection()
//        {

//            DatabaseConnectionServiceClient oraClient = new DatabaseConnectionServiceClient();
//            // build new endPoint Address
//            StringBuilder sb = new StringBuilder();
//            try
//            {
//                // preparing new EndPoint address CDSC-6334
//                string server = ConfigurationManager.AppSettings["Bao.Vo.Default.Server"];
//                if (server == null || string.IsNullOrEmpty(server.Trim()))
//                {
//                    server = Environment.MachineName;
//                } // end if

//                sb.Append(string.Format("http://{0}.bh.textron.com", server));
//                sb.Append(oraClient.Endpoint.ListenUri.AbsolutePath);

//                // override endPoint Address
//                EndpointAddress epa = new EndpointAddress(sb.ToString());
//                oraClient.Endpoint.Address = epa;

//                //oraClient.Open();
//                OraInfo oraInfo = oraClient.GetOraMasterAppInfo(ConfigurationManager.AppSettings["A060"], "DBAATWEB");
//                if (oraInfo != null && !string.IsNullOrEmpty(oraInfo.OraId) && !string.IsNullOrEmpty(oraInfo.OraPw))
//                {
//                    OraCredential.OraID = oraInfo.OraId;
//                    OraCredential.OraPwd = oraInfo.OraPw;
//                }
//                else
//                {
//                    throw new Exception(String.Format("Unexpected Database Error. {0}", "Problems with Oracle Master App."));
//                } // end if
//            }
//            finally
//            {
//                oraClient.Close();
//            }
//        }

//        public static string BuildConnectionString()
//        {
//            PrepareOracleConnection();
//            // build connection string
//            StringBuilder stringBuilder = new StringBuilder();
//            stringBuilder.Append("User Id=");
//            stringBuilder.Append(OraCredential.OraID);
//            stringBuilder.Append(";");
//            stringBuilder.Append("Password=");
//            stringBuilder.Append(OraCredential.OraPwd);
//            stringBuilder.Append(";");
//            stringBuilder.Append("Data Source=");
//            //stringBuilder.Append("A060PROD");
//            //stringBuilder.Append("A060TEST");
//            stringBuilder.Append(ConfigurationManager.AppSettings["A060"]);
//            stringBuilder.Append(";");
//            stringBuilder.Append("Min Pool Size=1");
//            stringBuilder.Append(";");
//            stringBuilder.Append("Connection Lifetime=120");
//            stringBuilder.Append(";");
//            stringBuilder.Append("Connection Timeout=60");
//            stringBuilder.Append(";");
//            stringBuilder.Append("Incr Pool Size=5");
//            stringBuilder.Append(";");
//            stringBuilder.Append("Decr Pool Size=1");
//            stringBuilder.Append(";");
//            stringBuilder.Append("Pooling=true");

//            // set connection string value
//            return stringBuilder.ToString();

//        }
//    }
//}


