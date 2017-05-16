////namespace RxUIReactiveTableViewSourceRepro
////{
////    using System;
////    using System.Collections.Generic;
////    using System.Linq;
////    using System.Reactive.Linq;
////    using MonoTouch.Foundation;
////    using ReactiveUI;

////    //public class Foo :  UITableView
////    //{
////    //    public override void EndUpdates()
////    //    {
////    //        base.EndUpdates();
////    //    }
////    //}

////    public class View : TableViewControllerBase<HammerTimeViewModel>
////    {
////        private static readonly NSString cellKey = new NSString("Key");

////        public View()
////        {
////            this.ViewModel = new HammerTimeViewModel();
////        }

////        public override void ViewDidAppear(bool animated)
////        {
////            base.ViewDidAppear(animated);

////            this.WhenAnyValue(x => x.ViewModel.Sections)
////                .Select(x => x.CreateDerivedCollection(y => new ItemsSection(y.Title, y.Items)))
////                .Select(x => new ReactiveTableViewSource<ItemViewModel>(this.TableView) { Data = x })
////                .BindTo(this.TableView, x => x.Source);
////        }

////        private sealed class ItemsSection : TableSectionInformation<ItemViewModel>
////        {
////            public ItemsSection(string title, IReadOnlyReactiveList<ItemViewModel> items)
////            {
////                this.Collection = items;
////                this.CellKeySelector = _ => cellKey;
////                this.Header = new TableSectionHeader(title);
////                this.SizeHint = 30;
////            }
////        }
////    }
////}