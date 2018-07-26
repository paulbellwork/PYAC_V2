using System;
using System.Collections.Generic;
using System.Text;
using Kepware.ClientAce.OpcDaClient;
using System.Globalization;

namespace Utl
{
    public class OPCUtil
    {
        private DaServerMgt _daServerMgt;
        private static string _Win64Directory = "c:\\windows\\syswow64\\";
        private static string _Win32Directory = "c:\\windows\\system32\\";
        private static string _OpcEnumDir = ".\\OPCEnum\\";
        private static string _OpcCom = "opccomn_ps.dll";
        private static string _OpcProxy = "opcproxy.dll";
        private static string _OpcEnum = "opcenum.exe";

        public bool checkOPCInstallation()
        {
            bool isIntalled = false;

            try
            {
                if (!System.IO.File.Exists(getWorkingDirectory() + _OpcEnum))
                {
                    //Copy Opc File
                    if (CopyOpcFile())
                    {
                        //register dll
                        RegisterOpc();
                        isIntalled = true;
                    }
                }
                else
                {
                    isIntalled = true;
                }
            }
            catch (Exception e)
            {
                Writer.writeAll("OPC Installation failed : " + e.ToString());
            }

            return isIntalled;
        }
        private static string getWorkingDirectory()
        {
            String WinDirectory = _Win32Directory;
            if (System.IO.Directory.Exists(_Win64Directory))
            {
                WinDirectory = _Win64Directory;
            }
            return WinDirectory;
        }
        private bool CopyOpcFile()
        {
            Boolean successCopy = true;
            String s = Environment.CurrentDirectory;
            try
            {
                System.IO.File.Copy(_OpcEnumDir + _OpcCom, getWorkingDirectory() + _OpcCom);
                System.IO.File.Copy(_OpcEnumDir + _OpcProxy, getWorkingDirectory() + _OpcProxy);
                System.IO.File.Copy(_OpcEnumDir + _OpcEnum, getWorkingDirectory() + _OpcEnum);
            }
            catch (Exception e)
            {
                Writer.writeAll(e.ToString());
                String lines = "Erreur IO \n" +
                                "\nCurrent Directory : " + s +
                                "\nWorkingDirectory : " + getWorkingDirectory() +
                                "\nError : " + e.ToString();
                Writer.writeAll(lines);
                successCopy = false;

            }
            return successCopy;
        }
        private void RegisterOpc()
        {
            RegisterOpcComn();
            RegisterOpcProxy();
            RegisterOpcEnum();

        }
        private void RegisterOpcComn()
        {

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = getWorkingDirectory();
            startInfo.FileName = "regsvr32.exe";

            startInfo.Arguments = "/s " + _OpcCom;
            process.StartInfo = startInfo;
            process.Start();

        }
        private void RegisterOpcProxy()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = getWorkingDirectory();
            startInfo.FileName = "regsvr32.exe";

            startInfo.Arguments = "/s " + _OpcProxy;
            process.StartInfo = startInfo;
            process.Start();
        }
        private void RegisterOpcEnum()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = getWorkingDirectory();
            startInfo.FileName = _OpcEnum;

