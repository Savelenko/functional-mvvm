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

        private static readonly StampDisplayOrder SortedValueHighToLow =
            StampDisplayOrder.NewSorted(StampSortedDisplayOrder.ByValueDescending);

        private static readonly StampDisplayOrder SortedValueLowToHigh =
            StampDisplayOrder.NewSorted(StampSortedDisplayOrder.ByValueAscending);

        private static readonly StampDisplayOrder NotSorted = StampDisplayOrder.Unsorted;

        public MainWindow(StampCollectionViewModel initialViewModel, ApplicationModel applicationModel)
        {
            InitializeComponent();

            this.applicationModel = applicationModel;

            this.Render(initialViewModel);

            this.applicationModel.ViewModel.Subscribe(this.Render);
        }

        private void Render(StampCollectionViewModel viewModel)
        {
            this.DataContext = viewModel;

            /* Some things are easier to make not using WPF data binding. The following fragment displays current
             * stamp display order using the corresponding radio buttons. */
             // { none of the radio buttons is checked }, by intention
            if (viewModel.DisplayOrder.Equals(SortedValueHighToLow))
            {
                this.rbSortHighToLow.IsChecked = true;
            }
            else if (viewModel.DisplayOrder.Equals(SortedValueLowToHigh))
            {
                this.rbSortLowToHigh.IsChecked = true;
            }
            else if (viewModel.DisplayOrder.Equals(NotSorted))
            {
                this.rbSortNone.IsChecked = true;
            }
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

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;

            var checkedNow = rb.IsChecked.GetValueOrDefault(false);
            rb.IsChecked = !checkedNow;

            var message =
                rb == this.rbSortHighToLow ? Message.NewSortStamps(StampSortedDisplayOrder.ByValueDescending) :
                rb == this.rbSortLowToHigh ? Message.NewSortStamps(StampSortedDisplayOrder.ByValueAscending) :
                rb == this.rbSortNone ? Message.RemoveStampSorting :
                this.Throw<Message>(new ArgumentException("Event handler attached to wrong element."));

            this.applicationModel.SendMessage(message);
        }

        private void ButtonCheckRarenessClick(object sender, RoutedEventArgs e)
        {
            var stamp = (sender as Button)?.DataContext as StampViewModel;
            if (stamp == null)
            {
                throw new ArgumentException("Event handler attached to wrong element.");
            }

            var message = Message.NewValidateRareness(stamp.Description, stamp.Value);

            this.applicationModel.SendMessage(message);
        }
    }
}
