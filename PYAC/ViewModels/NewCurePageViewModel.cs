using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Events;
using PYAC.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PYAC.ViewModels
{
    class NewCurePageViewModel : BindableBase
    {

        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;

        public DelegateCommand<object> ChangeTypeCommand { get; set; }
        public DelegateCommand<object> AddNewCureCommand { get; set; }
        public ICommand NavigateToPartsLoadPageCommand { get; private set; }

        public NewCurePageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            NavigateToPartsLoadPageCommand = new DelegateCommand(() => NavigateTo("PartsLoadPage"));

            ChangeTypeCommand = new DelegateCommand<object>(ChangeType);
            AddNewCureCommand = new DelegateCommand<object>(AddCure);
        }

        private void AddCure(object obj)
        {
            bool isNewCure;

            if (IsNewCureChecked == true && IsRestartPartsChecked == false)
            {
                isNewCure = true;
                //IsCureInProgress = true;
            }
            else 
            {
                isNewCure = false;
            }

            _eventAggregator.GetEvent<SendNewCureInfoEvent>().Publish(new InfoToSendCure(isNewCure, Batch_Number));
        }

        private void ChangeType(object obj)
        {
            if(obj.ToString() == "createCure")
            {
                IsNewCureChecked = true;
                IsRestartPartsChecked = false;
            }
            else
            {
                IsNewCureChecked = false;
                IsRestartPartsChecked = true;
            }
        }

        private void NavigateTo(string url)
        {
            _regionManager.RequestNavigate(Regions.ContentRegion, url);
        }

        private bool _isNewCureChecked;
        public bool IsNewCureChecked
        {
            get { return _isNewCureChecked; }
            set
            {
                SetProperty(ref _isNewCureChecked, value);
            }
        }
        private bool _isRestartPartsChecked;
        public bool IsRestartPartsChecked
        {
            get { return _isRestartPartsChecked; }
            set
            {
                SetProperty(ref _isRestartPartsChecked, value);
            }
        }
        private string _batch_Number;
        public string Batch_Number
        {
            get { return _batch_Number; }
            set{SetProperty(ref _batch_Number, value);}
        }
        //private bool _isCureInProgress;
        //public bool IsCureInProgress
        //{
        //    get { return _isCureInProgress; }
        //    set { SetProperty(ref _isCureInProgress, value); }
        //}
      
    }

    public class Batch_Details
    {
        public string Batch_Number;
    }
}
