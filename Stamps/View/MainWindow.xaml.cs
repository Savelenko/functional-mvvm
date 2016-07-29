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
using Stamps.ViewModel;
using Stamps.Messaging;

namespace Stamps.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApplicationModel applicationModel;

        public MainWindow(StampCollectionViewModel initialViewModel, ApplicationModel applicationModel)
        {
            InitializeComponent();

            this.applicationModel = applicationModel;

            this.DataContext = initialViewModel;

            this.applicationModel.ViewModel.Subscribe(this.Render);
        }

        private void Render(StampCollectionViewModel viewModel)
        {
            this.DataContext = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var description = this.newStampDescription.Text;

            uint value;
            if (UInt32.TryParse(this.newStampValue.Text, out value))
            {
                this.applicationModel.SendMessage(Message.NewAddStamp(description, value));
            }
            else
            {
                this.newStampValue.Text = "!!";
            }
                
        }
    }
}
