using Weave.User.BusinessObjects.Mutable.Extensions;

namespace Weave.User.BusinessObjects.Mutable
{
    public class FeedIndexUpdateExtensions
    {
        public static void UpdateFrom(this UserInfo o, UserIndex userIndex)
        {
            var helper = new UserMergeHelper(o, userIndex);
            helper.Merge();
        }
    }
}
