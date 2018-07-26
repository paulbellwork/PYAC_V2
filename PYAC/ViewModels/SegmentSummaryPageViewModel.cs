using Kepware.ClientAce.OpcDaClient;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Events;
using PYAC.Infrastructure;
using PYAC.StateMachineV2;
using PYAC.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PYAC.ViewModels
{
    class SegmentSummaryPageViewModel : BindableBase
    {
        DispatcherTimer _timer;
        TimeSpan _time;
        public ICommand NavigateToSegmentParameterPageCommand { get; private set; }
        public DelegateCommand<object> SetPtAdjCommand { get; set; }
        public DelegateCommand<object> SkipCommand { get; set; }
        public DelegateCommand<object> LowTempCommand { get; set; }
        public DelegateCommand<object> StartStopCommand { get; set; }
        public DelegateCommand<object> PartsApplyCommand { get; set; }
        private int newResult = 0;

        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;
        static bool isInstantiated;

        StateMachineV2.StateMachineV2 test;

        public SegmentSummaryPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            eventAggregator.GetEvent<UpdatedValueEvent>().Subscribe(GetItemValue);

            if (!isInstantiated)
            {
                isInstantiated = true;

                eventAggregator.GetEvent<IsSubscriptionCompleted>().Publish(true);
            }

            NavigateToSegmentParameterPageCommand = new DelegateCommand(() => NavigateTo("SegmentParameterPage"));


            SetPtAdjCommand = new DelegateCommand<object>(SetPtAdjBtn);
            SkipCommand = new DelegateCommand<object>(SkipBtn);
            LowTempCommand = new DelegateCommand<object>(lowTempBtn);
            StartStopCommand = new DelegateCommand<object>(StartStopBtn);
            PartsApplyCommand = new DelegateCommand<object>(partsApply);

            test = new StateMachineV2.StateMachineV2();
        }


        private void partsApply(object obj)
        {
            foreach (string hardware in Hardware.PLCApplyChangesTagsList)
            {
                string propValueToUpdate = Hardware.PLCApplyChangesPropertiesList[Hardware.PLCApplyChangesTagsList.IndexOf(hardware)];
                try
                {
                    _eventAggregator.GetEvent<UpdatedSourceEvent>().Publish(new OPCObject(hardware, this[propValueToUpdate].ToString()));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Unable to write: " + hardware + "\n" + ex.ToString());
                }
            }
        }

        private void GetItemValue(ItemValueCallback obj)
        {
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
                catch (Exception ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Failed to Update: " + name + "\n" + ex.ToString());
                }
            }
            if ((name == "TC_Highest" || name == "TC_Lowest"))
            {
                if (TCLowest != null && TCLowest != null)
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
                if (TRLowest != null && TRLowest != null)
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

            string RunningDisplayed = "RunningRamp" + RunningNb;
            RunningRampHrUI = this[RunningDisplayed].ToString();
            try
            {
                RunningRampMinUI = (Int32.Parse(RunningRampHrUI) / 60).ToString();
            }
            catch (Exception)
            {
                MessageBox.Show(Application.Current.MainWindow, "Unable to Update Ramp F/Min Value");
            }

        }
        public void ShowFailedToUpdateMessageBox(string name)
        {
            //this.Invoke(new Action(() => { MessageBox.Show(this, "text"); }));

            var thread = new Thread(
              () =>
              {
                  MessageBox.Show("Failed to Update: " + name);
              });
            thread.Start();
        }
        public object this[string propToUpdate]
        {
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


        private void SkipBtn(object parm)
        {
            //once state machine initialized, the state should be in idle.
            //could call currentState() and moveNext()
            test.MoveNext(Command.NextSegment);

            if (MessageBox.Show(Application.Current.MainWindow, "Skip Segment " + RunningNb + "?", "Segment " + RunningNb, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    if (Int32.Parse(RunningNb) < 8)
                    {
                        RunningNb = (Int32.Parse(RunningNb) + 1).ToString();
                        _eventAggregator.GetEvent<UpdatedSourceEvent>().Publish(new OPCObject("Running_Nb", RunningNb));
                    }
                    else
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Unable to Skip Segment " + RunningNb);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Unable to write:  RunningNb\n" + ex.ToString());
                }
            }

        }

        private void SetPtAdjBtn(object parm)
        {

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


        private void lowTempBtn(object parm)
        {

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
        private string _runningRamp1;
        public string RunningRamp1
        {
            get { return _runningRamp1; }
            set { SetProperty(ref _runningRamp1, value); }
        }
        private string _runningRamp2;
        public string RunningRamp2
        {
            get { return _runningRamp2; }
            set { SetProperty(ref _runningRamp2, value); }
        }
        private string _runningRamp3;
        public string RunningRamp3
        {
            get { return _runningRamp3; }
            set { SetProperty(ref _runningRamp3, value); }
        }
        private string _runningRamp4;
        public string RunningRamp4
        {
            get { return _runningRamp4; }
            set { SetProperty(ref _runningRamp4, value); }
        }
        private string _runningRamp5;
        public string RunningRamp5
        {
            get { return _runningRamp5; }
            set { SetProperty(ref _runningRamp5, value); }
        }
        private string _runningRamp6;
        public string RunningRamp6
        {
            get { return _runningRamp6; }
            set { SetProperty(ref _runningRamp6, value); }
        }
        private string _runningRamp7;
        public string RunningRamp7
        {
            get { return _runningRamp7; }
            set { SetProperty(ref _runningRamp7, value); }
        }
        private string _runningRamp8;
        public string RunningRamp8
        {
            get { return _runningRamp8; }
            set { SetProperty(ref _runningRamp8, value); }
        }
        private string _runningRampMin1;
        public string RunningRampMin1
        {
            get { return _runningRampMin1; }
            set { SetProperty(ref _runningRampMin1, value); }
        }
        private string _runningRampMin2;
        public string RunningRampMin2
        {
            get { return _runningRampMin2; }
            set { SetProperty(ref _runningRampMin2, value); }
        }
        private string _runningRampMin3;
        public string RunningRampMin3
        {
            get { return _runningRampMin3; }
            set { SetProperty(ref _runningRampMin3, value); }
        }
        private string _runningRampMin4;
        public string RunningRampMin4
        {
            get { return _runningRampMin4; }
            set { SetProperty(ref _runningRampMin4, value); }
        }
        private string _runningRampMin5;
        public string RunningRampMin5
        {
            get { return _runningRampMin5; }
            set { SetProperty(ref _runningRampMin5, value); }
        }
        private string _runningRampMin6;
        public string RunningRampMin6
        {
            get { return _runningRampMin6; }
            set { SetProperty(ref _runningRampMin6, value); }
        }
        private string _runningRampMin7;
        public string RunningRampMin7
        {
            get { return _runningRampMin7; }
            set { SetProperty(ref _runningRampMin7, value); }
        }
        private string _runningRampMin8;
        public string RunningRampMin8
        {
            get { return _runningRampMin8; }
            set { SetProperty(ref _runningRampMin8, value); }
        }
        private string _offsetEnteredAdj;
        public string OffsetEnteredAdj
        {
            get { return _offsetEnteredAdj; }
            set { SetProperty(ref _offsetEnteredAdj, value); }
        }


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

        private void NavigateTo(string url)
        {
            _regionManager.RequestNavigate(Regions.ContentRegion, url);
        }
    }
}
