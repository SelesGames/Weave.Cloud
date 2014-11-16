using Common.Data;
using SelesGames.WebApi;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Weave.Identity.Service.Sql;
using Weave.Services.Identity.Contracts;
using Weave.Services.Identity.DTOs;

namespace Weave.Identity.Service.WorkerRole
{
    public class IdentityLogic: IIdentityService
    {
        ITransactionalDatabaseClient client;

        public IdentityLogic(ITransactionalDatabaseClient client)
        {
            this.client = client;
        }




        #region Get User by userId

        public Task<IdentityInfo> GetUserById(Guid userId)
        {
            var ids = client
                .Get<AuthInfo>()
                .Where(x => userId.Equals(x.UserId))
                .Select(Convert);

            if (ids.Any())
                return Task.FromResult(ids.First());

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that userId");
        }

        #endregion




        #region Sync functions - take in a userId and an identity provider token, and map or create an account as appropriate

        public Task<IdentityInfo> SyncFacebook(Guid userId, string facebookToken)
        {
            IdentityInfo identity;

            if (string.IsNullOrWhiteSpace(facebookToken))
                throw new ArgumentException("facebookToken in SyncFacebook");

            var matchingAccounts = client.Get<AuthInfo>()
                .Where(x => x.UserId.Equals(userId) || facebookToken.Equals(x.FacebookAuthString))
                .ToList();

            var matchedUser = matchingAccounts.SingleOrDefault(o => o.UserId == userId);
            var matchedToken = matchingAccounts.SingleOrDefault(o => o.FacebookAuthString == facebookToken);

            // user exists, but facebook account is new
            if (matchedUser != null && matchedToken == null)
            {
                matchedUser.FacebookAuthString = facebookToken;
                client.Update(matchedUser);
                client.SubmitChanges();
                identity = Convert(matchedUser);
            }

            // facebook account exists but with a different UserId than what was provided
            else if (matchedUser == null && matchedToken != null)
            {
                identity = Convert(matchedToken);
            }

            // neither matched - brand new identity info
            else if (matchedUser == null && matchedToken == null)
            {
                var newSqlIdentity = new AuthInfo
                {
                    UserId = userId,
                    FacebookAuthString = facebookToken
                };
                client.Insert(newSqlIdentity);
                client.SubmitChanges();
                identity = Convert(newSqlIdentity);
            }

            // both facebook and userId matched.  Could be same or could be different users, but prioritize Facebook
            else
            {
                identity = Convert(matchedToken);
            }

            if (identity != null)
                return Task.FromResult(identity);

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that facebookToken");
        }

        public Task<IdentityInfo> SyncTwitter(Guid userId, string twitterToken)
        {
            IdentityInfo identity;

            if (string.IsNullOrWhiteSpace(twitterToken))
                throw new ArgumentException("facebookToken in SyncTwitter");

            var matchingAccounts = client.Get<AuthInfo>()
                .Where(x => x.UserId.Equals(userId) || twitterToken.Equals(x.TwitterAuthString))
                .ToList();

            var matchedUser = matchingAccounts.SingleOrDefault(o => o.UserId == userId);
            var matchedToken = matchingAccounts.SingleOrDefault(o => o.TwitterAuthString == twitterToken);

            // user exists, but facebook account is new
            if (matchedUser != null && matchedToken == null)
            {
                matchedUser.TwitterAuthString = twitterToken;
                client.Update(matchedUser);
                client.SubmitChanges();
                identity = Convert(matchedUser);
            }

            // facebook account exists but with a different UserId than what was provided
            else if (matchedUser == null && matchedToken != null)
            {
                identity = Convert(matchedToken);
            }

            // neither matched - brand new identity info
            else if (matchedUser == null && matchedToken == null)
            {
                var newSqlIdentity = new AuthInfo
                {
                    UserId = userId,
                    TwitterAuthString = twitterToken
                };
                client.Insert(newSqlIdentity);
                client.SubmitChanges();
                identity = Convert(newSqlIdentity);
            }

            // both facebook and userId matched.  Could be same or could be different users, but prioritize Facebook
            else
            {
                identity = Convert(matchedToken);
            }

            if (identity != null)
                return Task.FromResult(identity);

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that facebookToken");
        }

        public Task<IdentityInfo> SyncMicrosoft(Guid userId, string microsoftToken)
        {
            IdentityInfo identity;

            if (string.IsNullOrWhiteSpace(microsoftToken))
                throw new ArgumentException("microsoftToken in SyncMicrosoft");

            var matchingAccounts = client.Get<AuthInfo>()
                .Where(x => x.UserId.Equals(userId) || microsoftToken.Equals(x.MicrosoftAuthString))
                .ToList();

            var matchedUser = matchingAccounts.SingleOrDefault(o => o.UserId == userId);
            var matchedToken = matchingAccounts.SingleOrDefault(o => o.MicrosoftAuthString == microsoftToken);

            // user exists, but facebook account is new
            if (matchedUser != null && matchedToken == null)
            {
                matchedUser.MicrosoftAuthString = microsoftToken;
                client.Update(matchedUser);
                client.SubmitChanges();
                identity = Convert(matchedUser);
            }

            // facebook account exists but with a different UserId than what was provided
            else if (matchedUser == null && matchedToken != null)
            {
                identity = Convert(matchedToken);
            }

            // neither matched - brand new identity info
            else if (matchedUser == null && matchedToken == null)
            {
                var newSqlIdentity = new AuthInfo
                {
                    UserId = userId,
                    MicrosoftAuthString = microsoftToken
                };
                client.Insert(newSqlIdentity);
                client.SubmitChanges();
                identity = Convert(newSqlIdentity);
            }

            // both facebook and userId matched.  Could be same or could be different users, but prioritize Facebook
            else
            {
                identity = Convert(matchedToken);
            }

            if (identity != null)
                return Task.FromResult(identity);

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that facebookToken");
        }

        public Task<IdentityInfo> SyncGoogle(Guid userId, string googleToken)
        {
            IdentityInfo identity;

            if (string.IsNullOrWhiteSpace(googleToken))
                throw new ArgumentException("googleToken in SyncGoogle");

            var matchingAccounts = client.Get<AuthInfo>()
                .Where(x => x.UserId.Equals(userId) || googleToken.Equals(x.GoogleAuthString))
                .ToList();

            var matchedUser = matchingAccounts.SingleOrDefault(o => o.UserId == userId);
            var matchedToken = matchingAccounts.SingleOrDefault(o => o.GoogleAuthString == googleToken);

            // user exists, but facebook account is new
            if (matchedUser != null && matchedToken == null)
            {
                matchedUser.GoogleAuthString = googleToken;
                client.Update(matchedUser);
                client.SubmitChanges();
                identity = Convert(matchedUser);
            }

            // facebook account exists but with a different UserId than what was provided
            else if (matchedUser == null && matchedToken != null)
            {
                identity = Convert(matchedToken);
            }

            // neither matched - brand new identity info
            else if (matchedUser == null && matchedToken == null)
            {
                var newSqlIdentity = new AuthInfo
                {
                    UserId = userId,
                    GoogleAuthString = googleToken
                };
                client.Insert(newSqlIdentity);
                client.SubmitChanges();
                identity = Convert(newSqlIdentity);
            }

            // both facebook and userId matched.  Could be same or could be different users, but prioritize Facebook
            else
            {
                identity = Convert(matchedToken);
            }

            if (identity != null)
                return Task.FromResult(identity);

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that facebookToken");
        }

        #endregion




        #region Conversion helper methods

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

        #endregion
    }
}
