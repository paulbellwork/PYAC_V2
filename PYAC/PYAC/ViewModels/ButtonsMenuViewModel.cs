using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using PYAC.Events;
using PYAC.Infrastructure;
using PYAC.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PYAC.ViewModels
{
    class ButtonsMenuViewModel : BindableBase

    {
        protected readonly IEventAggregator _eventAggregator;
        public ICommand NavigateToSegmentSummaryPageCommand { get; private set; }
        public ICommand NavigateToPartsLoadPageCommand { get; private set; }
        public ICommand NavigateToStartupPageCommand { get; private set; }
        public ICommand OpenShutdownWindowCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public ICommand NavigateToRecipePageCommand { get; private set; }
        ShutdownWindow shut;

        private readonly IRegionManager _regionManager;
        public ButtonsMenuViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            NavigateToSegmentSummaryPageCommand = new DelegateCommand(() =>
            {
                NavigateTo("SegmentSummaryPage");
                _eventAggregator.GetEvent<UpdateTitleMenuBarEvent>().Publish("Segment Summary");
            });
            NavigateToPartsLoadPageCommand = new DelegateCommand(() =>
            {
                NavigateTo("PartsLoadPage");
                _eventAggregator.GetEvent<UpdateTitleMenuBarEvent>().Publish("Parts Load");
            });
            NavigateToStartupPageCommand = new DelegateCommand(() =>
            {
                NavigateTo("StartupPage");
                _eventAggregator.GetEvent<UpdateTitleMenuBarEvent>().Publish("Startup");
            });
            NavigateToRecipePageCommand = new DelegateCommand(() =>
            {
                NavigateTo("RecipePage");
                _eventAggregator.GetEvent<UpdateTitleMenuBarEvent>().Publish("Recipes");
            });
            OpenShutdownWindowCommand = new DelegateCommand(Shutdown);
            LogoutCommand = new DelegateCommand(Logout);

        }


        private void Logout()
        {
            //IF USER NOT SIGNED IN, ASK TO SIGN IN
            if(TitleMenuViewModel._username.Equals("Logged Off"))
            {
                var login = new Login();
                var loginVM = new LoginViewModel();

                loginVM.LoginCompleted += (sender, args) =>
                {
                    Application.Current.MainWindow.Show();
                    Application.Current.MainWindow.Activate();
                    LoginViewModel.IsLoggedIn = true;

                    login.Close();

                };
                login.DataContext = loginVM;
                login.ShowDialog();
                return;
            }

            //IF USER IS SIGNED IN, SIGN USER OUT
            if (MessageBox.Show("Log Off?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (!TitleMenuViewModel._username.Equals("Logged Off"))
                {
                    LoginViewModel.IsLoggedIn = false;
                }
            }

            //Application.Current.MainWindow.Hide();

                
             
        }

        private void Shutdown()
        {
                shut = new ShutdownWindow();
                shut.ShowDialog();
        }
        private void NavigateTo(string url)
        {
            _regionManager.RequestNavigate(Regions.ContentRegion, url);
        }

        
        //private string _logoutButtonContent = "Log Off";
        //public string LogoutButtonContent
        //{
        //    get { return _logoutButtonContent; }
        //    set { SetProperty(ref _logoutButtonContent, value); }
        //}
    }
}
