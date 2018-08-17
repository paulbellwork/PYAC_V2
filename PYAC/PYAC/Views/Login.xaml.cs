using MahApps.Metro.Controls;
using Prism.Events;
using Prism.Regions;
using PYAC.Events;
using PYAC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PYAC.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : MetroWindow

    {

        protected readonly IEventAggregator _eventAggregator;

        public Login()
        {
            InitializeComponent();
        }

        private void move(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
{
    if (this.DataContext != null)
    { ((dynamic)this.DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }
}


        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if ((UN.Text == "admin") && (PW.Password == ""))
        //    {
        //        Application.Current.MainWindow.Show();
        //        this.Close();
        //        //_eventAggregator.GetEvent<Zone1TempEvent>().Publish(true);
                

        //    }
        //    else
        //    {
        //        MessageBox.Show("Incorrect UserName or Password");
        //    }
        //}
 

        }
}
