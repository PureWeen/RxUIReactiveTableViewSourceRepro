using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using UIKit;
using ReactiveUI;
using RxUIReactiveTableViewSourceRepro.Utility;

namespace RxUIReactiveTableViewSourceRepro
{
    public class ScenariosView : UITableViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.TableView.Source = new Source(this);
        }

        private class Source : UITableViewSource
        {
            private static IList<Type> scenarios = new List<Type>();
            private readonly ScenariosView owner;
            private readonly UISegmentedControl viewTypeSegmentedControl;
            private readonly UISwitch groupedSwitch;

            static Source()
            {
                var nameTemplate = "RxUIReactiveTableViewSourceRepro.Scenario{0}ViewModel";
                var number = 1;

                while (true)
                {
                    var typeName = string.Format(nameTemplate, number);
                    var type = Type.GetType(typeName);

                    if (type == null)
                    {
                        break;
                    }

                    scenarios.Add(type);
                    ++number;
                }
            }

            public Source(ScenariosView owner)
            {
                this.owner = owner;
                this.viewTypeSegmentedControl = new UISegmentedControl(new object[] { "Table View", "Collection View" });
                this.viewTypeSegmentedControl.SelectedSegment = 1;
                this.groupedSwitch = new UISwitch();
                this.groupedSwitch.On = false;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return 2;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                switch (section)
                {
                    case 0:
                        return 2;
                    case 1:
                        return scenarios.Count;
                    default:
                        throw new InvalidOperationException();
                }
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                switch (section)
                {
                    case 0:
                        return "Options";
                    case 1:
                        return "Scenarios";
                    default:
                        throw new InvalidOperationException();
                }
            }

            public override nfloat GetHeightForHeader(UITableView tableView, nint section)
            {
                return 50;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Section == 0)
                {
                    switch (indexPath.Row)
                    {
                        case 0:
                            return CreateCellFor("Type", this.viewTypeSegmentedControl);
                        case 1:
                            return CreateCellFor("Grouped", this.groupedSwitch);
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else
                {
                    var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, "reuseID2");
                    cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    var problem = (ScenarioViewModelBase)Activator.CreateInstance(scenarios[indexPath.Row]);

                    cell.TextLabel.Text = problem.Name;
                    cell.DetailTextLabel.TextColor = UIColor.Gray;
                    cell.DetailTextLabel.Text = problem.Description;

                    return cell;
                }
            }

            private static UITableViewCell CreateCellFor(string title, UIView view)
            {
                var cell = new UITableViewCell();
                var label = new UILabel();
                label.Font = UIFont.PreferredSubheadline;
                label.Text = title;

                cell.ContentView.AddSubviews(label, view);

                cell.ConstrainLayout(() =>
                    cell.ContentView.Left() == cell.Left() &&
                    cell.ContentView.Right() == cell.Right() &&
                    cell.ContentView.Top() == cell.Top() &&
                    cell.ContentView.Bottom() == cell.Bottom() &&
                    label.Left() == cell.ContentView.Left() + Layout.StandardSuperviewSpacing &&
                    label.CenterY() == cell.ContentView.CenterY() &&
                    label.Width() == 100 &&
                    view.Left() == label.Right() + 10 &&
                    view.Right() == cell.ContentView.Right() - Layout.StandardSuperviewSpacing &&
                    view.CenterY() == cell.ContentView.CenterY());

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Section != 1)
                {
                    return;
                }

                var scenario = scenarios[indexPath.Row];
                Type viewType = null;

                if (this.viewTypeSegmentedControl.SelectedSegment == 0)
                {
                    viewType = this.groupedSwitch.On
                        ? typeof(ScenarioGroupedTableView<>).MakeGenericType(scenario)
                        : typeof(ScenarioUngroupedTableView<>).MakeGenericType(scenario);
                }
                else
                {
                    viewType = this.groupedSwitch.On
                        ? typeof(ScenarioGroupedCollectionView<>).MakeGenericType(scenario)
                        : typeof(ScenarioUngroupedCollectionView<>).MakeGenericType(scenario);
                }

                var viewInstance = Activator.CreateInstance(viewType) as UIViewController;
                Logger.Clear();
                this.owner.NavigationController.PushViewController(viewInstance, true);
            }
        }
    }
}
