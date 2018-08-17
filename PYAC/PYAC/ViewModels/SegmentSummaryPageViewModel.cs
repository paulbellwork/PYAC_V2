using Kepware.ClientAce.OpcDaClient;
using Oracle.ManagedDataAccess.Client;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Enums;
using PYAC.Events;
using PYAC.Infrastructure;
using PYAC.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Utl;
using static PYAC.ViewModels.RecipePageViewModel;

namespace PYAC.ViewModels
{
    class SegmentSummaryPageViewModel : BindableBase
    {
        //CLASS VARIABLES
        DispatcherTimer _timer;
        TimeSpan _time;
        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;
        public ICommand NavigateToSegmentParameterPageCommand { get; private set; }
        public DelegateCommand<object> SetPtAdjCommand { get; set; }
        public DelegateCommand<object> SkipCommand { get; set; }
        public DelegateCommand<object> LowTempCommand { get; set; }
        public DelegateCommand<object> StartStopCommand { get; set; }
        public DelegateCommand<object> PartsApplyCommand { get; set; }

        private int newResult = 0;

        OPCUtil OPCReadValues;

        Thread readOPCThread;

        static bool isInstantiated;

        string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();//<---------^

        //CONSTRUCTOR
        public SegmentSummaryPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            eventAggregator.GetEvent<UpdatedValueEvent>().Subscribe(GetItemValue);
            eventAggregator.GetEvent<CookInfoEvent>().Subscribe(GetCookInfo);

            if (!isInstantiated)
            {
                isInstantiated = true;

                eventAggregator.GetEvent<IsSubscriptionCompleted>().Publish(true);

            }

            NavigateToSegmentParameterPageCommand = new DelegateCommand(() => {
                NavigateTo("SegmentParameterPage");
                _eventAggregator.GetEvent<UpdateTitleMenuBarEvent>().Publish("Segment Parameters");

            });
            SetPtAdjCommand = new DelegateCommand<object>(SetPtAdjBtn);
            SkipCommand = new DelegateCommand<object>(SkipBtn);
            LowTempCommand = new DelegateCommand<object>(LowTempBtn);
            StartStopCommand = new DelegateCommand<object>(StartStopBtn);
            PartsApplyCommand = new DelegateCommand<object>(PartsApply);
        }

