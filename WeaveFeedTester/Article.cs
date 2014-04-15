using System.Threading.Tasks;
using Weave.Mobilizer.Cache.Readability;

namespace WeaveFeedTester
{
    public class Article
    {
        public string Source { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string MobilizedHtml { get; set; }
        public string PublishDateTime { get; set; }

        public string FormattedSource
        {
            get
            {
                return string.Format("{0} • {1}", Source, PublishDateTime);
            }
        }

        public async Task LoadFormattedWebView()
        {
            var rb = new ReadabilityClient("a142c0cc575c6760d5c46247f8aa6aabbacb6fd8");
            var rresult = await rb.Mobilize(Link);//, Title, "red", Source);
            var rformat = new ReadabilityFormatter();
            var html = rformat.GetFormattedHtml(rresult, Link, Title, "red", FormattedSource);
            MobilizedHtml = html;
        }
    }
}
