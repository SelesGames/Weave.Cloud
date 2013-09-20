using HtmlAgilityPack;
using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeedIconGrabber
{
    public class DomainIconAcquirer
    {
        string domainUrl;
        HtmlNode root;

        public DomainIconAcquirer(string domainUrl)
        {
            if (string.IsNullOrEmpty(domainUrl))
                throw new ArgumentException("value cannot be null or empty", "domainUrl");

            this.domainUrl = domainUrl;
        }

        public async Task<string> GetIconUrl()
        {
            //string domainUrl = null;

            //var feed = new Feed
            //{
            //    FeedUri = "http://feeds.feedburner.com/uproxx/gammasquad",
            //};

            //var status = await feed.Update();
            //if (status == Feed.RequestStatus.OK)
            //{
            //    domainUrl = feed.DomainUrl;
            //}
            //else
            //    return;

            var client = new SmartHttpClient();
            using (var stream = await client.GetStreamAsync(domainUrl))
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.Load(stream);

                root = htmlDoc.DocumentNode;
                var orderedIcons = GetIcons().OrderByDescending(o => o.Score);
                var bestPossibleIcon = orderedIcons.FirstOrDefault();

                return bestPossibleIcon == null ? null : bestPossibleIcon.Link;

                //// only accept valid html which describes 1 head node
                //var head = htmlDoc.DocumentNode.Descendants("head").SingleOrDefault();

                //if (head == null)
                //    return;

                //foreach (var link in head.Descendants("link"))
                //{
                //    if (link.Attributes["rel"].Value.Equals("apple-touch-icon", StringComparison.OrdinalIgnoreCase))
                //        ;
                //    HtmlAttribute att = link.Attributes["href"];
                //    hrefTags.Add(att.Value);
                //}
            }
        }




        //LinkScore SelectScore(HtmlNode node)
        //{
        //    var rel = node.Attributes["rel"];
        //    if (rel == null || string.IsNullOrEmpty(rel.Value))
        //        return LinkScore.None;

        //    if (rel.Value.Equals("apple-touch-icon", StringComparison.OrdinalIgnoreCase))
        //    {
        //        var link = GetLink(node);
        //        if (string.IsNullOrEmpty(link))
        //        {
        //            return LinkScore.None;
        //        }
        //        else
        //        {
        //            return new LinkScore(link, 5);
        //        }
        //    }

        //    else if (rel.Value.Equals("apple-touch-icon-precomposed", StringComparison.OrdinalIgnoreCase))
        //    {
        //        var link = GetLink(node);
        //        if (string.IsNullOrEmpty(link))
        //        {
        //            return LinkScore.None;
        //        }
        //        else
        //        {
        //            return new LinkScore(link, 4);
        //        }
        //    }


        //    return LinkScore.None;
        //}

        //string GetLink(HtmlNode node)
        //{
        //    var href = node.Attributes["href"];
        //    if (href == null || string.IsNullOrEmpty(href.Value))
        //        return null;

        //    var hrefVal = href.Value;
        //    string link = null;

        //    if (hrefVal.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
        //        hrefVal.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        //    {
        //        link = hrefVal;
        //    }

        //    else
        //    {
        //        link = domainUrl + hrefVal;
        //    }

        //    if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
        //        return link;

        //    return null;
        //}




        #region Private Helper functions for grabbing the different favicons

        IEnumerable<LinkScore> GetIcons()
        {
            var linkScore = CreateLinkScoreOrNull("apple-touch-icon", 5);
            if (linkScore != null)
                yield return linkScore;

            linkScore = CreateLinkScoreOrNull("apple-touch-icon-precomposed", 4);
            if (linkScore != null)
                yield return linkScore;

            linkScore = CreateLinkScoreOrNull("shortcut icon", 3);
            if (linkScore != null)
                yield return linkScore;

            linkScore = CreateLinkScoreOrNull("icon", 2);
            if (linkScore != null)
                yield return linkScore;
        }

        readonly string LINK_SEARCH_TEMPLATE = "/html/head/link[@rel='{0}' and @href]";

        LinkScore CreateLinkScoreOrNull(string iconType, int score)
        {
            var search = string.Format(LINK_SEARCH_TEMPLATE, iconType);

            var linkNode = root.SelectSingleNode(search);
            if (linkNode != null)
            {
                var link = GetLink(linkNode.Attributes["href"].Value);
                if (link != null)
                    return new LinkScore(link, score, linkNode.OuterHtml);
            }

            return null;
        }

        string GetLink(string url)
        {
            string link = null;

            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                link = url;
            }

            else
            {
                link = domainUrl + url;
            }

            if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
                return link;

            return null;
        }

        #endregion




        #region Private helper class LinkScore

        class LinkScore
        {
            public string Link { get; set; }
            public int Score { get; set; }
            public string NodeText { get; set; }

            public LinkScore(string link, int score, string nodeText)
            {
                Link = link;
                Score = score;
                NodeText = nodeText;
            }

            public override string ToString()
            {
                return string.Format("score {0}: {1} <-- {2}", Score, Link, NodeText);
            }
        }

        #endregion
    }
}
