using Kepware.ClientAce.OpcDaClient;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Events;
using PYAC.Infrastructure;
using PYAC.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Utl;

namespace PYAC.ViewModels
{
    class MainWindowViewModel : BindableBase, IOPCInterface
    {
        //CLASS VARIABLES
        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;

        private OPCUtil _OPCUtil;
        private OPCUtil _OPCRecipeThread;

        public string OPCBASEPATH = "AB.Pyradia";
        public string name = "";  
        public int countSubscription;
        int targetSubscription = 2;
        Thread loadingWindowThread;
        static Thread OPCConnectionThread;
        static bool isInstantiated;

        //CONSTRUCTOR
        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _regionManager.RegisterViewWithRegion(Regions.ContentRegion, typeof(SegmentSummaryPage)); //Sets default user control as SegmentSummaryPage
            _regionManager.RegisterViewWithRegion(Regions.TitleMenuRegion, typeof(TitleMenu));
            _regionManager.RegisterViewWithRegion(Regions.ButtonsMenuRegion, typeof(ButtonsMenu));

            _eventAggregator = eventAggregator;
            countSubscription = 0;

            //Start a new thread that runs the loading window independently(in order to allow the progress ring to appear)
            loadingWindowThread = new Thread(new ThreadStart(() =>
            {
                // Create and show the Window
                LoadingWindow tempWindow = new LoadingWindow();
            tempWindow.Show();
            tempWindow.Activate();
            // Start the Dispatcher Processing
            try
            {
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (Exception) { }
        }));
            loadingWindowThread.SetApartmentState(ApartmentState.STA);
            loadingWindowThread.IsBackground = true;
            loadingWindowThread.Start();

            eventAggregator.GetEvent<IsSubscriptionCompleted>().Subscribe(CountSubscription);
            eventAggregator.GetEvent<UpdatedSourceEvent>().Subscribe(WriteOPCValue);
        }


        //METHODS
        public void StartThread()
        {
            //Thread indep = new Thread(new ThreadStart(ReadRecipe));
            //Thread indep = new Thread(delegate() { ReadRecipe(); });
            Thread indep = new Thread(()=> { ReadRecipe("Test1"); });
        }
        public void ReadRecipe(string RecipName)
        {
            //1-instantiate separate OPC connection on separate thread
            _OPCRecipeThread = new OPCUtil();
            bool isConnected = _OPCRecipeThread.Connect(this);
            
            
            //2-Get recipe details that will be written onto the PLC(Need to access DB)
            //SegmentSummaryPageViewModel.RecipeName

            //if (isConnected)
            //{
                
            //}
        }


