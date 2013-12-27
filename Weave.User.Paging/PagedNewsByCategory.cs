
namespace Weave.User.Paging
{
    public class PagedNewsByCategory : PagedNewsBase
    {
        public string Category { get; set; }

        public override string CreateFileName()
        {
            return string.Format(
                "{0}-{1}-{2}",
                UserId.ToString("N"),
                System.Web.HttpUtility.UrlEncode(Category),
                Index);
        }
    }
}
