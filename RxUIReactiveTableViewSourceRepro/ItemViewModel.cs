using System;
using System.Diagnostics;
using ReactiveUI;

namespace RxUIReactiveTableViewSourceRepro
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class ItemViewModel : ReactiveObject
    {
        private readonly string name;
        private bool? isActive;

        public ItemViewModel(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }

        public bool? IsActive
        {
            get { return this.isActive; }
            set { this.RaiseAndSetIfChanged(ref this.isActive, value); }
        }

        public string DebuggerDisplay
        {
            get { return this.Name + " " + this.IsActiveDisplay; }
        }

        public override string ToString()
        {
            return this.DebuggerDisplay;
        }

        private string IsActiveDisplay
        {
            get
            {
                if (!this.IsActive.HasValue)
                {
                    return "rare";
                }

                return this.IsActive.Value ? "active" : "inactivate";
            }
        }
    }
}