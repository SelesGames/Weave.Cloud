
namespace Weave.User.Paging.BusinessObjects.News
{
    public class PagedNewsByCategory : PagedNewsBase
    {
        public string Category { get; set; }

        public override string CreateFileName()
        {
            return string.Format(
                "{0}-{1}-{2}",
                System.Web.HttpUtility.UrlEncode(Category),
                ListId.ToString("N"),
                Index);
        }
    }
}
