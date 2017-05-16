using System;
using ReactiveUI;
using UIKit;
using RxUIReactiveTableViewSourceRepro.Utility;

namespace RxUIReactiveTableViewSourceRepro
{
    public class TableItemView : ReactiveTableViewCell, IViewFor<ItemViewModel>
    {
        private ItemViewModel viewModel;
        private UILabel nameLabel;
        private bool didSetupConstraints;

        public TableItemView(IntPtr handle)
            : base(handle)
        {
            this.CreateView();
            this.CreateBindings();
            this.UpdateConstraints();
        }

        public ItemViewModel ViewModel
        {
            get { return this.viewModel; }
            set { this.RaiseAndSetIfChanged(ref this.viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return this.ViewModel; }
            set { this.ViewModel = (ItemViewModel)value; }
        }

        public override void UpdateConstraints()
        {
            base.UpdateConstraints();

            if (this.didSetupConstraints)
            {
                return;
            }

            this.ContentView.ConstrainLayout(() =>
                this.nameLabel.Top() == this.ContentView.Top() + Layout.StandardSiblingViewSpacing &&
                this.nameLabel.Bottom() == this.ContentView.Bottom() - Layout.StandardSiblingViewSpacing &&
                this.nameLabel.Left() == this.ContentView.Left() + Layout.StandardSuperviewSpacing &&
                this.nameLabel.Right() == this.ContentView.Right() - Layout.StandardSuperviewSpacing);

            this.didSetupConstraints = true;
        }

        private void CreateView()
        {
            this.Accessory = UITableViewCellAccessory.DisclosureIndicator;

            this.nameLabel = new UILabel();

            this.ContentView.AddSubviews(this.nameLabel);
        }

        private void CreateBindings()
        {
            this.OneWayBind(this.ViewModel, x => x.Name, x => x.nameLabel.Text);
        }
    }
}