using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class SubscriptionAggregator<TKey, TResult> : IObservable<TResult>
    {
        Dictionary<TKey, IDisposable> lookup = new Dictionary<TKey, IDisposable>();
        Subject<TResult> backingSub = new Subject<TResult>();

        public void AddSubscription(TKey key, IObservable<TResult> sub)
        {
            if (lookup.ContainsKey(key))
                throw new Exception();

            var disp = sub.Subscribe(backingSub.OnNext, backingSub.OnError);
            lookup.Add(key, disp);
        }

        public void RemoveSubscription(TKey key)
        {
            if (!lookup.ContainsKey(key))
                return;

            var disp = lookup[key];
            lookup.Remove(key);
            disp.Dispose();
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            return backingSub.AsObservable().Subscribe(observer);
        }
    }
}
