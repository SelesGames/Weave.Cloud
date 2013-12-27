
namespace Weave.User.Paging
{
    public class PagedNewsByAll : PagedNewsBase
    {
        public override string CreateFileName()
        {
            return string.Format(
                "{0}-{1}",
                UserId.ToString("N"),
                Index);
        }
    }
}
