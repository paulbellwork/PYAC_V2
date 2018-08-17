using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PYAC.ViewModels
{
    public class ShutdownWindowViewModel : BindableBase
    {
        public DelegateCommand<Window> ShutdownCloseCommand { get; }
        public DelegateCommand<Window> ShutdownCancelCommand { get; }


        public ShutdownWindowViewModel()
        {

            ShutdownCloseCommand = new DelegateCommand<Window>(CloseWindowCommand);
            ShutdownCancelCommand = new DelegateCommand<Window>(CancelWindowCommand);
        }
        private void CloseWindowCommand(Window obj)
        {
            Environment.Exit(0);
        }

        private void CancelWindowCommand(Window obj)
        {
            obj.Close();
        }
    }
}
