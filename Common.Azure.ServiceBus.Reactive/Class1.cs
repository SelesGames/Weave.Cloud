using Microsoft.ServiceBus.Messaging;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Common.Azure.ServiceBus.Reactive
{
    public static class Extensions
    {
        public static IObservable<BrokeredMessage> AsObservable(this SubscriptionClient client)
        {
            return Observable.Create<BrokeredMessage>(observer =>
            {
                IDisposable disp = Disposable.Empty;

                try
                {
                    bool shouldContinue = true;
                    disp = Disposable.Create(() => shouldContinue = false);

                    Observable.StartAsync(async () =>
                    {
                        while (shouldContinue)
                        {
                            var message = await client.ReceiveAsync();
                            if (message != null)
                                observer.OnNext(message);
                            else
                                await Task.Delay(5000);
                        }
                    });
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }

                return disp;
            });
        }
    }
}
