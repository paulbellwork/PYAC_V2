using Prism.Events;
using Prism.Mvvm;
using PYAC.Events;
using PYAC.ViewModels;
using PYAC.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PYAC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        //public App(IEventAggregator eventAggregator)
        //{
        //    eventAggregator.GetEvent<LoginEvent>().Subscribe(LoggedIn);
        //}


        //private void LoggedIn(bool result)
        //{
        //    IsLoggedIn = result;
        //}

        public App()
        {
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //var login = new Login();
            //var loginVM = new LoginViewModel();

            //loginVM.LoginCompleted += (sender, args) =>
            //{
                var bootstrapper = new Bootstrapper();
                bootstrapper.Run();

            //    login.Close();
            //};
            //login.DataContext = loginVM;
            //login.ShowDialog();
        }


    }




        
        

    }
