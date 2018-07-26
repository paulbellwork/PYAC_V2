using Prism.Commands;
using Prism.Mvvm;
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


        public TitleMenuViewModel()
        {
            startClock();
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
    }
}
