using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Weave.RssAggregator.LibraryClient
{
    public class WebResourcePoller<T> : IDisposable, IObservable<T>
    {
        ConditionalHttpClient<T> client;
        TimeSpan pollingInterval;
        IDisposable disposeHandle;
        Subject<T> resourceUpdated = new Subject<T>();


        public WebResourcePoller(TimeSpan pollingInterval, ConditionalHttpClient<T> client)
        {
            this.client = client;
            this.pollingInterval = pollingInterval;

            InitializeTimer();
        }

        void InitializeTimer()
        {
            disposeHandle = Observable.Interval(pollingInterval).Subscribe(_ => Update(), ex => { ; });
        }

        async Task Update()
        {
            try
            {
                if (await client.CheckForUpdate())
                    resourceUpdated.OnNext(client.LatestValue);
            }
#if DEBUG
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
            }
#else
            catch { }
#endif
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return resourceUpdated.Subscribe(observer);
        }

        public void Dispose()
        {
            if (disposeHandle != null)
                disposeHandle.Dispose();
        }
    }
}
