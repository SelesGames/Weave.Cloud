﻿using SelesGames.Common;
using System.Linq;
using Weave.UserFeedAggregator.BusinessObjects;
using Incoming = Weave.UserFeedAggregator.DTOs.ServerIncoming;
using Outgoing = Weave.UserFeedAggregator.DTOs.ServerOutgoing;


namespace Weave.UserFeedAggregator.Converters
{
    public class Converters :
        IConverter<Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem, NewsItem>,
        IConverter<Weave.RssAggregator.Core.DTOs.Outgoing.Image, Image>,
        IConverter<NewsItem, Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem>,
        IConverter<Image, RssAggregator.Core.DTOs.Outgoing.Image>,
        IConverter<NewsItem, Weave.Article.Service.DTOs.ServerIncoming.SavedNewsItem>,
        IConverter<Incoming.NewFeed, Feed>,
        IConverter<Incoming.UpdatedFeed, Feed>,
        IConverter<Incoming.UserInfo, UserInfo>
    {
        public static readonly Converters Instance = new Converters();

        public Image Convert(RssAggregator.Core.DTOs.Outgoing.Image o)
        {
            return new Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        public NewsItem Convert(RssAggregator.Core.DTOs.Outgoing.NewsItem o)
        {
            return new NewsItem
            {
                Id = o.Id,
                //FeedId = o.FeedId,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                UtcPublishDateTimeString = o.PublishDateTime,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                HasBeenViewed = false,
                IsFavorite = false,
                Image = o.Image == null ? null : o.Image.Convert<RssAggregator.Core.DTOs.Outgoing.Image, Image>(Instance),
            };
        }

        RssAggregator.Core.DTOs.Outgoing.Image IConverter<Image, RssAggregator.Core.DTOs.Outgoing.Image>.Convert(Image o)
        {
            return new RssAggregator.Core.DTOs.Outgoing.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.OriginalUrl,
                BaseImageUrl = o.BaseImageUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        RssAggregator.Core.DTOs.Outgoing.NewsItem IConverter<NewsItem, RssAggregator.Core.DTOs.Outgoing.NewsItem>.Convert(NewsItem o)
        {
            return new RssAggregator.Core.DTOs.Outgoing.NewsItem
            {
                Id = o.Id,
                FeedId = o.Feed.Id,
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl,
                PublishDateTime = o.UtcPublishDateTimeString,
                Description = null,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                Image = o.Image == null ? null : o.Image.Convert<Image, RssAggregator.Core.DTOs.Outgoing.Image>(Instance),
            };
        }




        #region from Server Incoming to Business Objects

        public Feed Convert(Incoming.NewFeed o)
        {
            return new Feed
            {
                Uri = o.Uri,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
        }

        public Feed Convert(Incoming.UpdatedFeed o)
        {
            return new Feed
            {
                Id = o.Id,
                Name = o.Name,
                Category = o.Category,
                ArticleViewingType = (ArticleViewingType)o.ArticleViewingType,
            };
        }

        public UserInfo Convert(Incoming.UserInfo o)
        {
            var user = new UserInfo { Id = o.Id, };

            if (o.Feeds != null)
            {
                foreach (var feed in o.Feeds.OfType<Incoming.NewFeed>().Select(x => x.Convert<Incoming.NewFeed, Feed>(Instance)))
                    user.AddFeed(feed, trustSource: false);
            }

            return user;
        }

        #endregion




        Article.Service.DTOs.ServerIncoming.SavedNewsItem IConverter<NewsItem, Article.Service.DTOs.ServerIncoming.SavedNewsItem>.Convert(NewsItem o)
        {
            return new Article.Service.DTOs.ServerIncoming.SavedNewsItem
            {
                Title = o.Title,
                Link = o.Link,
                ImageUrl = o.ImageUrl, 
                UtcPublishDateTime = o.UtcPublishDateTimeString,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,

                //Image = o.Image == null ? null : o.Image.Convert<Image, Outgoing.Image>(Instance),
            };
        }
    }
}