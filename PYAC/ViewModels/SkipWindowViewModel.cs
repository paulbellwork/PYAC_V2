using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PYAC.ViewModels
{
    public class SkipWindowViewModel : BindableBase
    {
        public DelegateCommand<Window> SkipWindowYesCloseWindowCommand { get; }
        public DelegateCommand<Window> SkipWindowNoCloseWindowCommand { get; }

        public SkipWindowViewModel()
        {
            SkipWindowYesCloseWindowCommand = new DelegateCommand<Window>(YesCloseWindowCommand);
            SkipWindowNoCloseWindowCommand = new DelegateCommand<Window>(NoCloseWindowCommand);
        }

        private void NoCloseWindowCommand(Window obj)
        {
            //do stuff to skip
            obj.Close();
        }

        private void YesCloseWindowCommand(Window obj)
        {
            obj.Close();
        }


    }
}
