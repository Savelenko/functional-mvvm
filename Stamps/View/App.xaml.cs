using Stamps.DataAccess;
using System.Windows;

namespace Stamps.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {            
            var stampService = new WorldStampKnowledgeBaseService();
            var initialModel = Stamps.Model.Model.emptyCollection(stampService);
            var initialViewModel = Stamps.ViewModel.ViewModel.initialViewModel(initialModel);

            var application = new ApplicationModel(initialModel, initialViewModel);
            var mainWindow = new MainWindow(initialViewModel, application);
            mainWindow.Show();
        }
    }
}
