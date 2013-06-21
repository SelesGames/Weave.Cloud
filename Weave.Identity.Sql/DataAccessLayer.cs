using Common.Data;
using SelesGames.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.Identity.Service.DTOs;

namespace Weave.Identity.Service.Sql
{
    public class DataAccessLayer
    {
        IProvider<ITransactionalDatabaseClient> clientProvider;

        public DataAccessLayer(IProvider<ITransactionalDatabaseClient> clientProvider)
        {
            this.clientProvider = clientProvider;
        }

        public async Task<IdentityInfo> GetUserFromFacebookToken(string facebookToken)
        {
            if (string.IsNullOrWhiteSpace(facebookToken))
                throw new ArgumentException("facebookToken in GetUserIdFromFacebookToken");

            using (var client = clientProvider.Get())
            {
                var id = await client.Get<AuthInfo, IdentityInfo>(o => o
                    .Where(x => facebookToken.Equals(x.FacebookAuthString))
                    .Select(Convert).AsQueryable());

                return id.First();
            }
        }

        public async Task<IdentityInfo> GetUserFromTwitterToken(string twitterToken)
        {
            if (string.IsNullOrWhiteSpace(twitterToken))
                throw new ArgumentException("twitterToken in GetUserIdFromTwitterToken");

            using (var client = clientProvider.Get())
            {
                var id = await client.Get<AuthInfo, IdentityInfo>(o => o
                    .Where(x => twitterToken.Equals(x.TwitterAuthString))
                    .Select(Convert).AsQueryable());

                return id.First();
            }
        }

        public async Task<IdentityInfo> GetUserFromMicrosoftToken(string microsoftToken)
        {
            if (string.IsNullOrWhiteSpace(microsoftToken))
                throw new ArgumentException("microsoftToken in GetUserIdFromMicrosoftToken");

            using (var client = clientProvider.Get())
            {
                var id = await client.Get<AuthInfo, IdentityInfo>(o => o
                    .Where(x => microsoftToken.Equals(x.MicrosoftAuthString))
                    .Select(Convert).AsQueryable());

                return id.First();
            }
        }

        public async Task<IdentityInfo> GetUserFromGoogleToken(string googleToken)
        {
            if (string.IsNullOrWhiteSpace(googleToken))
                throw new ArgumentException("googleToken in GetUserIdFromGoogleToken");

            using (var client = clientProvider.Get())
            {
                var id = await client.Get<AuthInfo, IdentityInfo>(o => o
                    .Where(x => googleToken.Equals(x.GoogleAuthString))
                    .Select(Convert).AsQueryable());

                return id.First();
            }
        }

        public async Task Add(IdentityInfo user)
        {
            var sqlUser = Convert(user);

            using (var client = clientProvider.Get())
            {
                client.Insert(sqlUser);
                await client.SubmitChanges();
            }
        }

        public async Task Update(IdentityInfo user)
        {
            var sqlUser = Convert(user);

            using (var client = clientProvider.Get())
            {
                client.Update(sqlUser);
                await client.SubmitChanges();
            }
        }

        AuthInfo Convert(IdentityInfo user)
        {
            return new AuthInfo
            {
                UserId = user.UserId,
                FacebookAuthString = user.FacebookAuthToken,
                TwitterAuthString = user.TwitterAuthToken,
                MicrosoftAuthString = user.MicrosoftAuthToken,
                GoogleAuthString = user.GoogleAuthToken,
                UserName = user.UserName,
                PasswordHash = user.PasswordHash,
            };
        }

        IdentityInfo Convert(AuthInfo user)
        {
            return new IdentityInfo
            {
                UserId = user.UserId,
                FacebookAuthToken = user.FacebookAuthString,
                TwitterAuthToken = user.TwitterAuthString,
                MicrosoftAuthToken = user.MicrosoftAuthString,
                GoogleAuthToken = user.GoogleAuthString,
                UserName = user.UserName,
                PasswordHash = user.PasswordHash,
            };
        }
    }
}
