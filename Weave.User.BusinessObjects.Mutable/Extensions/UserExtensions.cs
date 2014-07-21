using Weave.User.BusinessObjects.Mutable.Extensions;
using Weave.User.BusinessObjects.Mutable.Extensions.Helpers;

namespace Weave.User.BusinessObjects.Mutable
{
    public static class UserExtensions
    {
        public static void UpdateFrom(this UserInfo o, UserIndex userIndex)
        {
            UserMergeHelper.Merge(o, userIndex);
        }

        public static UserIndex CreateUserIndex(this UserInfo user)
        {
            return UserIndexCreator.Create(user);
        }
    }
}