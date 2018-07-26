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
    class AddPartPageViewModel : BindableBase
    {
        public ICommand NavigateToPartsLoadPageCommand { get; private set; }

        private readonly IRegionManager _regionManager;
        protected readonly IEventAggregator _eventAggregator;
        public DelegateCommand<object> AddPartCommand { get; set; }


        public AddPartPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            NavigateToPartsLoadPageCommand = new DelegateCommand(() => NavigateTo("PartsLoadPage"));
            AddPartCommand = new DelegateCommand<object>(AddPart);
        }

        private void AddPart(object obj)
        {
            _eventAggregator.GetEvent<SendAddPartInfoEvent>().Publish(new InfoToSendParts(PartNumber, TravellerNumber, Operation));
        }

        private void NavigateTo(string url)
        {
            _regionManager.RequestNavigate(Regions.ContentRegion, url);
        }

        private string _partNumber;
        public string PartNumber
        {
            get { return _partNumber; }
            set { SetProperty(ref _partNumber, value); }
        }
        private string _travellerNumber;
        public string TravellerNumber
        {
            get { return _travellerNumber; }
            set { SetProperty(ref _travellerNumber, value); }
        }
        private string _operation;
        public string Operation
        {
            get { return _operation; }
            set { SetProperty(ref _operation, value); }
        }
    }
}
