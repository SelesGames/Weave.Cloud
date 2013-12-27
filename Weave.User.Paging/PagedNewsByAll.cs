
namespace Weave.User.Paging
{
    public class PagedNewsByAll : PagedNewsBase
    {
        public string CreateFileName()
        {
            return string.Format(
                "{0}-{1}",
                UserId.ToString("N"),
                Index);
        }
    }
}