        //METHODS        
        public void InitializeRecipeSegments()
        {
        /**************************************************************************************************************
        -> Method     : InitializeRecipeSegments
        -> Event      : When Commit load is clicked
        -> Description: This method is used to initialize the chosen recipe by binding all of the properties to the UI.
                        The list of type Segment_Details called "Segments" holds all segments in the recipe and allows 
                        to skip between the segments.
        **************************************************************************************************************/


            //1-instantiate separate OPC connection on separate thread

            //with each new segment, need to write on PLC the desired target value, then need to monitor every x amount seconds
            // need to constantly monitor the values of the PLC for the running segment(maybe on separate thread?, and maybe on separate OPC instance, unsure)

            //readOPCThread = new Thread(new ThreadStart(() =>
            //{
            //    OPCReadValues = new OPCUtil();
            //    if (OPCReadValues.Connect())
            //    {
            //        MessageBox.Show("Connected!");
            //        _eventAggregator.GetEvent<UpdatedSourceEvent>().Publish(new OPCObject("Set_Pt", newResult.ToString()));
            //    }
                
            //}));
            //readOPCThread.Start();


            //2-Get recipe segments that will be written onto the PLC(Need to access DB) and store as segment objects in list "Segments"
            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                //string queryString = String.Format("SELECT * FROM RECIPESEGMENT INNER JOIN segment ON RECIPESEGMENT.ID_segment = segment.id INNER JOIN recipe ON RECIPESEGMENT.ID_recipe = recipe.id where recipe.recipe_name = '{0}' order by segment.segment_number", RecipeName);
                string queryString = String.Format("SELECT * FROM SEGMENTS INNER JOIN RECIPES on SEGMENTS.ID_recipe = recipes.id where recipes.recipe_name = '{0}' order by segment_number", RecipeName);

                OracleCommand command = new OracleCommand(queryString, connection);
                connection.Open();
                try
                {

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        for (int i = 0; i < dataTable.Rows.Count; ++i)
                            Segments.Add(new Segment_Details
                            {
                                ID = Convert.ToInt32(dataTable.Rows[i][0]),
                                Segment_Number = dataTable.Rows[i][2].ToString(),
                                RAMP_SP = dataTable.Rows[i][3].ToString(),
                                MIN_RAMP = dataTable.Rows[i][4].ToString(),
                                MAX_RAMP = dataTable.Rows[i][5].ToString(),
                                SOAK_SP = dataTable.Rows[i][6].ToString(),
                                MIN_SOAK = dataTable.Rows[i][7].ToString(),
                                MAX_SOAK = dataTable.Rows[i][8].ToString(),
                                SOAK_TIME = dataTable.Rows[i][9].ToString(),
                                LOW_TEMP_MODE_SP = dataTable.Rows[i][10].ToString(),
                                ALARM_TEMP_TH = dataTable.Rows[i][11].ToString(),
                                LOW_TEMP_MODE_EN = Convert.ToChar(dataTable.Rows[i][12]),

                            });

                        //3-Update Running Segment fields 
                        NbSegments = Segments.Count().ToString();
                        CurrentRunningSegment = Segments[0];
                        RunningNb = CurrentRunningSegment.Segment_Number;
                        RunningRemSegments = (Int32.Parse(NbSegments) - Int32.Parse(RunningNb)).ToString();
                        RunningRampHrUI = Segments[0].MAX_RAMP;
                        RunningRampMinUI = Segments[0].MAX_RAMP;
                        SetPt = Segments[0].RAMP_SP;
                        RunningSoak = Segments[0].SOAK_TIME;

                        RunningSegmentStatus purge = RunningSegmentStatus.Purge;
                        RunningStatusUI = purge.Description();

                        SegmentSummaryStates testEnCours = SegmentSummaryStates.CycleEnCours;
                        TitleMenuStatus = testEnCours.Description();

                        //4-Update Next Segment Fields                        
                        NextNb = (Int32.Parse(RunningNb) + 1).ToString();

                        if (Segments.Count > 1)
                        {
                            NextRampHr = Segments[1].MAX_RAMP;
                            NextRampMin = Segments[1].MIN_RAMP;
                            NextSetPt = Segments[1].RAMP_SP;
                            NextSoak = Segments[1].SOAK_SP;
                        }
                        else
                        {
                            RunningRemSegments = "0";
                            NextNb = "NULL";
                            NextRampHr = "NULL";
                            NextRampMin = "NULL";
                            NextSetPt = "NULL";
                            NextSoak = "NULL";
                        }

                    }

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        public void UploadRecipePLC()
        {
            //upload TC active, 
            //create delta in past mins on 6 values, raise alarm to operator
            _eventAggregator.GetEvent<UpdatedSourceEvent>().Publish(new OPCObject(SetPt, CurrentRunningSegment.RAMP_SP));
            
        }
        public void NextSegment()
        {
         /**************************************************************************************************************
        -> Method     : NextSegment
        -> Event      : "Skip Segmemnt" clicked OR when Segment ends        
        -> Description: This method skips to the next segment. It also updates the properties of the running and next segment 
                        There is an object of type Segment_Details called "CurrentRunningSegment" and it holds all properties 
                        of a segment adn is updated with each new Segment.
        **************************************************************************************************************/
            try
            {
                //If reach Max Segment
                if (Int32.Parse(CurrentRunningSegment.Segment_Number) == Segments.Count - 1 )
                {
                    MessageBox.Show("No More Segments Available");
                    RunningRemSegments = "0";
                    RunningNb = (Int32.Parse(CurrentRunningSegment.Segment_Number) + 1).ToString();
                    NextNb = "NULL";
                    NextRampHr = "NULL";
                    NextRampMin = "NULL";
                    NextSetPt = "NULL";
                    NextSoak = "NULL";
                    return;
                }

                //1-Update Running Segment Properties
                CurrentRunningSegment = Segments[(Int32.Parse(RunningNb))]; //runningNb is 2 lets say, so segment 2. I want to go to segment 3, which is Segments[3-1]=Segments[2]=Segments[RunningNb]
                RunningNb = (Int32.Parse(CurrentRunningSegment.Segment_Number)).ToString();
                RunningRemSegments = (Int32.Parse(NbSegments) - Int32.Parse(RunningNb)).ToString();
                RunningRampHrUI = CurrentRunningSegment.MAX_RAMP;
                RunningRampMinUI = CurrentRunningSegment.MAX_RAMP;
                SetPt = CurrentRunningSegment.RAMP_SP;
                RunningSoak = CurrentRunningSegment.SOAK_TIME;

                //2-Update Next Segment Properties
                NextNb = (Int32.Parse(RunningNb) + 1).ToString();
                NextRampHr = CurrentRunningSegment.MAX_RAMP;
                NextRampMin = CurrentRunningSegment.MIN_RAMP;
                NextSetPt = CurrentRunningSegment.RAMP_SP;
                NextSoak = CurrentRunningSegment.SOAK_SP;

                SegmentSummaryStates testEnAttente = SegmentSummaryStates.EnAttente;
                TitleMenuStatus = testEnAttente.Description();

                //System.Windows.MessageBox.Show(teste);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }
        private void GetCookInfo(Cook cook_obj)
        {
         /**************************************************************************************************************
        -> Method     : GetCookInfo
        -> Param	  : Cook cook_obj
        -> Event      : When Commit Load is clicked. Method triggered by event called CookInfoEvent. The event is published 
                        by PartsLoadPageViewModel      
        -> Description: It passes as a parameter a cook object, which contains the recipe name and batch ID. With this 
                        information, all binding properties on the UI can be updated, such as Recipe Name, Recipe Number,
                        RecipeID, and allows to get all segments details to eventually be able to initialize the recipe.
        **************************************************************************************************************/   

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    //1-Get Batch number from ID
                    string getBatchNumber = string.Format("select batch_number from batch where ID='{0}'", cook_obj.batch_ID);
                    OracleCommand command1 = new OracleCommand(getBatchNumber, connection);
                    connection.Open();
                    using (OracleDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BatchNumber = reader.GetString(0);
                        }
                        reader.Close();
                    }

                    //2-Update recipe Name binding
                    RecipeName = cook_obj.recipe_name;

                    //3-Get recipe number from name
                    string getRecipeNumber = string.Format("select recipe_number from recipes where recipe_name='{0}'", cook_obj.recipe_name);
                    OracleCommand command2 = new OracleCommand(getRecipeNumber, connection);
                    using (OracleDataReader reader = command2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RecipeNumber = reader.GetString(0);
                        }
                        reader.Close();
                    }

                    //4-Get recipe id from recipe number
                    string getRecipeID = string.Format("select id from recipes where recipe_name='{0}'", cook_obj.recipe_name);
                    OracleCommand command3 = new OracleCommand(getRecipeID, connection);
                    using (OracleDataReader reader = command3.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RecipeID = reader.GetInt32(0).ToString();
                        }
                        reader.Close();
                    }


                    ////5-Count number of segments by coutning number of rows in data table
                    //string countRows = String.Format("SELECT recipesegment.id_segment FROM RECIPESEGMENT INNER JOIN segment ON RECIPESEGMENT.ID_segment = segment.id INNER JOIN recipe ON RECIPESEGMENT.ID_recipe = recipe.id where recipe.id = '{0}'", RecipeID);

                    //OracleCommand command4 = new OracleCommand(countRows, connection);
                    //int NbSegmentsINT = 0;


                    //using (OracleDataReader reader = command4.ExecuteReader())

                    //while (reader.Read())
                    //{
                    //    NbSegmentsINT++;
                    //}
                    //NbSegments = NbSegmentsINT.ToString();
                    StartTime = DateTime.Now.ToString("HH:mm:ss tt");
                    NavigateTo("SegmentSummaryPage");
                    InitializeRecipeSegments();
                }

