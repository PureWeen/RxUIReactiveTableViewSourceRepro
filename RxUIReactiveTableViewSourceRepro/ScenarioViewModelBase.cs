using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using Splat;

namespace RxUIReactiveTableViewSourceRepro
{
    public abstract class ScenarioViewModelBase : ReactiveObject, IDisposable, IEnableLogger
    {
        private readonly CompositeDisposable disposables;
        protected readonly ReactiveList<ItemViewModel> allItems;
        protected readonly IReactiveDerivedList<SectionViewModel> sections;
        private bool disposed;

        protected ScenarioViewModelBase()
        {
            var scheduler = ImmediateScheduler.Instance;
            this.disposables = new CompositeDisposable();
            this.allItems = new ReactiveList<ItemViewModel>(scheduler: scheduler)
            {
                ChangeTrackingEnabled = true
            };

            var activeItems = this.allItems.CreateDerivedCollection(
                    x => x,
                    x => x.IsActive.GetValueOrDefault(),
                    scheduler: scheduler)
                .AddTo(this.disposables);
            var activeSection = new SectionViewModel("Active", activeItems);

            var inactiveItems = this.allItems.CreateDerivedCollection(
                    x => x,
                    x => !x.IsActive.GetValueOrDefault(true),
                    scheduler: scheduler)
                .AddTo(this.disposables);
            var inactiveSection = new SectionViewModel("Inactive", inactiveItems);

            var rareItems = this.allItems.CreateDerivedCollection(
                    x => x,
                    x => !x.IsActive.HasValue,
                    scheduler: scheduler)
                .AddTo(this.disposables);
            var rareSection = new SectionViewModel("Rare", rareItems);

            var allSections = new ReactiveList<SectionViewModel>(new[] { activeSection, inactiveSection, rareSection });
            var anySectionCountChanged = Observable
                .Merge(
                    activeSection.ObservableForProperty(x => x.IsVisible, skipInitial: true).Select(x => "Active section visibility: " + x.Value),
                    inactiveSection.ObservableForProperty(x => x.IsVisible, skipInitial: true).Select(x => "Inactive section visibility: " + x.Value),
                    rareSection.ObservableForProperty(x => x.IsVisible, skipInitial: true).Select(x => "Rare section visibility: " + x.Value));
            anySectionCountChanged
                .ObserveOn(scheduler)
                .Subscribe(x => this.Log().Debug(x));
            this.sections = allSections
                .CreateDerivedCollection(x => x, x => x.Items.Count > 0, signalReset: anySectionCountChanged, scheduler: scheduler)
                .AddTo(this.disposables);
        }

        public abstract string Name
        {
            get;
        }

        public abstract string Description
        {
            get;
        }

        protected CompositeDisposable Disposables => this.disposables;

        public IReactiveList<ItemViewModel> AllItems => this.allItems;

        public IReactiveDerivedList<SectionViewModel> Sections => this.sections;

        public bool IsDisposed => this.disposed;

        public void Dispose()
        {
            this.disposables.Dispose();
            this.disposed = true;
        }

        public abstract void Start();
    }
}