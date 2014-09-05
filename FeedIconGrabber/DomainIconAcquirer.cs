using HtmlAgilityPack;
using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
            var client = new SmartHttpClient();
            using (var response = await client.GetAsync(domainUrl))
            using (var stream = await response.ReadStream())
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.Load(stream);

                root = htmlDoc.DocumentNode;
                var icons = await GetIcons();
                var orderedIcons = icons.OrderByDescending(o => o.Score);
                var bestPossibleIcon = orderedIcons.FirstOrDefault();

                return bestPossibleIcon == null ? null : bestPossibleIcon.Link;
            }
        }




        #region Private Helper functions for grabbing the different favicons

        async Task<IEnumerable<LinkScore>> GetIcons()
        {
            List<LinkScore> scores = new List<LinkScore>();

            // only accept valid html which describes 1 head node
            var head = root.Descendants("head").SingleOrDefault();

            if (head == null)
                return scores;

            LinkScore linkScore = null;

            foreach (var link in head.Descendants("link"))
            {
                linkScore = await GetLinkFromHtml(link, "apple-touch-icon", 10);
                if (linkScore != null)
                    scores.Add(linkScore);

                linkScore = await GetLinkFromHtml(link, "apple-touch-icon-precomposed", 8);
                if (linkScore != null)
                    scores.Add(linkScore);

                linkScore = await GetLinkFromHtml(link, "shortcut icon", 2);
                if (linkScore != null)
                    scores.Add(linkScore);

                linkScore = await GetLinkFromHtml(link, "icon", 1);
                if (linkScore != null)
                    scores.Add(linkScore);
            }

            linkScore = await GetLinkFromRootDirectory("apple-touch-icon-57x57-precomposed.png", 3);
            if (linkScore != null)
                scores.Add(linkScore);

            linkScore = await GetLinkFromRootDirectory("apple-touch-icon-57x57.png", 4);
            if (linkScore != null)
                scores.Add(linkScore);

            linkScore = await GetLinkFromRootDirectory("apple-touch-icon-precomposed.png", 4);
            if (linkScore != null)
                scores.Add(linkScore);

            linkScore = await GetLinkFromRootDirectory("apple-touch-icon.png", 5);
            if (linkScore != null)
                scores.Add(linkScore);

            linkScore = await GetLinkFromRootDirectory("apple-touch-icon-152x152-precomposed.png", 4);
            if (linkScore != null)
                scores.Add(linkScore);

            linkScore = await GetLinkFromRootDirectory("apple-touch-icon-152x152.png", 5);
            if (linkScore != null)
                scores.Add(linkScore);

            linkScore = await GetLinkFromRootDirectory("favicon.ico", 1);
            if (linkScore != null)
                scores.Add(linkScore);

            return scores;
        }

        async Task<LinkScore> GetLinkFromHtml(HtmlNode node, string iconType, int score)
        {
            var rel = node.Attributes["rel"];
            if (rel == null || string.IsNullOrEmpty(rel.Value))
                return null;

            if (rel.Value.Equals(iconType, StringComparison.OrdinalIgnoreCase))
            {
                var link = GetLink(node);
                if (link != null)
                {
                    var linkIsValid = await TestLinkValidity(link);
                    if (linkIsValid)
                        return new LinkScore(link, score, node.OuterHtml);
                }
            }

            return null;
        }

        string GetLink(HtmlNode node)
        {
            var href = node.Attributes["href"];
            if (href == null || string.IsNullOrEmpty(href.Value))
                return null;

            var hrefVal = href.Value;
            return GetLink(hrefVal);
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

        async Task<LinkScore> GetLinkFromRootDirectory(string fileName, int score)
        {
            string url = domainUrl;

            if (!domainUrl.EndsWith("/"))
                url += "/";

            url += fileName;

            var link = GetLink(url);
            if (link != null)
            {
                var linkIsValid = await TestLinkValidity(link);
                if (linkIsValid)
                    return new LinkScore(link, score, null);
            }

            return null;
        }

        async Task<bool> TestLinkValidity(string link)
        {
            var client = new SmartHttpClient();
            var request = new HttpRequestMessage(HttpMethod.Head, link);
            var response = await client.SendAsync(request, CancellationToken.None);
            return 
                response.HttpResponseMessage.IsSuccessStatusCode && 
                IsImageType(response.HttpResponseMessage.Content.Headers.ContentType.MediaType);
        }

        bool IsImageType(string mediaType)
        {
            return
                //mediaType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ||
                //mediaType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase);
                mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
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
