using SelesGames.Common;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.Service.Converters;
using Weave.User.Service.Repositories;
using Store = Weave.User.DataStore;

namespace Weave.User.Service
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
