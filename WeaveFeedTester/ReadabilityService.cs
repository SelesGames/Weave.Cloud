using System;
using System.IO;
using Weave.Readability;

namespace WeaveFeedTester
{
    public class ArticleFormatter
    {
        const string CSS_PATH = "/WeaveFeedTester;component/css.txt";
        static string htmlTemplate;


        public string CreateHtml(string domainUrl, string domain, string title, string body, string linkColor)
        {
            if (htmlTemplate == null)
                ReadHtmlTemplate();

            var html = htmlTemplate
                        .Replace("[SOURCE]", domain)
                        .Replace("[TITLE]", title)
                        .Replace("[BODY]", body)
                        .Replace("[ACCENT]", linkColor);

            return html;
        }




        #region Helper methods for creating the html from a template

        static void ReadHtmlTemplate()
        {
            Uri uri = new Uri(CSS_PATH, UriKind.Relative);
            using (var fuckyou = File.Open("css.txt", FileMode.Open))
            //StreamResourceInfo streamResourceInfo = Application.GetResourceStream(uri);
            //using (StreamReader streamReader = new StreamReader(streamResourceInfo.Stream))
            using (StreamReader streamReader = new StreamReader(fuckyou))
            {
                htmlTemplate = streamReader.ReadToEnd();
            }
        }

        #endregion
    }

    public class StandardArticleFormatter
    {
        ArticleFormatter formatter = new ArticleFormatter();

        public string GetFormattedHtml(string link, string articleTitle, string linkColor, string feedName, string description)
        {
            if (string.IsNullOrEmpty(description)) throw new Exception("No content in the Description field");
            if (string.IsNullOrEmpty(articleTitle)) throw new Exception("No Title for NewsItem");

            var domainUrl = feedName;
            var domain = feedName;
            var html = formatter.CreateHtml(domainUrl, domain, articleTitle, description, linkColor);
            return html;
        }
    }

    public class ReadabilityFormatter
    {
        ArticleFormatter formatter = new ArticleFormatter();

        public string GetFormattedHtml(ReadabilityResult result, string link, string articleTitle, string linkColor, string feedName)
        {
            var domainUrl = result.domain;
            var domain = !string.IsNullOrEmpty(feedName) ? feedName : result.domain;
            var title = !string.IsNullOrEmpty(articleTitle) ? articleTitle : result.title;

            var html = formatter.CreateHtml(domainUrl, domain, title, result.content, linkColor);
            return html;
        }
    }
}
