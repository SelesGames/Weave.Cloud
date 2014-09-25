using Weave.Updater.BusinessObjects;

namespace Weave.FeedUpdater.BusinessObjects.Cache
{
    static class ExpandedEntryExtensions
    {
        public static void NullOutHeavyFields(this ExpandedEntry entry)
        {
            entry.Description = null;
            entry.OriginalRssXml = null;
        }
    }
}