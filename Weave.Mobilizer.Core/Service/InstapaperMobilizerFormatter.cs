using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Weave.Mobilizer.Core.Cache;
using Weave.Readability;

namespace Weave.Mobilizer.Core
{
    [ServiceBehavior(
        IncludeExceptionDetailInFaults = false,
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple
    )]
    public class InstapaperMobilizerFormatter : IInstapaperMobilizerFormatter
    {
        ReadabilityCache cache;
        HtmlFormatter formatter;
        //ThreadLocal<DataContractJsonSerializer> serializerCache;

        //public static string BaseArticleRedirectUrl { get; set; }

        public InstapaperMobilizerFormatter(ReadabilityCache cache, HtmlFormatter formatter)
        {
            this.cache = cache;
            this.formatter = formatter;
            //serializerCache = new ThreadLocal<DataContractJsonSerializer>(() => new DataContractJsonSerializer(typeof(ReadabilityResult)));
        }

        public IAsyncResult BeginGetString(string url, AsyncCallback callback, object state)
        {
            return cache
                .Get(HttpUtility.UrlEncode(url))
                .ToObservable()
                .ToTask(state)
                .ContinueWith(task => callback(task));
        }

        public Stream EndGetString(IAsyncResult result)
        {
            var task = (Task<ReadabilityResult>)result;

            if (task.Exception != null)
            {
                //string error = "There was an error with this page.  Try turning the mobilizer off temporarily (the switch in the upper right).";
                //byte[] errorBytes = Encoding.UTF8.GetBytes(error);
                //return new MemoryStream(errorBytes);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                return null;
            }

            var readability = task.Result;
            byte[] bytes;

            var serializer = new DataContractJsonSerializer(typeof(ReadabilityResult));
            bytes = Compress(readability, serializer.WriteObject);

            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            WebOperationContext.Current.OutgoingResponse.Headers.Add(HttpResponseHeader.ContentEncoding, "gzip");
            WebOperationContext.Current.OutgoingResponse.ContentLength = bytes.LongLength;

            return new MemoryStream(bytes);
        }

        //IObservable<byte[]> GetResponseBytes(string url)
        //{
        //    var observer = new AsyncSubject<byte[]>();

        //    try
        //    {
        //        GetFormattedHtmlAsync(url)
        //            .Take(1)
        //            .Subscribe(
        //                rawHtml =>
        //                {
        //                    var resultBytes = Compress(rawHtml);
        //                    observer.OnNext(resultBytes);
        //                    observer.OnCompleted();
        //                },
        //                exception => observer.OnError(exception));
        //    }
        //    catch (Exception exception)
        //    {
        //        observer.OnError(exception);
        //    }

        //    return observer.AsObservable();
        //}

        //static IObservable<string> GetFormattedHtmlAsync(string url)
        //{
        //    return Observable.Create<string>(observer =>
        //    {
        //        var disposables = new CompositeDisposable();
        //        try
        //        {
        //            var request = HttpWebRequest.Create(url);
        //            var asyncResponse = Observable
        //                .FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.EndGetResponse)()
        //                .Take(1);

        //            var disp = asyncResponse.Subscribe(
        //                response =>
        //                {
        //                    try
        //                    {
        //                        using (var stream = response.GetResponseStream())
        //                        {
        //                            string result = ReformatHtml(stream);
        //                            stream.Close();
        //                            observer.OnNext(result);
        //                        }
        //                    }
        //                    catch (Exception exception)
        //                    {
        //                        observer.OnError(exception);
        //                    }
        //                    finally
        //                    {
        //                        response.Close();
        //                        ((IDisposable)response).Dispose();
        //                    }
        //                },
        //                exception => observer.OnError(exception));
        //        }
        //        catch (Exception exception)
        //        {
        //            observer.OnError(exception);
        //        }
        //        return disposables;
        //    });
        //}

        //static string ReformatHtml(Stream result)
        //{
        //    var stopwatch = Stopwatch.StartNew();
        //    if (result == null)
        //        return null;

        //    HtmlDocument doc = new HtmlDocument();
        //    doc.OptionDefaultStreamEncoding = Encoding.UTF8;
        //    doc.Load(result);

        //    stopwatch.Stop();
        //    Console.WriteLine("Took {0} milliseconds to load the HTML", stopwatch.ElapsedMilliseconds);

        //    stopwatch = Stopwatch.StartNew();

        //    var htmlNode = doc.DocumentNode.ChildNodes.FindFirst("html");
        //    var headNode = htmlNode.ChildNodes.FindFirst("head");
        //    var metaNode = headNode.ChildNodes.FindFirst("meta");
        //    var styleNode = htmlNode.ChildNodes.FindFirst("style");
        //    var bodyNode = htmlNode.ChildNodes.FindFirst("body");
        //    var scriptNode = headNode.ChildNodes.FindFirst("script");

        //    stopwatch.Stop();
        //    Console.WriteLine("Took {0} milliseconds to find the nodes", stopwatch.ElapsedMilliseconds);

        //    stopwatch = Stopwatch.StartNew();
        //    //metaNode.Attributes["content"].Value =
        //    //    "width=device-width; initial-scale=1.0; user-scalable=no; minimum-scale=1.0; maximum-scale=1.0;";

        //    //metaNode.Attributes["content"].Value =
        //    //    "width=320; initial-scale=1.0; user-scalable=no; minimum-scale=1.0; maximum-scale=1.0;";

        //    if (scriptNode != null)
        //        scriptNode.InnerHtml = HtmlStaticStrings.SCRIPT;
        //    if (styleNode != null)
        //        styleNode.InnerHtml = HtmlStaticStrings.STYLE;

