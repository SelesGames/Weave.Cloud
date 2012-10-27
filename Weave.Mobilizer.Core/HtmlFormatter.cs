using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
//using HtmlAgilityPack;
using Weave.Readability;

namespace Weave.Mobilizer.Core
{
    public class HtmlFormatter
    {
        string htmlTemplate;

        public void ReadHtmlTemplate()
        {
            var stream = File.OpenRead("css.txt");
            htmlTemplate = stream.GetReadStream();
        }

        public string CreateHtml(string title, string source, string body)
        {
            if (htmlTemplate == null)
                ReadHtmlTemplate();

            var html = htmlTemplate
                        .Replace("[TITLE]", title)
                        .Replace("[SOURCE]", source)
                        .Replace("[BODY]", body);

            return html;
        }
    }

    public static class ReadabilityExtensions
    {
        public static string FormatToHtml(this ReadabilityResult result, HtmlFormatter formatter)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (formatter == null) throw new ArgumentNullException("formatter");

            var source = string.Format("{0} • {1}", result.domain, result.date_published);

            return formatter.CreateHtml(result.title, source, result.content);
        }
    }

    //public static class HtmlCleaning
    //{
    //    public static string ReformatHtml(this string html)
    //    {
    //        if (string.IsNullOrEmpty(html))
    //            return null;

    //        var stopwatch = Stopwatch.StartNew();

    //        HtmlDocument doc = new HtmlDocument();
    //        doc.OptionDefaultStreamEncoding = Encoding.UTF8;
    //        doc.LoadHtml(html);


    //        var links = doc.DocumentNode
    //            .Descendants("a")
    //            .Where(a => a.Attributes.Contains("href"))
    //            .Select(a => a.Attributes["href"])
    //            .OfType<HtmlAttribute>()
    //            .ToList();

    //        var replacementUrl = string.Format("{0}/IPF?url=", InstapaperMobilizerFormatter.BaseArticleRedirectUrl);
    //        foreach (var link in links)
    //        {
    //            link.Value = replacementUrl + link.Value;
    //        }

    //        string output = doc.DocumentNode.WriteTo();

    //        stopwatch.Stop();
    //        Debug.WriteLine("Took {0} milliseconds to modify the html", stopwatch.ElapsedMilliseconds);
    //        Console.WriteLine();
    //        return output;
    //    }
    //}
}