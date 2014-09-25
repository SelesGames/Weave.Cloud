
namespace Weave.FeedUpdater.BusinessObjects.Cache
{
    public static class ExpandedEntryCacheFactory
    {
        public static ExpandedEntryCache CreateCache(int inMemoryCacheSize)
        {
            var cache = new ExpandedEntryCache(
                localCacheLength: inMemoryCacheSize,
                azureUserIndexStorageAccountName: "weaveuser2",
                azureUserIndexStorageAccountKey: "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                azureUserIndexContainerName: "expandedentries");

            return cache;
        }

        public static ExpandedEntrySaveHelper CreateSaveHelper()
        {
            var helper = new ExpandedEntrySaveHelper(
                azureUserIndexStorageAccountName: "weaveuser2",
                azureUserIndexStorageAccountKey: "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
                azureUserIndexContainerName: "expandedentries");

            return helper;
        }
    }
}