        private void CheckResponsibility(ItemValueCallback itemValue)
        {
        /**************************************************************************************************************
        -> Method     : CheckResponsibility
        -> Param	  : ItemValueCallback itemValue
        -> Event      : This method is triggered by OPC_DataChanged Method      
        -> Description: This method publishes an event which updates the properties on UI
        **************************************************************************************************************/
            _eventAggregator.GetEvent<UpdatedValueEvent>().Publish(itemValue);
        }
        private void CountSubscription(bool count)
        {
        /**************************************************************************************************************
        -> Method     : CountSubscription
        -> Param	  : bool count
        -> Event      : This method is triggered whenever a new View has subscribed to the event       
        -> Description: This method is used to know when is the right time to instantiate the OPC Connection. Everytime
                        a new view is subscribed, it publishes an event that contains a bool as payload, which increases 
                        the value of the countSubsription property. When the value matches the target, 
                        instantiateOPCConnection is called.
        **************************************************************************************************************/
            if (count)
            {
                countSubscription++;
            }
            if (!isInstantiated)
            {
                //navigate to the user controls in order to successfully load all properties on the view causing the publish to be effective              
                isInstantiated = true;

                _regionManager.RequestNavigate(Regions.ContentRegion, "SegmentParameterPage");
                _regionManager.RequestNavigate(Regions.ContentRegion, "PartsLoadPage");
                _regionManager.RequestNavigate(Regions.ContentRegion, "RecipePage");
                _regionManager.RequestNavigate(Regions.ContentRegion, "SegmentSummaryPage");

            }
            //the thread below instantiates the opc connection, waits 4 seconds(to leave the loading window up and running) then aborts the loading window thread.
            if (countSubscription == targetSubscription)
            {
                //InstantiateOPCConnection();
                OPCConnectionThread = new Thread(() => { InstantiateOPCConnection(); Thread.Sleep(4000); loadingWindowThread.Abort();  });
                OPCConnectionThread.Start();
            }
        }
        public void InstantiateOPCConnection()
        {
        /**************************************************************************************************************
        -> Method     : InstantiateOPCConnection
        -> Event      : This method is triggered by CountSubscription Method       
        -> Description: This method updates all properties in the Hardware class lists and instantiates OPCUtil.
        **************************************************************************************************************/
            _OPCUtil = new OPCUtil();

            bool isConnected = _OPCUtil.Connect(this);

            if (isConnected)
            {
                foreach (String hardware in Hardware.SegmentSummaryTagsList)
                {
                    try
                    {
                        _OPCUtil.Subsribe(GetItemIdentifier(hardware.ToString()));

                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Error: " + hardware.ToString() + " failed to be subscribe to\n(" + Ex.Message + ")");
                    }
                }
                foreach (String hardware in Hardware.SegmentParameterTagsList)
                {
                    try
                    {
                        _OPCUtil.Subsribe(GetItemIdentifier(hardware.ToString()));
                    }
                    catch (Exception Ex)           
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Error: " + hardware.ToString() + " failed to be subscribe to\n(" + Ex.Message + ")");
                    }
                }
                //foreach (String hardware in Hardware.OffsetTagsList)
                //{
                //    try
                //    {
                //        _OPCUtil.Subsribe(GetItemIdentifier(hardware.ToString()));
                //    }
                //    catch (Exception Ex)
                //    {
                //        MessageBox.Show(Application.Current.MainWindow, "Error: " + hardware.ToString() + " failed to be subscribe to\n(" + Ex.Message + ")");
                //    }
                //}//useless foreach loop
            }
            else // If OPC server fails to connect
            {
                Writer.writeAll("Error : " + name + " is unable to connect to OPC server");
                MessageBox.Show(Application.Current.MainWindow, "Error: Unable to Connect to Server\nApplication will now Shutdown");
                Environment.Exit(0);
            }
        }
        public void OPC_DataChanged(int clientSubscription, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues)
        {
        /**************************************************************************************************************
        -> Method     : OPC_DataChanged
        -> Param	  : int clientSubscription
        -> Param	  : bool allQualitiesGood
        -> Param	  : bool noErrors
        -> Param	  : ItemValueCallback[] itemValues
        -> Description: This method calls CheckResponsibility method to update UI Properties
        **************************************************************************************************************/
            try
            {
                Writer.writeAll(this.ToString());
                foreach (ItemValueCallback itemValue in itemValues)
                {
                    if (itemValue.ResultID.Succeeded)
                    {

                        CheckResponsibility(itemValue);
                    }
                    else
                    {
                        Writer.writeAll("Item error");

                    }
                }
            }
            catch (Exception Ex)
            {
                Writer.writeAll("DataChanged exception. Reason: " + Ex.Message);
            }
        }//main one
        private void WriteOPCValue(OPCObject obj)
        {
        /**************************************************************************************************************
        -> Method     : WriteOPCValue
        -> Param	  : OPCObject obj
        -> Event      : This method is triggered whenever a write request is submitted, uses an EVENT called UpdatedSourceEvent      
        -> Description: This method is used to write a value on the PLC
        **************************************************************************************************************/
            try
            {
                _OPCUtil.WriteAsync(GetItemIdentifier(obj.itemIdentifier), GetItemValue(obj.itemValue));

            }
            catch (Exception Ex)
            {
                MessageBox.Show(Application.Current.MainWindow, "Error, unable to write\n" + Ex.Message);
            }
        }


        //The following methods are used very rarely
        public void OPC_ReadCompleted(int transactionHandle, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues)
        {
            throw new NotImplementedException();
        }//not too important
        public void OPC_ServerStateChanged(int clientHandle, ServerState state)
        {
            /**************************************************************************************************************
            -> Method     : OPC_ServerStateChanged
            -> Param	  : int clientHandle
            -> Param	  : ServerState state
            -> Event      : This method is triggered whenever the server times out      
            -> Description: This method will retry to instantiate OPC Connection
            **************************************************************************************************************/
            //throw new ServerStateChangedException();
            InstantiateOPCConnection();
        }


        //The following methods just place in the right format the name and value of a tag.
        private ItemIdentifier[] GetItemIdentifier(string tagName)
        {
            ItemIdentifier[] id = new ItemIdentifier[1];

            id[0] = new ItemIdentifier();
            id[0].ItemName = OPCBASEPATH + "." + tagName;
            id[0].ClientHandle = tagName;
            id[0].DataType = Type.GetType("System.String");
            return id;
        }
        public ItemValue[] GetItemValue(string value) //item value is a Kepware class  
        {
            ItemValue[] itemValues = new ItemValue[1];
            itemValues[0] = new ItemValue();
            itemValues[0].Value = value;
            itemValues[0].Quality = QualityID.OPC_QUALITY_GOOD;
            itemValues[0].TimeStamp = DateTime.Now;
            return itemValues;
        }
    }

    [Serializable]
    internal class ServerStateChangedException : Exception
    {
        public ServerStateChangedException()
        {
        }

        public ServerStateChangedException(string message) : base(message)
        {
        }

        public ServerStateChangedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ServerStateChangedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}