            startInfo.Arguments = "-service";
            process.StartInfo = startInfo;
            process.Start();

        }
        private String getOPCServeurURL()
        {
            return System.Configuration.ConfigurationManager.AppSettings["url"];
        }
        public bool Connect(IOPCInterface o)
        {
            bool connect = false;
            connect = Connect();
            if (connect)
            {
                try
                {
                    _daServerMgt.ReadCompleted += o.OPC_ReadCompleted;
                    _daServerMgt.DataChanged += o.OPC_DataChanged;
                    _daServerMgt.ServerStateChanged += o.OPC_ServerStateChanged;
                }
                catch (Exception e)
                {
                    Writer.writeAll("Handled Connect exception. Reason: " + e.ToString());
                    _daServerMgt = null;
                    connect = false;
                }
            }
            return connect;
        }
        public bool Connect()
        {
            bool connect = false;
            try
            {
                String s = CultureInfo.CurrentCulture.Name;
                ConnectInfo ci = new ConnectInfo();
                ci.ClientName = "Kepware.KEPServerEX.V6";
                ci.LocalId = s;// "en";
                ci.KeepAliveTime = 10000;
                ci.RetryAfterConnectionError = true;
                ci.RetryInitialConnection = true;


                ci.RetryInitialConnection = true;
                _daServerMgt = new DaServerMgt();

                bool connectFailed = false;
                int clientHandle = 1;
                string url = getOPCServeurURL();

                _daServerMgt.Connect(url, clientHandle, ref ci, out connectFailed);

                if (!connectFailed)
                {
                    connect = true;
                }
                else
                {
                     
                }
            }
            catch (Exception e)
            {
                Writer.writeAll("Handled Connect exception. Reason: " + e.ToString());
                connect = false;
            }
            return connect;
        }
        public void Subsribe(ItemIdentifier[] id)
        {

            int clientSubscriptionHandle = 1;
            bool active = true;
            int updateRate = 100;
            Single deadBand = 0;

            int revisedUpdateRate;
            int serverSubscription;

            try
            {
                ReturnCode rc = _daServerMgt.Subscribe(clientSubscriptionHandle, active, updateRate, out revisedUpdateRate, deadBand, ref id, out serverSubscription);
                if (rc != ReturnCode.SUCCEEDED)
                {
                    Writer.writeAll("subscribe request failed for one or more items : " + rc.ToString());
                    Writer.writeAll("ItemID : " + id.ToString());

                    // Examine ResultID objects for detailed information.
                }
                else
                {
                    Writer.writeAll("Successfully subscribe to ItemID : " + id[0].ItemName);
                }
            }
            catch (Exception e)
            {
                Writer.writeAll("Handled Subscribe exception. Reason: " + e.ToString());
            }
        }
        public bool WriteAsync(ItemIdentifier[] itemIdentifiers, ItemValue[] itemValues)
        {
            bool succes = false;
            // Declare variables
            int transactionHandle = 1;

            ReturnCode returnCode;
            try
            { // Call WriteAsync API method
                returnCode = _daServerMgt.WriteAsync(transactionHandle, ref itemIdentifiers, itemValues);
                // Check item results

                if (returnCode != ReturnCode.SUCCEEDED)
                {
                    Writer.writeAll("Write request failed for one or more items : " + returnCode.ToString());
                    Writer.writeAll("ItemID : " + itemIdentifiers.ToString());
                    Writer.writeAll("ItemValue : " + itemValues.ToString());
                    // Examine ResultID objects for detailed information.
                }
                else
                {
                    succes = true;
                }
            }
            catch (Exception e)
            {

                for (int i = 0; i < itemIdentifiers.Length; i++)
                {
                    Writer.writeAll("Item : " + itemIdentifiers[i].ItemName);
                    Writer.writeAll("Value : " + itemValues[i].Value.ToString());
                }
                Writer.writeAll("WriteAsync exception. Reason: " + e);
                if (!_daServerMgt.IsConnected)
                {
                    Writer.writeAll("");
                    Connect();
                }
            }

            return succes;

        }

        public void AsyncRead(ItemIdentifier[] itemIdentifiers)
        {
            ReturnCode returnCode;
            object itemIndex = "1";
            int maxAge = 0;
            int TransID = (new Random()).Next(0, 65535);

            try
            {
                returnCode = _daServerMgt.ReadAsync(TransID, maxAge, ref itemIdentifiers);

                if (returnCode != ReturnCode.SUCCEEDED)
                {
                    Writer.writeAll("ReturnCode :" + returnCode.ToString());
                }
            }
            catch (Exception e)
            {
                Writer.writeAll("AsyncRead failed Reason : " + e.ToString());
            }

        }

        public void Disconnect()
        {

            if (_daServerMgt.IsConnected)
            {
                _daServerMgt.Disconnect();
            }
        }
    }
}
