
namespace Weave.User.Paging.News
{
    public class PagedNewsByAll : PagedNewsBase
    {
        public override string CreateFileName()
        {
            return string.Format(
                "{0}-{1}-{2}",
                UserId.ToString("N"),
                ListId.ToString("N"),
                Index);
        }
    }
}
