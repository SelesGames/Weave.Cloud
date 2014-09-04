using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Weave.Parsing;
using Weave.RssAggregator.LibraryClient;


namespace WeaveFeedTester
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FeedSource> Feeds { get; set; }
        public FeedSource SelectedFeed { get; set; }
        public LinkedList<Article> Articles { get; set; }
        public Article SelectedArticle
        {
            get { return selectedArticle; }
            set { selectedArticle = value; PropertyChanged(this, new PropertyChangedEventArgs("SelectedArticle")); }
        }
        Article selectedArticle;

        public MainViewModel()
        {
            Feeds = new ObservableCollection<FeedSource>();
        }

        public async Task LoadFeeds()
        {
            //var client = new ExpandedLibrary("https://weave.blob.core.windows.net/settings/masterfeeds.xml");
            var client = new FeedLibraryClient(@"C:\WORK\CODE\SELES GAMES\Weave.Cloud\masterfeeds.xml");
            await client.LoadFeedsAsync();
            var feeds = client.Feeds;

            var groupedFeeds = feeds.GroupBy(o => o.Category).ToList();


            foreach (var feed in feeds)
                Feeds.Add(feed);
        }

        public async Task LoadArticles()
        {
            if (SelectedFeed == null)
                return;

            var requester = new Feed { Uri = SelectedFeed.FeedUri };
            await requester.Update();
            Articles = new LinkedList<Article>(requester.News.Select(o =>
                new Article 
                { 
                    Source = SelectedFeed.FeedName,
                    Link = o.Link, 
                    Description = o.Description, 
                    Title = o.Title,
                    PublishDateTime = o.UtcPublishDateTimeString,
                    ImageUrl = o.GetImageUrl(),
                }));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
