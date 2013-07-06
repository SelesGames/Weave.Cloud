using System.Threading.Tasks;
using Weave.User.BusinessObjects;

namespace Weave.User.Service
{
    public interface IUserWriter
    {
        void DelayedWrite(UserInfo user);
        Task ImmediateWrite(UserInfo user);
    }
}
