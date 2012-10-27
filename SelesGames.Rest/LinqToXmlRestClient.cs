using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SelesGames.Rest
{
    public class LinqToXmlRestClient<T>
    {
        public bool UseGzip { get; set; }

        public Task<T> GetAndParseAsync(string url, Func<XElement, T> parser, CancellationToken cancellationToken)
        {
            var client = new XmlRestClient { UseGzip = UseGzip };
            return client.GetAsync(url, cancellationToken).ContinueWith(task => parser(task.Result), cancellationToken);
        }
    }
}
