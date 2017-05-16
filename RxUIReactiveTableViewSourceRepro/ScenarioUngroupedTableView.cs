using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Foundation;
using UIKit;
using ReactiveUI;

namespace RxUIReactiveTableViewSourceRepro
{
    public class ScenarioUngroupedTableView<T> : ReactiveTableViewController, IViewFor<T>
        where T : ScenarioViewModelBase, new()
    {
        private static readonly NSString cellKey = new NSString("Key");
        private T viewModel;

        public ScenarioUngroupedTableView()
        {
            this.ViewModel = new T();
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
            this.TableView.RegisterClassForCellReuse(typeof(TableItemView), cellKey);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            this.WhenAnyValue(x => x.ViewModel.AllItems)
                .Select(x => new ReactiveTableViewSource<ItemViewModel>(this.TableView, x, cellKey, 30))
                .BindTo(this.TableView, x => x.Source);

            this.ViewModel.Start();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            this.ViewModel.Dispose();
        }
    }
}
