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
    public class ApplicationModel
    {
        private StampCollection model;

        private StampCollectionViewModel viewModel;

        private readonly Subject<StampCollectionViewModel> viewModelSubject;

        internal ApplicationModel(StampCollection initialModel, StampCollectionViewModel initialViewModel)
        {
            this.model = initialModel;
            this.viewModel = initialViewModel;

            this.viewModelSubject = new Subject<StampCollectionViewModel>();
        }

        internal IObservable<StampCollectionViewModel> ViewModel => this.viewModelSubject.AsObservable();

        internal void SendMessage(Message message)
        {
            var newMandVm = Dispatching.dispatch(this.model,this.viewModel, message);

            this.model = newMandVm.Item1;
            this.viewModel = newMandVm.Item2;

            var effect = newMandVm.Item3;

            if (effect.IsEffect)
            {
                var e = effect as Effect<Message>.Effect;
                ExecuteEffect(e.Item, this.SendMessage);
            }

            this.viewModelSubject.OnNext(this.viewModel);
        }

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
