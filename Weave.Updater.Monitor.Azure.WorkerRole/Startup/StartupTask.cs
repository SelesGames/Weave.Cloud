using Weave.Updater.PubSub;

namespace Weave.Updater.Monitor.Azure.WorkerRole.Startup
{
    internal class StartupTask
    {
        public async void OnStart()
        {
            var persister = new FeedUpdatePersister(
                "weaveuser2",
                "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                "updaterfeeds");

            await persister.Initialize();
        }
    }
}