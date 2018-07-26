using Kepware.ClientAce.OpcDaClient;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Events;
using PYAC.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PYAC.ViewModels
{
    public class SegmentParameterPageViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;
        static bool isInstantiated;

        public SegmentParameterPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<UpdatedValueEvent>().Subscribe(GetItemValue);

            if (!isInstantiated)
            {
                isInstantiated = true;

                eventAggregator.GetEvent<IsSubscriptionCompleted>().Publish(true);
            }

        }
        private void GetItemValue(ItemValueCallback obj)
        {
            string name = (string) obj.ClientHandle;
            string value = (string) obj.Value;

            if (Hardware.SegmentParameterTagsList.Contains(name))
            {
                try
                {
                    int index = Hardware.SegmentParameterTagsList.IndexOf(name);
                    string propToUpdate = Hardware.SegmentParameterPropertiesList[index];

                    this[propToUpdate] = value;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, "Failed to Update" + name + "\n" + ex.ToString());
                }
            }

        }
        public object this[string propToUpdate]
        {
            get
            {
                Type propType = typeof(SegmentParameterPageViewModel);
                PropertyInfo myPropInfo = propType.GetProperty(propToUpdate);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type propType = typeof(SegmentParameterPageViewModel);
                PropertyInfo myPropInfo = propType.GetProperty(propToUpdate);
                myPropInfo.SetValue(this, value, null);
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
        private string _nbSegments;
        public string NbSegments
        {
            get { return _nbSegments; }
            set { SetProperty(ref _nbSegments, value); }
        }

    }

}
