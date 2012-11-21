using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using weave;
using System.Reactive.Concurrency;
using System.Threading;
using System.Reactive;
using System.Reactive.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Weave.RssAggregator.RssFeedParsingTestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var temps = new List<string>
            {
                //" http://tvprofil.net/rss/?g=0",
                //"http://feeds.pheedo.com/toms_hardware", great for testing redirects
                //"http://feeds.gawker.com/gizmodo/vip",
                //"http://www.theverge.com/rss/index.xml",
                "http://www.comicsalliance.com/rss.xml",
            };

            var scheduler = new EventLoopScheduler(ts => new Thread(ts) { Name = "shizzles" });

            temps.Select(s => HttpWebRequest.Create(s).GetWebResponseAsync()).Merge().ObserveOn(scheduler).Aggregate(0d, (d, o) =>

            //.Subscribe(o =>
            {
                using (var stream = o.GetResponseStream())
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var news = stream.ToRssIntermediates().Select(x => x.ToNewsItem()).ToList();

                    GetRedirectUris(news.Select(x => x.Link))
                        .Subscribe(redirectUris =>
                        {
                            var redirectedNews = news.Zip(redirectUris, (ni, ru) => new { NewsItem = ni, Redirect = ru }).ToList();
                            DebugEx.WriteLine(redirectedNews.ToString());
                        });

                    var query = BuildFacebookSocialQuery(news.Select(x => x.Link));

                    DebugEx.WriteLine(query);

                    HttpWebRequest.Create(query).GetWebResponseAsync().Subscribe(x =>
                    {
                        using (var s = x.GetResponseStream())
                        {
                            var xml = XDocument.Load(s);
                            XNamespace facebookNS = "http://api.facebook.com/1.0/";

                            var linkStats = xml.Root.Elements(facebookNS + "link_stat").Select(y => new
                                {
                                    LikeCount = y.Element(facebookNS + "like_count").ValueOrDefault(),
                                    ShareCount = y.Element(facebookNS + "share_count").ValueOrDefault(),
                                    ClickCount = y.Element(facebookNS + "click_count").ValueOrDefault(),
                                    CommentCount = y.Element(facebookNS + "comment_count").ValueOrDefault(),
                                    TotalCount = y.Element(facebookNS + "total_count").ValueOrDefault(),
                                });

                            var modifiedNews = news.Zip(linkStats, (ni, ls) => new { NewsItem = ni, FacebookStats = ls }).ToList();

                            DebugEx.WriteLine(modifiedNews.ToString());
                            s.Close();
                        }
                    });

                    sw.Stop();
                    //DebugEx.WriteLine("took {0} ms to translate all to newsitem", sw.ElapsedMilliseconds);
                    stream.Close();
                    d += sw.ElapsedMilliseconds;
                    DebugEx.WriteLine(news.ToString());
                    return d;
                }
            })
            .Subscribe(o =>
            {
                DebugEx.WriteLine("average time across runs = {0}ms", o / (double)temps.Count);
            });
            //});

            while (true)
                Console.Read();
        }

//http://api.facebook.com/method/fql.query?query=select%20like_count,%20share_count%20from%20link_stat%20where%20url=%22http://www.collegehumor.com/picture/6505598/bar-rafaeli-hover-hand%22%20or%20url=%22http://www.wwtdd.com/2011/08/jim-carrey-is-insane-hearts-emma-stone/%22%20or%20url=%22http://www.saschakimmel.com/2010/05/how-to-get-statistics-for-a-facebook-like-button-and-shared-urls/%22%20or%20url=%22http://www.thesuperficial.com/minka-kelly-derek-jeter-split-08-2011%22%20or%20url=%22http://www.thesuperficial.com/adrianne-curry-christopher-knight-files-for-divorce-08-2011%22%20or%20url=%22http://www.thesuperficial.com/jersey-shore-deena-bi-sexual-hookup-the-situation-08-2011%22%20or%20url=%22http://www.thesuperficial.com/kelly-brook-bike-08-2011%22%20or%20url=%22http://www.thesuperficial.com/ricky-gervais-jesus-new-humanist-cover-08-2011%22%20or%20url=%22http://www.thesuperficial.com/kris-jenner-kris-humphries-manager-08-2011%22%20or%20url=%22http://www.thesuperficial.com/katy-perry-is-looking-just-fantastic-and-other-news-08-2011%22%20or%20url=%22http://www.thesuperficial.com/alexander-skarsgard-birthday-gym-clothes-08-2011%22

        static readonly string baseQueryUrl =
"http://api.facebook.com/method/fql.query?query=select%20like_count,%20share_count,%20click_count,%20comment_count,%20total_count%20from%20link_stat%20where%20url=";

        static string BuildFacebookSocialQuery(IEnumerable<string> urls)
        {
            var firstUrl = urls.FirstOrDefault();

            if (string.IsNullOrEmpty(firstUrl))
                return null;

            var sb = new StringBuilder(baseQueryUrl);

            sb.Append(HttpUtility.UrlEncode(string.Format("\"{0}\"", firstUrl)));

            foreach (var url in urls.Skip(1))
                sb.Append(HttpUtility.UrlEncode(string.Format(" or url=\"{0}\"", url)));

            return sb.ToString();
        }

        static IObservable<List<string>> GetRedirectUris(IEnumerable<string> uris)
        {
            return uris
                .Select(uri => 
                {
                    var request = HttpWebRequest.Create(uri) as HttpWebRequest;
                    request.AllowAutoRedirect = false;
                    return request.GetWebResponseAsync().Select(response =>
                    {
                        if (response is HttpWebResponse)
                        {
                            var httpResponse = (HttpWebResponse)response;
                            if (httpResponse.StatusCode == HttpStatusCode.Moved)
                                return httpResponse.Headers["Location"];
                            else
                                return uri;
                        }
                        else
                            return uri;
                    });
                })
                .Merge()
                .Aggregate(new List<string>(), (list, uri) =>
                {
                    list.Add(uri);
                    return list;
                });
        }
    }
}
