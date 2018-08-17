using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using PYAC.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PYAC.ViewModels
{
    class TitleMenuViewModel : BindableBase
    {

        protected readonly IEventAggregator _eventAggregator;

        public TitleMenuViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            startClock();
            ChangeUsernameEverySecond();
            LogoutEvery10Minutes();
            eventAggregator.GetEvent<UpdateTitleMenuBarEvent>().Subscribe(UpdateTitleMenuBar);
            eventAggregator.GetEvent<LoginUsernameEvent>().Subscribe(GetUsername);

        }


        private void GetUsername(string obj)
        {
            Username = obj;
        }


        private void UpdateTitleMenuBar(string obj)
        {
            TitleMenuLabel = obj;
        }

        private void startClock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tickevent;
            timer.Start();
        }

        private void tickevent(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            TimeLabel = DateTime.Now.ToString("HH:mm:ss tt");
            //dateText.Text = DateTime.Now.ToString("dddd dd MMMM");
            DateLabel = DateTime.Now.ToString("dddd, MMMM d, yyyy");
        }


        public async Task ChangeUsernameEverySecond()
     {
            while (true)
            {
                if (LoginViewModel.IsLoggedIn)
                {
                    Username = LoginViewModel._username;
                }
                else
                {
                    Username = "Logged Off";
                }
                await Task.Delay(1000);
            }
        }
        public async Task LogoutEvery10Minutes()
        {
            while (true)
            {
                LoginViewModel.IsLoggedIn = false;
                Username = "Logged Off";
                await Task.Delay(600000);
            }
        }
        //PROPERTIES
        private string _dateLabel;
        public string DateLabel
        {
            get { return _dateLabel; }
            set { SetProperty(ref _dateLabel, value); }
        }
        private string _timeLabel;
        public string TimeLabel
        {
            get { return _timeLabel; }
            set { SetProperty(ref _timeLabel, value); }
        }
        private string _titleMenuLabel = "Segment Summary";
        public string TitleMenuLabel
        {
            get { return _titleMenuLabel; }
            set { SetProperty(ref _titleMenuLabel, value); }

        }

        public static string _username = "Logged Off";
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }


    }
}
