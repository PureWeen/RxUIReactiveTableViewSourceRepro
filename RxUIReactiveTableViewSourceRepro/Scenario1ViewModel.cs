using System;
using System.Reactive.Concurrency;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace RxUIReactiveTableViewSourceRepro
{
    public class Scenario1ViewModel : ScenarioViewModelBase
    {
        public override void Start()
        {
            RxApp.MainThreadScheduler.Schedule<string>(
                null,
                TimeSpan.FromSeconds(1),
                (s, st) =>
                {
                    List<ItemViewModel> items = new List<ItemViewModel>();
                    for (int i = 0; i < 10; ++i)
                        items.Add(new ItemViewModel($"Item {i}") { IsActive = true });

                    allItems.AddRange(items);
                });

            RxApp.MainThreadScheduler.Schedule<string>(
             null,
             TimeSpan.FromSeconds(5),
             (s, st) =>
             {
                 allItems.RemoveRange(1, 9);
             });
        }

        public override string Name => "Click Me - Batch Delete";

        public override string Description
            => "Items deleted off tail end of code";
    }
}
