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
        private readonly IRegionManager _regionManager;

        private OPCUtil _OPCUtil;
        public string OPCBASEPATH = "AB.Pyradia";
        public string name = "";  // for now, blank, unsure what to use as name of the server
        protected readonly IEventAggregator _eventAggregator;
        public int countSubscription;
        int targetSubscription = 2;
        LoadingWindow loading;
        Thread newWindowThread;
        static bool isInstantiated;


        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            loading = new LoadingWindow();

            _regionManager = regionManager;
            _regionManager.RegisterViewWithRegion(Regions.ContentRegion, typeof(SegmentSummaryPage)); //Sets default user control as SegmentSummaryPage
            _regionManager.RegisterViewWithRegion(Regions.TitleMenuRegion, typeof(TitleMenu));
            _regionManager.RegisterViewWithRegion(Regions.ButtonsMenuRegion, typeof(ButtonsMenu));

            _eventAggregator = eventAggregator;
            countSubscription = 0;

            //Start a new thread that runs the loading window independently(in order to allow the progress ring to appear)
            //newWindowThread = new Thread(new ThreadStart(() =>
            //{
            //    // Create and show the Window
            //    LoadingWindow tempWindow = new LoadingWindow();
            //    tempWindow.Show();
            //    tempWindow.Activate();
            //    // Start the Dispatcher Processing
            //    try
            //    {
            //        System.Windows.Threading.Dispatcher.Run();
            //    }
            //    catch (Exception) { }
            //}));
            //newWindowThread.SetApartmentState(ApartmentState.STA);
            //newWindowThread.IsBackground = true;
            //newWindowThread.Start();

            eventAggregator.GetEvent<IsSubscriptionCompleted>().Subscribe(CountSubscription);
            eventAggregator.GetEvent<UpdatedSourceEvent>().Subscribe(WriteOPCValue);
        }


        private void checkResponsability(ItemValueCallback itemValue)
        {
            _eventAggregator.GetEvent<UpdatedValueEvent>().Publish(itemValue);
        }
        private void CountSubscription(bool count)
        {

            if (count)
            {
                countSubscription++;
            }
            if (!isInstantiated)
            {
                //navigate to the user controls               
                isInstantiated = true;

                _regionManager.RequestNavigate(Regions.ContentRegion, "SegmentParameterPage");
                _regionManager.RequestNavigate(Regions.ContentRegion, "SegmentSummaryPage");
                _regionManager.RequestNavigate(Regions.ContentRegion, "PartsLoadPage");
                _regionManager.RequestNavigate(Regions.ContentRegion, "RecipePage");
                _regionManager.RequestNavigate(Regions.ContentRegion, "SegmentSummaryPage");

            }

            if (countSubscription == targetSubscription)
            {

                //InstantiateOPCConnection();
            }

        }

        public void InstantiateOPCConnection()
        {
            _OPCUtil = new OPCUtil();

            bool isConnected = _OPCUtil.Connect(this);

            if (isConnected)
            {

                foreach (String hardware in Hardware.SegmentSummaryTagsList)
                {
                    try
                    {
                        _OPCUtil.Subsribe(getItemIdentifier(hardware.ToString()));

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Error: " + hardware.ToString() + " failed to be subscribe to\n(" + ex.ToString() + ")");
                        string cheese = ex.ToString();
                    }
                }
                foreach (String hardware in Hardware.SegmentParameterTagsList)
                {
                    try
                    {
                        _OPCUtil.Subsribe(getItemIdentifier(hardware.ToString()));
                    }
                    catch (Exception ex)           
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Error: " + hardware.ToString() + " failed to be subscribe to\n(" + ex.ToString() + ")");
                    }
                }

                foreach (String hardware in Hardware.OffsetTagsList)
                {
                    try
                    {
                        _OPCUtil.Subsribe(getItemIdentifier(hardware.ToString()));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Error: " + hardware.ToString() + " failed to be subscribe to\n(" + ex.ToString() + ")");
                    }
                }
                //newWindowThread.Abort();
            }
            else // If OPC server fails to connect
            {
                Writer.writeAll("Error : " + name + " is unable to connect to OPC server");
                MessageBox.Show(Application.Current.MainWindow, "Error: Unable to Connect to Server\nApplication will now Shutdown");
                System.Windows.Application.Current.Shutdown();
            }
        }

        private ItemIdentifier[] getItemIdentifier(string tagName)
        {
            ItemIdentifier[] id = new ItemIdentifier[1];

            id[0] = new ItemIdentifier();

            id[0].ItemName = OPCBASEPATH + "." + tagName;
            id[0].ClientHandle = tagName;
            id[0].DataType = Type.GetType("System.String");

            return id;

        }

        public ItemValue[] getItemValue(string value) //item value is a kepware class  
        {
            ItemValue[] itemValues = new ItemValue[1];
            itemValues[0] = new ItemValue();
            itemValues[0].Value = value;
            itemValues[0].Quality = QualityID.OPC_QUALITY_GOOD;
            itemValues[0].TimeStamp = DateTime.Now;

            return itemValues;
        }
        public void OPC_DataChanged(int clientSubscription, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues)
        {
            try
            {
                Writer.writeAll(this.ToString());
                foreach (ItemValueCallback itemValue in itemValues)
                {
                    if (itemValue.ResultID.Succeeded)
                    {

                        checkResponsability(itemValue);
                    }
                    else
                    {
                        Writer.writeAll("Item error");

                    }
                }
            }
            catch (Exception ex)
            {
                Writer.writeAll("DataChanged exception. Reason: " + ex.ToString());
            }
        }//main one

        public void OPC_ReadCompleted(int transactionHandle, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues)
        {
            throw new NotImplementedException();
        }//not too important


        public void OPC_ServerStateChanged(int clientHandle, ServerState state)
        {
            //throw new ServerStateChangedException();
            InstantiateOPCConnection();
        }

        private void WriteOPCValue(OPCObject obj)
        {
            try
            {
                _OPCUtil.WriteAsync(getItemIdentifier(obj.itemIdentifier), getItemValue(obj.itemValue));

            }
            catch (Exception Ex)
            {
                MessageBox.Show(Application.Current.MainWindow, "Error, unable to write\n" + Ex.ToString());
            }
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