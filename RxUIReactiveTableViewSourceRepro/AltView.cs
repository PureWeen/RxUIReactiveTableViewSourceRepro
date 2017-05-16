using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using UIKit;
using ReactiveUI;

namespace RxUIReactiveTableViewSourceRepro
{
    public class VM : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> someProperty;

        public VM()
        {
            this.someProperty = Observable
                .Never<bool>()
                .ToProperty(this, x => x.SomeProperty, initialValue: false, scheduler: RxApp.MainThreadScheduler);
        }

        public bool SomeProperty => this.someProperty.Value;
    }

    public class AltView : UIViewController
    {
        public AltView()
        {
            var vm = new VM();
            vm
                .ObservableForProperty(x => x.SomeProperty, skipInitial: true)
                //.ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Debug.WriteLine("WTF?", x.Value));
        }
    }
}