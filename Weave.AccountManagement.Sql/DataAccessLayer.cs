using Common.Data;
using SelesGames.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.AccountManagement.Sql
{
    public class DataAccessLayer
    {
        IProvider<ITransactionalDatabaseClient> clientProvider;

        public DataAccessLayer(IProvider<ITransactionalDatabaseClient> clientProvider)
        {
            this.clientProvider = clientProvider;
        }

        public async Task<Guid> GetUserIdFromFacebookToken(string facebookToken)
        {
            if (string.IsNullOrWhiteSpace(facebookToken))
                throw new ArgumentException("facebookToken in GetUserIdFromFacebookToken");

            using (var client = clientProvider.Get())
            {
                var id = await client.Get<Linq.UserInfo, Guid>(o => o
                    .Where(x => facebookToken.Equals(x.FacebookAuthString))
                    .Select(x => x.UserId));

                return id.First();
            }
        }

        public async Task<Guid> GetUserIdFromTwitterToken(string twitterToken)
        {
            if (string.IsNullOrWhiteSpace(twitterToken))
                throw new ArgumentException("twitterToken in GetUserIdFromTwitterToken");

            using (var client = clientProvider.Get())
            {
                var id = await client.Get<Linq.UserInfo, Guid>(o => o
                    .Where(x => twitterToken.Equals(x.TwitterAuthString))
                    .Select(x => x.UserId));

                return id.First();
            }
        }

        public async Task<Guid> GetUserIdFromMicrosoftToken(string microsoftToken)
        {
            if (string.IsNullOrWhiteSpace(microsoftToken))
                throw new ArgumentException("microsoftToken in GetUserIdFromMicrosoftToken");

            using (var client = clientProvider.Get())
            {
                var id = await client.Get<Linq.UserInfo, Guid>(o => o
                    .Where(x => microsoftToken.Equals(x.MicrosoftAuthString))
                    .Select(x => x.UserId));

                return id.First();
            }
        }

        public async Task<Guid> GetUserIdFromGoogleToken(string googleToken)
        {
            if (string.IsNullOrWhiteSpace(googleToken))
                throw new ArgumentException("googleToken in GetUserIdFromGoogleToken");

            using (var client = clientProvider.Get())
            {
                var id = await client.Get<Linq.UserInfo, Guid>(o => o
                    .Where(x => googleToken.Equals(x.GoogleAuthString))
                    .Select(x => x.UserId));

                return id.First();
            }
        }

        public async Task Add(UserInfo user)
        {
            var sqlUser = Convert(user);

            using (var client = clientProvider.Get())
            {
                client.Insert(sqlUser);
                await client.SubmitChanges();
            }
        }

        public async Task Update(UserInfo user)
        {
            var sqlUser = Convert(user);

            using (var client = clientProvider.Get())
            {
                client.Update(sqlUser);
                await client.SubmitChanges();
            }
        }

        Linq.UserInfo Convert(UserInfo user)
        {
            return new Linq.UserInfo
            {
                UserId = user.Id,
                FacebookAuthString = user.FacebookAuthToken,
                TwitterAuthString = user.TwitterAuthToken,
                MicrosoftAuthString = user.MicrosoftAuthToken,
                GoogleAuthString = user.GoogleAuthToken,
            };
        }
    }
}
