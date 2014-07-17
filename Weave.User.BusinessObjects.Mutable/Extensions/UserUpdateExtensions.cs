using Weave.User.BusinessObjects.Mutable.Extensions;

namespace Weave.User.BusinessObjects.Mutable
{
    public static class UserUpdateExtensions
    {
        public static void UpdateFrom(this UserInfo o, UserIndex userIndex)
        {
            var helper = new UserMergeHelper(o, userIndex);
            helper.Merge();
        }
    }
}
