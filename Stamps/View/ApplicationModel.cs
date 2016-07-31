using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Stamps.Messaging;
using Stamps.Model;
using Stamps.ViewModel;

namespace Stamps.View
{
    /// <summary>
    /// <para>
    /// This class is the main "effectful" component of the application, besides the view itself, which is inherently
    /// mutable and stateful. It contains current model and view model, passing them to the dispatcher function and
    /// replacing them with new model and view model when the dispatching function returns. It is also responsible for
    /// performing effectful computations requested by the model and fitting this into the execution model of the
    /// view (things like GUI threads and so on). Lastly, this class provides to the view the possibility of sending
    /// messages. Note, that the view is not aware of the final destination of the messages.
    /// </para>
    /// <para>
    /// Intuitively, this component has nothing to do with the view; the using directives contain no dependency 
    /// whatsoever on WPF. We can distinguish the layers of the system even more, by putting this class together with
    /// the "main" function of the system, separately from the view. However, in WPF it is idiomatic that the main
    /// function is in the view itself, so I kept this class in the view.
    /// </para>
    /// </summary>
    public class ApplicationModel
    {
        /// <summary>
        /// Current model.
        /// </summary>
        private StampCollection model;

        /// <summary>
        /// Current view model.
        /// </summary>
        private StampCollectionViewModel viewModel;

        /// <summary>
        /// When the view sends a message, this may result in changes to the view. As we aim for the view to be
        /// completely derivable from the view model and the view model is immutable, we need a way for the view to
        /// "receive" the new view model. This Rx subject is used by this class to send new view models to the view.
        /// </summary>
        private readonly Subject<StampCollectionViewModel> viewModelSubject;

        /// <summary>
        /// Initializes the initial state of the application, consisting of the initial model and initial view model.        
        /// </summary>
        /// <remarks>
        /// Note, that as this class is merely a "state holder", it does not create the initial model and view model
        /// itself. This happens in the "main" function of the application.
        /// </remarks>
        /// <param name="initialModel">
        /// The initial model.
        /// </param>
        /// <param name="initialViewModel">
        /// The initial view model.
        /// </param>
        internal ApplicationModel(StampCollection initialModel, StampCollectionViewModel initialViewModel)
        {
            this.model = initialModel;
            this.viewModel = initialViewModel;

            this.viewModelSubject = new Subject<StampCollectionViewModel>();
        }

        /// <summary>
        /// This is the stream of view models, representing changes of the view model, accessible to the view.
        /// </summary>
        internal IObservable<StampCollectionViewModel> ViewModel => this.viewModelSubject.AsObservable();

        /// <summary>
        /// The view sends messages using this method. This method is also used for messages originating from the world
        /// of effectful computations. In a way, this class also belongs to that world...
        /// </summary>
        /// <param name="message"></param>
        internal void SendMessage(Message message)
        {
            // Invoke the dispatching function with current model, current view model and the message.
            var newMandVm = Dispatching.dispatch(this.model,this.viewModel, message);

            // Remember new model and view model as current model and view model, respectively. Observe, that this
            // class doesn't care whether the model or view model actually changed.
            this.model = newMandVm.Item1;
            this.viewModel = newMandVm.Item2;

            // If the model requested an effectful computation, perform it and route the message back to this function,
            // such that it will be passed to the dispatcher function, exactly the same way as for messages coming from
            // the view.
            var effect = newMandVm.Item3;
            if (effect.IsEffect)
            {
                var e = effect as Effect<Message>.Effect;
                ExecuteEffect(e.Item, this.SendMessage); // executes asynchronously (AE)
            }

            // Send the updated view model to the view. Property (AE) is crucial, because the GUI will not be blocked
            // and the view can assume any intermediate state defined by the view model, until the computation (if any)
            // is done. The example in this demo is simulated verification of stamp rareness, where the view displays a
            // "working on it" feed-back.
            this.viewModelSubject.OnNext(this.viewModel);
        }

        /// <summary>
        /// Implementation of the world of effectful computations. It respects the threading model of WPF.
        /// </summary>
        /// <param name="effect">
        /// The effectful computation to be performed.
        /// </param>
        /// <param name="messageDestination">
        /// A function which accepts the message produced by <paramref name="effect"/>, when it completes.
        /// </param>
        private static async void ExecuteEffect(FSharpAsync<Message> effect, Action<Message> messageDestination)
        {
            var m = await FSharpAsync.StartAsTask(
                effect,
                FSharpOption<TaskCreationOptions>.None,
                FSharpOption<CancellationToken>.None);

            messageDestination(m);
        }
    }
}
