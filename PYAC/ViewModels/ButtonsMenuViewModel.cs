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

            NavigateToSegmentSummaryPageCommand = new DelegateCommand(() => NavigateTo("SegmentSummaryPage"));
            NavigateToPartsLoadPageCommand = new DelegateCommand(() => NavigateTo("PartsLoadPage"));
            NavigateToStartupPageCommand = new DelegateCommand(() => NavigateTo("StartupPage"));
            NavigateToRecipePageCommand = new DelegateCommand(() => NavigateTo("RecipePage"));
            OpenShutdownWindowCommand = new DelegateCommand(Shutdown);
            LogoutCommand = new DelegateCommand(Logout);

        }


        private void Logout()
        {
            if (MessageBox.Show("Log Off?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Application.Current.MainWindow.Hide();
                var login = new Login();
                var loginVM = new LoginViewModel();

                loginVM.LoginCompleted += (sender, args) =>
                {
                    Application.Current.MainWindow.Show();
                    Application.Current.MainWindow.Activate();

                    login.Close();

                };
                login.DataContext = loginVM;
                //loading.Close();
                login.ShowDialog();
            }    
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
        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{

        //        foreach (Window w in Application.Current.Windows)
        //        {
        //            if (w is MainWindow)
        //            {
        //                w.Close();
        //                Login log = new Login();
        //                log.Show();
        //            }
        //        } 
        //}
    }
}
