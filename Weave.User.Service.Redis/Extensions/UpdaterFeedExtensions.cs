using Weave.Updater.BusinessObjects;

namespace Weave.User.Service.Redis
{
    public static class UpdaterFeedExtensions
    {
        public static void CopyStateTo(this Feed source, Feed target)
        {
            target.TeaserImageUrl = source.TeaserImageUrl;
            target.LastRefreshedOn = source.LastRefreshedOn;
            target.Etag = source.Etag;
            target.LastModified = source.LastModified;
            target.MostRecentNewsItemPubDate = source.MostRecentNewsItemPubDate;

            foreach (var entry in source.News)
                target.News.Add(entry);
        }
    }
}