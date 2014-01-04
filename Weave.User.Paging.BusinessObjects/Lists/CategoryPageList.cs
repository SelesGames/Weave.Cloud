
namespace Weave.User.Paging.BusinessObjects.Lists
{
    public class CategoryPageList : BasePageList
    {
        public string Category { get; set; }

        public override string CreateFileName()
        {
            return string.Format(
                "{0}.PageList",
                System.Web.HttpUtility.UrlEncode(Category));
        }
    }
}
