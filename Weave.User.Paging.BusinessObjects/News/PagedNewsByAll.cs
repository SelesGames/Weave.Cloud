
namespace Weave.User.Paging.BusinessObjects.News
{
    public class PagedNewsByAll : PagedNewsBase
    {
        public override string CreateFileName()
        {
            return string.Format(
                "{0}-{1}",
                ListId.ToString("N"),
                Index);
        }
    }
}
