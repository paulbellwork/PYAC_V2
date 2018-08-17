using Prism.Events;
using Prism.Mvvm;
using PYAC.Events;
using System.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using System.Security;
using System.Windows.Input;
using System.Windows.Controls;
using Prism.Regions;

namespace PYAC.ViewModels
{
    class LoginViewModel : BindableBase
    {
        public DelegateCommand<object> LoginCommand { get; set; }
        public DelegateCommand<object> ExitCommand { get; set; }
        public DelegateCommand<object> CloseLoginCommand { get; set; }


        public LoginViewModel()
        {
            LoginCommand = new DelegateCommand<object>(ExecuteLogin);
            ExitCommand = new DelegateCommand<object>(Quit);
            CloseLoginCommand = new DelegateCommand<object>(CloseLogin);
        }

        private void CloseLogin(object obj)
        {
            try
            {
                Window login = (Window)obj;
                login.Close();
            }
            catch
            {

            }

        }

        private void Quit(object obj)
        {
            Environment.Exit(0);
        }

        void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;

            if (passwordBox != null)
            {
                Password = passwordBox.Password;
            }  
            ValidateLoginDetails();
        }
        public void ValidateLoginDetails()
        {
                if (Username.Equals("Admin3") && Password.Equals("hi"))
                {
                    RaiseLoginCompletedEvent(Username);
                    IsLoggedIn = true;
                    //_eventAggregator.GetEvent<UpdateLoginUsernameEvent>().Publish(Username);
                    //TitleMenuViewModel.ChangeUsername();
                }
                else
                {
                    MessageBox.Show("Username or Password is Incorrect");
                }
        }
        public static bool IsLoggedIn { get; set; }


        public event EventHandler LoginCompleted;
        private void RaiseLoginCompletedEvent(string user)
        {
            var handler = LoginCompleted;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }


        public static string _username = "Username";

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public string Password { private get; set; }
        //public SecureString SecurePassword { private get; set; }
  
        }

}