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

namespace PYAC.ViewModels
{
    class LoginViewModel : BindableBase
    {

        public DelegateCommand<object> LoginCommand { get; set; }
        public DelegateCommand<object> ExitCommand { get; set; }


        public LoginViewModel()
        {
            LoginCommand = new DelegateCommand<object>(ExecuteLogin);
            ExitCommand = new DelegateCommand<object>(Quit);
        }

        private void Quit(object obj)
        {
            System.Windows.Application.Current.Shutdown();
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
                if (Username.Equals("admin") && Password.Equals("hi"))
                {
                    RaiseLoginCompletedEvent();
                }
                else
                {
                    MessageBox.Show("Username or Password is Incorrect");
                }
        }



        public event EventHandler LoginCompleted;
        private void RaiseLoginCompletedEvent()
        {
            var handler = LoginCompleted;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }


        private string _username = "Username";

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public string Password { private get; set; }
        //public SecureString SecurePassword { private get; set; }
  
        }

}