        //    var storyNode = bodyNode.ChildNodes
        //        .Where(node =>
        //            node.Name == "div" &&
        //            node.Attributes
        //                .Where(a => a.Name == "id" && a.Value == "story")
        //                .Count() > 0)
        //        .SingleOrDefault();

        //    if (storyNode == null)
        //        goto AFTER_PROCESS_HEADER_NODE;

        //    int numBodyChildNodes = bodyNode.ChildNodes.Count;
        //    int storyNodeIndex = bodyNode.ChildNodes.IndexOf(storyNode);

        //    var junkNodeRange = Enumerable.Range(0, storyNodeIndex);
        //    if (numBodyChildNodes > storyNodeIndex + 1)
        //    {
        //        int afterStoryNodeIndex = storyNodeIndex + 1;
        //        int numberOfNodesAfter = numBodyChildNodes - afterStoryNodeIndex;
        //        junkNodeRange = junkNodeRange.Union(
        //            Enumerable.Range(afterStoryNodeIndex, numberOfNodesAfter));
        //    }

        //    //var junkNodes = Enumerable.Range(0, 9).Select(i => bodyNode.ChildNodes[i]).ToList();
        //    var junkNodes = junkNodeRange.Select(i => bodyNode.ChildNodes[i]).ToList();

        //    foreach (var node in junkNodes)
        //        node.Remove();

        //    stopwatch.Stop();
        //    Console.WriteLine("Took {0} milliseconds to delete junk nodes", stopwatch.ElapsedMilliseconds);


        //    #region REMOVE ALL NODES PRECEDING THE HEADER NODE, IF PRESENT

        //    stopwatch = Stopwatch.StartNew();

        //    // REMOVE ALL NODES PRECEDING THE HEADER NODE, IF PRESENT
        //    var headerNode = storyNode.ChildNodes.FindFirst("h1");

        //    if (headerNode == null)
        //        goto AFTER_PROCESS_HEADER_NODE;

        //    List<HtmlNode> nodesToRemove = new List<HtmlNode>();
        //    var currentNode = headerNode;
        //    while (true)
        //    {
        //        var currentParentNode = currentNode.ParentNode;
        //        var currentNodesIndex = currentParentNode.ChildNodes.IndexOf(currentNode);
        //        if (currentNodesIndex > 0 && currentNodesIndex < currentParentNode.ChildNodes.Count)
        //        {
        //            nodesToRemove.AddRange(
        //                Enumerable.Range(0, currentNodesIndex)
        //                    .Select(i => currentParentNode.ChildNodes[i]));
        //        }
        //        else
        //            break;

        //        if (currentParentNode == storyNode)
        //            break;

        //        currentNode = currentParentNode;
        //    }

        //    foreach (var node in nodesToRemove)
        //        node.Remove();

        //    stopwatch.Stop();
        //    Console.WriteLine("Took {0} milliseconds to remove nodes prior to h1", stopwatch.ElapsedMilliseconds);

        //    #endregion


        //AFTER_PROCESS_HEADER_NODE:

        //    //bodyNode.InnerHtml = bodyNode.InnerHtml
        //    //    .Replace("‘", "&lsquo;")        // left single quote 
        //    //    .Replace("’", "&rsquo;")        // right single quote 
        //    //    .Replace("“", "&ldquo;")        // left double quote 
        //    //    .Replace("”", "&rdquo;");       // right double quote 

        //    stopwatch = Stopwatch.StartNew();

        //    var links = bodyNode
        //        .Descendants("a")
        //        .Where(a => a.Attributes.Contains("href"))
        //        .Select(a => a.Attributes["href"])
        //        .OfType<HtmlAttribute>()
        //        .ToList();

        //    var replacementUrl = string.Format("{0}/IPF?url=", BaseArticleRedirectUrl);
        //    foreach (var link in links)
        //    {
        //        if (link.Value.StartsWith("http://www.instapaper.com/m?u="))
        //            link.Value = link.Value.Replace(
        //                "http://www.instapaper.com/m?u=",
        //                replacementUrl);
        //    }

        //    stopwatch.Stop();
        //    Console.WriteLine("Took {0} milliseconds to find swap Instapaper URL", stopwatch.ElapsedMilliseconds);

        //    stopwatch = Stopwatch.StartNew();

        //    var brandingNode = doc.CreateElement("div");
        //    var brandingHtml = string.Format(HtmlStaticStrings.BRANDING, BaseArticleRedirectUrl);
        //    brandingNode.InnerHtml = brandingHtml;
        //    bodyNode.AppendChild(brandingNode);

        //    string output = doc.DocumentNode.WriteTo();
        //    //Debug.WriteLine(output);
        //    stopwatch.Stop();
        //    Console.WriteLine("Took {0} milliseconds to write the html", stopwatch.ElapsedMilliseconds);
        //    Console.WriteLine();
        //    return output;
        //}

        public static byte[] Compress<T>(T data)
        {
            byte[] result = null;
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(memory, CompressionMode.Compress, false))
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    formatter.Serialize(zip, data);
                }

                result = memory.ToArray();
            }

            return result;
        }

        public static byte[] Compress(string data)
        {
            byte[] temp = Encoding.UTF8.GetBytes(data);

            using (MemoryStream memory = new MemoryStream())
            using (GZipStream zip = new GZipStream(memory, CompressionMode.Compress, false))
            {
                zip.Write(temp, 0, temp.Length);
                return memory.ToArray();
            }
        }

        public static byte[] Compress<T>(T data, Action<Stream, T> serialize)
        {
            byte[] result = null;
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    serialize(zip, data);
                }

                result = memory.ToArray();
            }

            return result;
        }
    }
}
