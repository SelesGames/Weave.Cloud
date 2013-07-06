
namespace Weave.UserFeedAggregator
{
    public class AzureCredentials
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public bool UseHttps { get; set; }

        public AzureCredentials(string accountName, string accountKey, bool useHttps)
        {
            AccountName = accountName;
            AccountKey = accountKey;
            UseHttps = useHttps;
        }
    }
}
