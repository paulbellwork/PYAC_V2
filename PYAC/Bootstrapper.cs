using Autofac;
using Prism.Autofac;
using Prism.Events;
using PYAC.ViewModels;
using PYAC.Views;
using PYAC.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Mvvm;

namespace PYAC
{
    class Bootstrapper : AutofacBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
        public override void Run(bool runWithDefaultConfiguration)

        { 
            base.Run(true);
        }
        protected override void InitializeShell()
        {
            var login = new Login();
            var loginVM = new LoginViewModel();

            //loginVM.LoginCompleted += (sender, args) =>
            //{
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.Activate();

            //    login.Close();

            //};
            //login.DataContext = loginVM;
            //login.ShowDialog();
            //login.Activate();
        }

        protected override void ConfigureContainerBuilder(ContainerBuilder builder)
        {
            base.ConfigureContainerBuilder(builder);
            builder.RegisterTypeForNavigation<TitleMenu>("TitleMenu");
            //builder.RegisterTypeForNavigation<TabsMenu>("TabsMenu");
            builder.RegisterTypeForNavigation<ButtonsMenu>("ButtonsMenu");
            builder.RegisterTypeForNavigation<MainPage>("MainPage");
            builder.RegisterTypeForNavigation<SegmentSummaryPage>("SegmentSummaryPage");
            builder.RegisterTypeForNavigation<PartsLoadPage>("PartsLoadPage");
            builder.RegisterTypeForNavigation<AddPartPage>("AddPartPage");
            builder.RegisterTypeForNavigation<NewCurePage>("NewCurePage");
            builder.RegisterTypeForNavigation<AddRecipePage>("AddRecipePage");
            builder.RegisterTypeForNavigation<StartupPage>("StartupPage");
            builder.RegisterTypeForNavigation<SegmentParameterPage>("SegmentParameterPage");
            builder.RegisterTypeForNavigation<RecipePage>("RecipePage");
        }
    }
}
