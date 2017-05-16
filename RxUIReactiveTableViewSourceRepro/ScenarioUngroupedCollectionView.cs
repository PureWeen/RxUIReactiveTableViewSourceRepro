using System.Drawing;
using System.Reactive.Linq;
using Foundation;
using UIKit;
using ReactiveUI;

namespace RxUIReactiveTableViewSourceRepro
{
    public class ScenarioUngroupedCollectionView<T> : ReactiveCollectionViewController, IViewFor<T>
        where T : ScenarioViewModelBase, new()
    {
        private static readonly NSString cellKey = new NSString("Key");
        private T viewModel;

        public ScenarioUngroupedCollectionView()
            : base(GetLayout())
        {
            this.ViewModel = new T();
            this.CollectionView.BackgroundColor = UIColor.White;
        }

        private static UICollectionViewLayout GetLayout()
        {
            return new UICollectionViewFlowLayout
            {
                ItemSize = new SizeF(250, 40),
                SectionInset = new UIEdgeInsets(0, 0, 0, 0)
            };
        }

        public T ViewModel
        {
            get { return this.viewModel; }
            set { this.RaiseAndSetIfChanged(ref this.viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return this.ViewModel; }
            set { this.ViewModel = (T)value; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.CollectionView.RegisterClassForCell(typeof(CollectionItemView), cellKey);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            this.WhenAnyValue(x => x.ViewModel.AllItems)
                .Select(x => new ReactiveCollectionViewSource<ItemViewModel>(this.CollectionView, x, cellKey))
                .BindTo(this.CollectionView, x => x.Source);

            this.ViewModel.Start();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            this.ViewModel.Dispose();
        }
    }
}
