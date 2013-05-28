using System.Threading.Tasks;
using Weave.User.BusinessObjects;

namespace Weave.UserFeedAggregator
{
    public interface IUserWriter
    {
        void DelayedWrite(UserInfo user);
        Task ImmediateWrite(UserInfo user);
    }
}
