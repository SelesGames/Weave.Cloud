using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.FeedUpdater.HighFrequency;

namespace Weave.FeedUpdater.Service.Role.Ak
{
    public class DoStuff
    {
        public void Test()
        {
            var system = ActorSystem.Create("system");
            var feedUrl = "test";
            var feedActor = system.ActorOf(FeedUpdateActor.Props(feedUrl));
            feedActor.Tell(new object());

            var router = system.ActorOf(Props.Empty.WithRouter(new FeedPool()));
        }
    }

    public class FeedPool : Akka.Routing.Group
    {

    }

    public class FeedUpdateActor : ReceiveActor
    {
        HighFrequencyFeed feed;
        StandardProcessorChain processorChain;

        public static Props Props(string feedUrl)
        {
            return Akka.Actor.Props.Create(() => new FeedUpdateActor(feedUrl));
        }

        public FeedUpdateActor(string feedUrl)
        {
            Receive<object>(_ => Update());
        }

        async void Update()
        {
            await feed.Refresh(processorChain);
        }
    }
}