                catch (Exception Ex)
                {
                    MessageBox.Show("Failed to get Recipe Data\n" + Ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void PartsApply(object obj)
        {
         /**************************************************************************************************************
         -> Method     : PartsApply
         -> Param	   : object obj
         -> Event      : This Method is triggered when Apply Changes is clicked       
         -> Description: This method is used to update the values entered in the desired value column in Parts Temp Control
                         In the Hardware class there is a list of all properties that are to be updated when this button is clicked
         **************************************************************************************************************/
            foreach (string hardware in Hardware.PLCApplyChangesTagsList)
            {
                string propValueToUpdate = Hardware.PLCApplyChangesPropertiesList[Hardware.PLCApplyChangesTagsList.IndexOf(hardware)];
                try
                {
                    _eventAggregator.GetEvent<UpdatedSourceEvent>().Publish(new OPCObject(hardware, this[propValueToUpdate].ToString()));
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Unable to write: " + hardware + "\n" + Ex.Message);
                }
            }
        }
        private void GetItemValue(ItemValueCallback obj)
        {
            /**************************************************************************************************************
            -> Method     : PartsApply
            -> Param	  : object obj
            -> Event      : This Method triggered by event called UpdatedValueEvent and is called whenever the PLC updates 
                            a value of a property. The event is published by MainWindowViewModel         
            -> Description: This Method takes in an ItemValueCallBack obj contains a property called "ClientHandle", which 
                            holds the property name and contains another property called "Value". This method then updates 
                            the local context of the property to the new value using the "This" method, which uses reflection.
                            This method could also calculate the TC and TR average.
            **************************************************************************************************************/
            string name = (string) obj.ClientHandle;
            string value = (string) obj.Value;

            if (Hardware.SegmentSummaryTagsList.Contains(name))
            {
                try
                {
                    int index = Hardware.SegmentSummaryTagsList.IndexOf(name);
                    string propToUpdate = Hardware.SegmentSummaryPropertiesList[index];
                    this[propToUpdate] = value;

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Failed to Update: " + name + "\n" + Ex.Message);
                }
            }
            if ((name == "TC_Highest" || name == "TC_Lowest"))
            {
                if (TCLowest != null && TCHighest != null)
                {
                    try
                    {
                        CalculatedTCAvg = ((Int32.Parse(TCHighest) + Int32.Parse(TCLowest)) / 2).ToString();
                    }
                    catch (Exception)
                    {
                        ShowFailedToUpdateMessageBox(name);
                    }

                }
            }
            if ((name == "TR_Highest" || name == "TR_Lowest"))
            {
                if (TRLowest != null && TRHighest != null)
                {
                    try
                    {
                        CalculatedTRAvg = ((Int32.Parse(TRHighest) + Int32.Parse(TRLowest)) / 2).ToString();
                    }
                    catch (Exception)
                    {
                        ShowFailedToUpdateMessageBox(name);
                        //MessageBox.Show(Application.Current.MainWindow, "Failed to Update: " + name);
                    }

                }
            }

            //string RunningDisplayed = "RunningRamp" + RunningNb;
            //RunningRampHrUI = this[RunningDisplayed].ToString();
            //try
            //{
                //RunningRampMinUI = (Int32.Parse(RunningRampHrUI) / 60).ToString();
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show(Application.Current.MainWindow, "Unable to Update Ramp F/Min Value");
           // }

        }
        private void SkipBtn(object parm)
        {
            /**************************************************************************************************************
            -> Method     : SkipBtn
            -> Param	  : object parm
            -> Event      : This method is called when Skip Segment Button is called       
            -> Description: This method is used to skip the current runnning segment in the cook, which calls the NextSegment() method.
            **************************************************************************************************************/


            if (MessageBox.Show(Application.Current.MainWindow, "Skip Segment " + RunningNb + "?", "Segment " + RunningNb, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    NextSegment();
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Unable to write:  RunningNb\n" + Ex.Message);
                }
            }

        }
        private void SetPtAdjBtn(object parm)
        {
            /**************************************************************************************************************
            -> Method     : SetPtAdjBtn
            -> Param	  : object parm
            -> Event      : This method is triggered when the Adjust button is clicked      
            -> Description: This method is used to adjust the Set Point of the current running segment 
            **************************************************************************************************************/

            try
            {
                if (Int32.Parse(OffsetEnteredAdj) >= -20 && Int32.Parse(OffsetEnteredAdj) <= 20)
                {
                    newResult = Int32.Parse(OffsetEnteredAdj) + Int32.Parse(SetPt);
                    if (MessageBox.Show(Application.Current.MainWindow, "Adjust Current Set Point of " + SetPt + "?\nNew SetPoint: " + newResult, "SetPoint", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        _eventAggregator.GetEvent<UpdatedSourceEvent>().Publish(new OPCObject("Set_Pt", newResult.ToString()));
                    }
                }
                else
                {
                    MessageBox.Show(Application.Current.MainWindow, "Please enter value between -20 and 20.");
                }
                OffsetEnteredAdj = "";
            }
            catch (Exception)
            {
                MessageBox.Show(Application.Current.MainWindow, "Error: SetPoint is null");
            }
        }
        private void LowTempBtn(object parm)
        {
        /**************************************************************************************************************
        -> Method     : LowTempBtn
        -> Param	  : object parm
        -> Event      : This method is triggered when Low Temp On/Off button clicked  
        -> Description: This method toggles the low temp mode, not yet implemented
        **************************************************************************************************************/
            if (LowTempEnabled)
            {
                MessageBox.Show(Application.Current.MainWindow, "Hi, Enabled");
                LowTempEnabled = false;
                LowTempLabel = "Low Temp On";
            }
            else
            {
                MessageBox.Show(Application.Current.MainWindow, "Bye, Disabled");
                LowTempEnabled = true;
                LowTempLabel = "Low Temp Off";
            }

        }
        private void StartStopBtn(object parm)
        {
        /**************************************************************************************************************
        -> Method     : StartStopBtn
        -> Param	  : object parm
        -> Event      : This method is triggered when the Start/Stop Button is clicked      
        -> Description: This method starts a timer and alerts the user with a messsage box when the timer is done.
        **************************************************************************************************************/
            if (StartStopEnabled)
            {
                try
                {
                    _time = new TimeSpan((Int32.Parse(HrLabel)), (Int32.Parse(MinLabel)), Int32.Parse(SecLabel));
                }
                catch (Exception)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Please Enter a Valid Number", "PYAC Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                {
                    TimerLabel = _time.ToString("c");
                    if (_time == TimeSpan.Zero)
                    {
                        _timer.Stop();
                    }
                    _time = _time.Add(TimeSpan.FromSeconds(-1));
                    if (TimerLabel.Equals("00:00:00"))
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Timer Done");
                    }
                }, Application.Current.Dispatcher);

                _timer.Start();
                StartStopEnabled = false;
                StartStopLabel = "Stop";
            }
            else
            {
                try
                {
                    _timer.Stop();

                }
                catch (Exception)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Timer Not Started", "PYAC Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                StartStopEnabled = true;
                StartStopLabel = "Start";
            }

        }
        public void ShowFailedToUpdateMessageBox(string name)
        {
        /**************************************************************************************************************
        -> Method     : ShowFailedToUpdateMessageBox
        -> Param	  : string name
        -> Event      : This method is triggered when a special type of message box needs to be shown       
        -> Description: This method creates a message box in a new thread in order to avoid freezing the UI until the
                        message box is dismissed. This method is important, because if a message box is called when the 
                        application is opening up and the OK button is not clicked the application will wait until it is clicked. 
                        This method solves this problem.
        **************************************************************************************************************/

            var thread = new Thread(
              () =>
              {
                  MessageBox.Show("Failed to Update: " + name);
              });
            thread.Start();
        }
        public object this[string propToUpdate]
        {
        /**************************************************************************************************************
        -> Method     : string
        -> Event      : This method is triggered in the GetItemValue Method.       
        -> Description: This method is used whenever we only have the NAME of the property we want to update. It uses 
                        reflection    
        **************************************************************************************************************/
            get
            {
                Type propType = typeof(SegmentSummaryPageViewModel);
                PropertyInfo myPropInfo = propType.GetProperty(propToUpdate);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type propType = typeof(SegmentSummaryPageViewModel);
                PropertyInfo myPropInfo = propType.GetProperty(propToUpdate);
                myPropInfo.SetValue(this, value, null);
            }
        }
        private void NavigateTo(string url)
        {
        /**************************************************************************************************************
        -> Method     : NavigateTo
        -> Param	  : string url
        -> Description: This method is used when a user clicks a button to navigate onto another View.
        **************************************************************************************************************/
            _regionManager.RequestNavigate(Regions.ContentRegion, url);
        }
       
        //PRISM PROPERTIES
        private string _zone1Temp;
        public string Zone1Temp
        {
            get { return _zone1Temp; }
            set { SetProperty(ref _zone1Temp, value); }
        }
        private string _zone2Temp;
        public string Zone2Temp
        {
            get { return _zone2Temp; }
            set { SetProperty(ref _zone2Temp, value); }
        }
        private string _lowTempLabel = "Low Temp On";
        public string LowTempLabel
        {
            get { return _lowTempLabel; }
            set { SetProperty(ref _lowTempLabel, value); }
        }
        private bool _lowTempEnabled;
        public bool LowTempEnabled
        {
            get { return _lowTempEnabled; }
            set { SetProperty(ref _lowTempEnabled, value); }
        }
        private string _startStopLabel = "Start";
        public string StartStopLabel
        {
            get { return _startStopLabel; }
            set { SetProperty(ref _startStopLabel, value); }
        }
        private bool _startStopEnabled = true;
        public bool StartStopEnabled
        {
            get { return _startStopEnabled; }
            set { SetProperty(ref _startStopEnabled, value); }
        }
        private string _timerLabel;
        public string TimerLabel
        {
            get { return _timerLabel; }
            set { SetProperty(ref _timerLabel, value); }
        }
        private string _hrLabel;
        public string HrLabel
        {
            get { return _hrLabel; }
            set { SetProperty(ref _hrLabel, value); }
        }
        private string _minLabel;
        public string MinLabel
        {
            get { return _minLabel; }
            set { SetProperty(ref _minLabel, value); }
        }
        private string _secLabel;
        public string SecLabel
        {
            get { return _secLabel; }
            set { SetProperty(ref _secLabel, value); }
        }
        private string _zone1OP;
        public string Zone1OP
        {
            get { return _zone1OP; }
            set { SetProperty(ref _zone1OP, value); }
        }
        private string _zone2OP;
        public string Zone2OP
        {
            get { return _zone2OP; }
            set { SetProperty(ref _zone2OP, value); }
        }
        private string _zoneCool;
        public string ZoneCool
        {
            get { return _zoneCool; }
            set { SetProperty(ref _zoneCool, value); }
        }
        private string _setPt;
        public string SetPt
        {
            get { return _setPt; }
            set { SetProperty(ref _setPt, value); }
        }
        private string _runningRemSegments;
        public string RunningRemSegments
        {
            get { return _runningRemSegments; }
            set { SetProperty(ref _runningRemSegments, value); }
        }
        private string _runningNb;
        public string RunningNb
        {
            get { return _runningNb; }
            set { SetProperty(ref _runningNb, value); }
        }
        private string _runningSoak;
        public string RunningSoak
        {
            get { return _runningSoak; }
            set { SetProperty(ref _runningSoak, value); }
        }
        private string _nextNb;
        public string NextNb
        {
            get { return _nextNb; }
            set { SetProperty(ref _nextNb, value); }
        }
        private string _tCActive;
        public string TCActive
        {
            get { return _tCActive; }
            set { SetProperty(ref _tCActive, value); }
        }
        private string _tCHighest;
        public string TCHighest
        {
            get { return _tCHighest; }
            set { SetProperty(ref _tCHighest, value); }
        }
        private string _tCLowest;
        public string TCLowest
        {
            get { return _tCLowest; }
            set { SetProperty(ref _tCLowest, value); }
        }
        private string _tCGblAverage;
        public string TCGblAverage
        {
            get { return _tCGblAverage; }
            set { SetProperty(ref _tCGblAverage, value); }
        }
        private string _tRHighest;
        public string TRHighest
        {
            get { return _tRHighest; }
            set { SetProperty(ref _tRHighest, value); }
        }
        private string _tRLowest;
        public string TRLowest
        {
            get { return _tRLowest; }
            set { SetProperty(ref _tRLowest, value); }
        }
        private string _nbSegments;
        public string NbSegments
        {
            get { return _nbSegments; }
            set { SetProperty(ref _nbSegments, value); }
        }
        private string _calculatedTCAvg;
        public string CalculatedTCAvg
        {
            get { return _calculatedTCAvg; }
            set { SetProperty(ref _calculatedTCAvg, value); }
        }
        private string _calculatedTRAvg;
        public string CalculatedTRAvg
        {
            get { return _calculatedTRAvg; }
            set { SetProperty(ref _calculatedTRAvg, value); }
        }
        private string _partsAlarmDelay;
        public string PartsAlarmDelay
        {
            get { return _partsAlarmDelay; }
            set { SetProperty(ref _partsAlarmDelay, value); }
        }
        private string _partsMinSoak;
        public string PartsMinSoak
        {
            get { return _partsMinSoak; }
            set { SetProperty(ref _partsMinSoak, value); }
        }
        private string _partsMaxSoak;
        public string PartsMaxSoak
        {
            get { return _partsMaxSoak; }
            set { SetProperty(ref _partsMaxSoak, value); }
        }
        private string _partsLowTemp;
        public string PartsLowTemp
        {
            get { return _partsLowTemp; }
            set { SetProperty(ref _partsLowTemp, value); }
        }
        private string _partsRampRateMax;
        public string PartsRampRateMax
        {
            get { return _partsRampRateMax; }
            set { SetProperty(ref _partsRampRateMax, value); }
        }
        private string _partsRampRateMin;
        public string PartsRampRateMin
        {
            get { return _partsRampRateMin; }
            set { SetProperty(ref _partsRampRateMin, value); }
        }
        private string _partsTempThresh;
        public string PartsTempThresh
        {
            get { return _partsTempThresh; }
            set { SetProperty(ref _partsTempThresh, value); }
        }
        private string _partsUnsealDoor;
        public string PartsUnsealDoor
        {
            get { return _partsUnsealDoor; }
            set { SetProperty(ref _partsUnsealDoor, value); }
        }
        private string _partsPLCAlarmDelay;
        public string PartsPLCAlarmDelay
        {
            get { return _partsPLCAlarmDelay; }
            set { SetProperty(ref _partsPLCAlarmDelay, value); }
        }
        private string _partsPLCMinSoak;
        public string PartsPLCMinSoak
        {
            get { return _partsPLCMinSoak; }
            set { SetProperty(ref _partsPLCMinSoak, value); }
        }
        private string _partsPLCMaxSoak;
        public string PartsPLCMaxSoak
        {
            get { return _partsPLCMaxSoak; }
            set { SetProperty(ref _partsPLCMaxSoak, value); }
        }
        private string _partsPLCLowTemp;
        public string PartsPLCLowTemp
        {
            get { return _partsPLCLowTemp; }
            set { SetProperty(ref _partsPLCLowTemp, value); }
        }
        private string _partsPLCRampRateMax;
        public string PartsPLCRampRateMax
        {
            get { return _partsPLCRampRateMax; }
            set { SetProperty(ref _partsPLCRampRateMax, value); }
        }
        private string _partsPLCRampRateMin;
        public string PartsPLCRampRateMin
        {
            get { return _partsPLCRampRateMin; }
            set { SetProperty(ref _partsPLCRampRateMin, value); }
        }
        private string _partsPLCTempThresh;
        public string PartsPLCTempThresh
        {
            get { return _partsPLCTempThresh; }
            set { SetProperty(ref _partsPLCTempThresh, value); }
        }
        private string _partsPLCUnsealDoor;
        public string PartsPLCUnsealDoor
        {
            get { return _partsPLCUnsealDoor; }
            set { SetProperty(ref _partsPLCUnsealDoor, value); }
        }
        private string _runningRampHrUI;
        public string RunningRampHrUI
        {
            get { return _runningRampHrUI; }
            set { SetProperty(ref _runningRampHrUI, value); }
        }
        private string _runningRampMinUI;
        public string RunningRampMinUI
        {
            get { return _runningRampMinUI; }
            set { SetProperty(ref _runningRampMinUI, value); }
        }

        private string _offsetEnteredAdj;
        public string OffsetEnteredAdj
        {
            get { return _offsetEnteredAdj; }
            set { SetProperty(ref _offsetEnteredAdj, value); }
        }
        private string _nextRampHr;
        public string NextRampHr
        {
            get { return _nextRampHr; }
            set { SetProperty(ref _nextRampHr, value); }
        }
        private string _nextRampMin;
        public string NextRampMin
        {
            get { return _nextRampMin; }
            set { SetProperty(ref _nextRampMin, value); }
        }
        private string _nextSetPt;
        public string NextSetPt
        {
            get { return _nextSetPt; }
            set { SetProperty(ref _nextSetPt, value); }
        }
        private string _nextSoak;
        public string NextSoak
        {
            get { return _nextSoak; }
            set { SetProperty(ref _nextSoak, value); }
        }
        private string _batchNumber;
        public string BatchNumber
        {
            get { return _batchNumber; }
            set { SetProperty(ref _batchNumber, value); }
        }
        private string _recipeNumber;
        public string RecipeNumber
        {
            get { return _recipeNumber; }
            set { SetProperty(ref _recipeNumber, value); }
        }
        private string _recipeName;
        public string RecipeName
        {
            get { return _recipeName; }
            set { SetProperty(ref _recipeName, value); }
        }
        private string _startTime;
        public string StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }
        private string _endTime;
        public string EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, value); }
        }
        private string _duration;
        public string Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }
        private string _titleMenuStatus;
        public string TitleMenuStatus
        {
            get { return _titleMenuStatus; }
            set { SetProperty(ref _titleMenuStatus, value); }
        }
        private string _runningStatusUI;
        public string RunningStatusUI
        {
            get { return _runningStatusUI; }
            set { SetProperty(ref _runningStatusUI, value); }
        }  

        //OTHER PROPERTIES
        private SegmentSummaryStates _segmentSummaryStates;
        public SegmentSummaryStates SegmentSummaryState
        {
            get { return _segmentSummaryStates; }
            set { SetProperty(ref _segmentSummaryStates, value); }
        }

        private SegmentSummaryStates _runningSummaryStates;
        public SegmentSummaryStates RunningSummaryStates
        {
            get { return _runningSummaryStates; }
            set { SetProperty(ref _segmentSummaryStates, value); }
        }

        private ObservableCollection<Segment_Details> _segments = new ObservableCollection<Segment_Details>();
        public ObservableCollection<Segment_Details> Segments
        {
            get { return _segments; }
            set { _segments = value; }
        }

        public string RecipeID { get; private set; }


        private Segment_Details _currentRunningSegment;
        public Segment_Details CurrentRunningSegment
        {
            get
            {
                return _currentRunningSegment;
            }
            set
            {
                try
                {
                    SetProperty(ref _currentRunningSegment, value);
                }
                catch (Exception)
                {

                }
            }
        }

    }
}

