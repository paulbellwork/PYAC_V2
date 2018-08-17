using MahApps.Metro.Controls;
using Prism.Events;
using Prism.Mvvm;
using PYAC.Events;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PYAC.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
        
    {
       
        public MainWindow()
        {
            InitializeComponent();

            //enum test
            SegmentSummaryStates test = SegmentSummaryStates.CycleEnCours;
            string teste = test.Description();
            //System.Windows.MessageBox.Show(teste);

        }

       

    }
    
}
