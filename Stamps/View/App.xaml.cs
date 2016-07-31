using Stamps.DataAccess;
using System.Windows;

namespace Stamps.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// This serves as the main function of the complete application. It creates initial state, consisting of the
        /// initial model and view model, and ties together the view and the dispatching function through an
        /// <see cref="ApplicationModel"/> object. All dependencies of the model, like our small data access layer,
        /// are also initialized here and passed to the model. I like the term "composition root" for describing this
        /// place in the system.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {            
            // Create the initial model and view model.
            var stampService = new WorldStampKnowledgeBaseService();
            var initialModel = Stamps.Model.Model.smallCollection(stampService);
            var initialViewModel = Stamps.ViewModel.ViewModel.initialViewModel(initialModel);

            var application = new ApplicationModel(initialModel, initialViewModel);
            var mainWindow = new MainWindow(initialViewModel, application);
            mainWindow.Show();
        }
    }
}
