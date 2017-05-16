using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace RxUIReactiveTableViewSourceRepro
{
    public static class CompositeDisposableExtensions
    {
        public static T AddTo<T>(this T @this, CompositeDisposable disposable)
            where T : IDisposable
        {
            if (@this == null)
            {
                throw new ArgumentNullException("@this");
            }
            disposable.Add(@this);
            return @this;
        }
    }
}
