using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PYAC.ViewModels
{
    class TabsMenuViewModel 
    {
        

        public ICommand NavigateToMainPageCommand { get; private set; }
        public ICommand NavigateToSegmentSummaryPageCommand { get; private set; }

        private readonly IRegionManager _regionManager;
        public TabsMenuViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateToMainPageCommand = new DelegateCommand(() => NavigateTo("MainPage"));
            NavigateToSegmentSummaryPageCommand = new DelegateCommand(() => NavigateTo("SegmentSummaryPage"));
        }
        private void NavigateTo(string url)
        {
            _regionManager.RequestNavigate(Regions.ContentRegion, url);
        }

    }
}
