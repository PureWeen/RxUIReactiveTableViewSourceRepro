using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI;

namespace RxUIReactiveTableViewSourceRepro
{
    public class SectionViewModel : ReactiveObject
    {
        private readonly string title;
        private readonly IReadOnlyReactiveList<ItemViewModel> items;
        private readonly ObservableAsPropertyHelper<bool> isVisible;

        public SectionViewModel(string title, IReadOnlyReactiveList<ItemViewModel> items)
        {
            this.title = title;
            this.items = items;
            this.isVisible = this.items
                .CountChanged
                .Select(x => x > 0)
                .ToProperty(this, x => x.IsVisible, initialValue: false, scheduler: RxApp.MainThreadScheduler);
        }

        public string Title => this.title;

        public IReadOnlyReactiveList<ItemViewModel> Items => this.items;

        public bool IsVisible => this.isVisible.Value;
    }
}
