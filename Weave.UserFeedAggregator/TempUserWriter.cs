using SelesGames.Common;
using System.Threading.Tasks;
using Weave.UserFeedAggregator.BusinessObjects;
using Weave.UserFeedAggregator.Converters;
using Weave.UserFeedAggregator.Repositories;
using Store = Weave.User.DataStore;

namespace Weave.UserFeedAggregator
{
    public class TempUserWriter : IUserWriter
    {
        UserRepository userRepo;

        public TempUserWriter(UserRepository userRepo)
        {
            this.userRepo = userRepo;
        }

        public void DelayedWrite(UserInfo user)
        {
            var userST = user.Convert<UserInfo, Store.UserInfo>(BusinessObjectToDataStore.Instance);
            userRepo.Save(userST);
        }

        public Task ImmediateWrite(UserInfo user)
        {
            var userST = user.Convert<UserInfo, Store.UserInfo>(BusinessObjectToDataStore.Instance);
            return userRepo.Save(userST);
        }
    }
}
