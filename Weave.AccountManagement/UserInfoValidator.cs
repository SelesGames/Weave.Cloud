using Common.Validation;
using System.Linq;
using Weave.AccountManagement.DTOs;

namespace Weave.AccountManagement
{
    public class UserInfoValidator : ValidationEngine
    {
        public UserInfoValidator()
        {
            AddRule<UserInfo>(o => o.Feeds.Any(), "must be some feeds");
        }
    }
}
