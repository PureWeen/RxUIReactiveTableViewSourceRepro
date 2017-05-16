using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Foundation;
using UIKit;
using ReactiveUI;

namespace RxUIReactiveTableViewSourceRepro
{
    public class ScenarioGroupedTableView<T> : ReactiveTableViewController, IViewFor<T>
        where T : ScenarioViewModelBase, new()
    {
        private static readonly NSString cellKey = new NSString("Key");
        private T viewModel;

        public ScenarioGroupedTableView()
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

            this.WhenAnyValue(x => x.ViewModel.Sections)
                .Select(x => x.CreateDerivedCollection(y => new ItemsSection(y.Title, y.Items)))
                .Select(x => new ReactiveTableViewSource<ItemViewModel>(this.TableView) { Data = x })
                .BindTo(this.TableView, x => x.Source);

            this.ViewModel.Start();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            this.ViewModel.Dispose();
        }

        private sealed class ItemsSection : TableSectionInformation<ItemViewModel>
        {
            public ItemsSection(string title, IReadOnlyReactiveList<ItemViewModel> items)
            {
                this.Collection = items;
                this.CellKeySelector = _ => cellKey;
                this.Header = new TableSectionHeader(title);
                this.SizeHint = 30;
                this.InitializeCellAction = x => x.Accessory = UITableViewCellAccessory.None;
            }
        }
    }
}
