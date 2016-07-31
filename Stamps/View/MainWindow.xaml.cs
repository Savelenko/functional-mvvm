using System;
using System.Windows;
using System.Windows.Controls;
using Stamps.Messaging;
using Stamps.ViewModel;

namespace Stamps.View
{
    /// <summary>
    /// The main type of the view layer. This WPF code-behind component translates GUI events to messages and renders
    /// the view model when it "changes".
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// We need a reference to <see cref="ApplicationModel"/> for accessing
        /// <see cref="ApplicationModel.SendMessage(Message)"/>.
        /// </summary>
        private readonly ApplicationModel applicationModel;

        /* The following three helper values are used for rendering the state of stamp display order. */

        private static readonly StampDisplayOrder SortedValueHighToLow =
            StampDisplayOrder.NewSorted(StampSortedDisplayOrder.ByValueDescending);

        private static readonly StampDisplayOrder SortedValueLowToHigh =
            StampDisplayOrder.NewSorted(StampSortedDisplayOrder.ByValueAscending);

        private static readonly StampDisplayOrder NotSorted = StampDisplayOrder.Unsorted;

        /// <summary>
        /// Initializes the main view (window) and renders the initial view based on the initial view model.
        /// </summary>
        /// <param name="initialViewModel">
        /// The initial view model.
        /// </param>
        /// <param name="applicationModel">
        /// The object which provides updates of the view model and a way of sending messages.
        /// </param>
        public MainWindow(StampCollectionViewModel initialViewModel, ApplicationModel applicationModel)
        {
            InitializeComponent();

            this.applicationModel = applicationModel;

            // Render the initial view.
            this.Render(initialViewModel); // (IR)

            // Subscribe to changes in the view model. Each time a new view model arrives, render it.
            this.applicationModel.ViewModel.Subscribe(this.Render);

            /* We could avoid initial explicit call for initial rendering (IR) by using some fancy Rx subject which
             * sends the initial model already upon subscription. I kept things simple here, because this is an
             * irrelevant implementation detail. */
        }

        /// <summary>
        /// Renders the view based on the provided view model.
        /// </summary>
        /// <param name="viewModel">
        /// The view model which must be reflected by the view.
        /// </param>
        private void Render(StampCollectionViewModel viewModel)
        {
            /* In this case, I use a mixture of WPF data binding paradigm and manual WPF element manipulation. The key
             * point is that this is merely an implementation detail of the view and uses view-specific idioms and
             * "technology". The view model is completely unaware of this, which is good, because usually GUI
             * frameworks are stateful and mutable. */
            
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

        private void AddNewStampButtonClick(object sender, RoutedEventArgs e)
        {
            var description = this.newStampDescription.Text;

            // Admittedly, this validation should be performed in the view model or even model, but I kept this for
            // simplicity; it does not distract from the main idea of the architecture.
            uint value;
            if (UInt32.TryParse(this.newStampValue.Text, out value))
            {
                // Translate this GUI event to the corresponding message and send it.
                this.applicationModel.SendMessage(Message.NewAddStamp(description, value));
            }
            else
            {
                this.newStampValue.Text = "!!";
            }
                
        }

        private void StampSortingRadioButtonClick(object sender, RoutedEventArgs e)
        {
            /* Something interesting happens here. WPF jumps into conclusion by deriving the new state of radio
             * buttons when we merely click on them. We don't want this side effect, because the state of the view must
             * be derived from the view model. So we discard the state of radio buttons induced by WPF in the unwelcome
             * manner, until the view model changes. Only then render the new state of radio buttons, see method
             * Render. This idea is much deeper than it seems and leads to some very interesting questions, but this is
             * just a simple demo, so let's not go there... */
            var rb = (RadioButton)sender;
            rb.IsChecked = false;

            // Prepare a message based on which of the radio buttons exactly was clicked.
            var message =
                rb == this.rbSortHighToLow ? Message.NewSortStamps(StampSortedDisplayOrder.ByValueDescending) :
                rb == this.rbSortLowToHigh ? Message.NewSortStamps(StampSortedDisplayOrder.ByValueAscending) :
                rb == this.rbSortNone ? Message.RemoveStampSorting :
                this.Throw<Message>(new ArgumentException("Event handler attached to wrong element."));

            // Send the message.
            this.applicationModel.SendMessage(message);
        }

        private void ButtonCheckRarenessClick(object sender, RoutedEventArgs e)
        {
            /* Similar to other event handlers, nothing special here. */
            var stamp = (sender as Button).DataContext as StampViewModel;
            
            var message = Message.NewValidateRareness(stamp.Description, stamp.Value);

            this.applicationModel.SendMessage(message);
        }
    }
}
