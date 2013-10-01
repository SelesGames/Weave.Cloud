using Common.Data;
using SelesGames.WebApi;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.Identity.Service.Contracts;
using Weave.Identity.Service.DTOs;
using Weave.Identity.Service.Sql;

namespace Weave.Identity.Service.WorkerRole.Controllers
{
    public class IdentityController : ApiController, IIdentityService
    {
        ITransactionalDatabaseClient client;

        public IdentityController(ITransactionalDatabaseClient client)
        {
            this.client = client;
        }

        [HttpGet]
        [ActionName("sync")]
        public Task<IdentityInfo> Sync(
            Guid userId,
            string facebookToken = null,
            string twitterToken = null,
            string microsoftToken = null,
            string googleToken = null)
        {
            var tokens = new[] { facebookToken, twitterToken, microsoftToken, googleToken };

            if (tokens.Count(o => !string.IsNullOrWhiteSpace(o)) > 1)
            {
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "You can only specify one identity provider token at a time");          
            }

            if (!string.IsNullOrWhiteSpace(facebookToken))
                return SyncFacebook(userId, facebookToken);

            if (!string.IsNullOrWhiteSpace(twitterToken))
                return SyncTwitter(userId, twitterToken);

            if (!string.IsNullOrWhiteSpace(microsoftToken))
                return SyncMicrosoft(userId, microsoftToken);

            if (!string.IsNullOrWhiteSpace(googleToken))
                return SyncGoogle(userId, googleToken);

            throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest,
                "You must specify one of the following: facebookToken, twitterToken, microsoftToken, or googleToken");
        }


        [HttpGet]
        public Task<IdentityInfo> Get(
            Guid? userId = null,
            string facebookToken = null, 
            string twitterToken = null,
            string microsoftToken = null,
            string googleToken = null,
            string username = null,
            string password = null)
        {
            if (userId.HasValue)
                return GetUserById(userId.Value);

            if (!string.IsNullOrWhiteSpace(facebookToken))
                return GetUserFromFacebookToken(facebookToken);

            if (!string.IsNullOrWhiteSpace(twitterToken))
                return GetUserFromTwitterToken(twitterToken);

            if (!string.IsNullOrWhiteSpace(microsoftToken))
                return GetUserFromMicrosoftToken(microsoftToken);

            if (!string.IsNullOrWhiteSpace(googleToken))
                return GetUserFromGoogleToken(googleToken);

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                return GetUserFromUserNameAndPassword(username, password);

            throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest,
                "You must specify one of the following: facebookToken, twitterToken, microsoftToken, googleToken, or username and password combo");
        }






        #region Specific Get implementations

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

        public Task<IdentityInfo> GetUserFromFacebookToken(string facebookToken)
        {
            if (string.IsNullOrWhiteSpace(facebookToken))
                throw new ArgumentException("facebookToken in GetUserIdFromFacebookToken");

            var ids = client
                .Get<AuthInfo>()
                .Where(x => facebookToken.Equals(x.FacebookAuthString))
                .Select(Convert);

            if (ids.Any())
                return Task.FromResult(ids.First());

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that facebookToken");
        }

        public Task<IdentityInfo> GetUserFromTwitterToken(string twitterToken)
        {
            if (string.IsNullOrWhiteSpace(twitterToken))
                throw new ArgumentException("twitterToken in GetUserIdFromTwitterToken");

            var ids = client
                .Get<AuthInfo>()
                .Where(x => twitterToken.Equals(x.TwitterAuthString))
                .Select(Convert);

            if (ids.Any())
                return Task.FromResult(ids.First());

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that twitterToken");
        }

        public Task<IdentityInfo> GetUserFromMicrosoftToken(string microsoftToken)
        {
            if (string.IsNullOrWhiteSpace(microsoftToken))
                throw new ArgumentException("microsoftToken in GetUserIdFromMicrosoftToken");

            var ids = client.Get<AuthInfo>()
                .Where(x => microsoftToken.Equals(x.MicrosoftAuthString))
                .Select(Convert);

            if (ids.Any())
                return Task.FromResult(ids.First());

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that microsoftToken");
        }

        public Task<IdentityInfo> GetUserFromGoogleToken(string googleToken)
        {
            if (string.IsNullOrWhiteSpace(googleToken))
                throw new ArgumentException("googleToken in GetUserIdFromGoogleToken");

            var ids = client.Get<AuthInfo>()
                .Where(x => googleToken.Equals(x.GoogleAuthString))
                .Select(Convert);

            if (ids.Any())
                return Task.FromResult(ids.First());

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that googleToken");
        }

        public Task<IdentityInfo> GetUserFromUserNameAndPassword(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username in GetUserFromUserNameAndPassword");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("password in GetUserFromUserNameAndPassword");

            var ids = client.Get<AuthInfo>()
                .Where(x => username.Equals(x.UserName))
                .Select(Convert);

            if (ids.Any())
            {
                var matchingId = ids.First();

                // hash the password, which was what was saved in the database as opposed to the raw password
                var passwordHash = Hash(password);
                if (passwordHash.Equals(matchingId.PasswordHash))
                    return Task.FromResult(matchingId);
                else
                    throw ResponseHelper.CreateResponseException(HttpStatusCode.Forbidden, "password does not match for the given username");
            }

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that username");
        }

        string Hash(string password)
        {
            return password;
        }

        #endregion




        [HttpPost]
        public void Add(IdentityInfo user)
        {
            var sqlUser = Convert(user);

            client.Insert(sqlUser);
            client.SubmitChanges();
        }

        [HttpPut]
        public void Update(IdentityInfo user)
        {
            Sql.AuthInfo existingUser = null;
            try
            {
                existingUser = client.Get<AuthInfo>().SingleOrDefault(o => o.UserId.Equals(user.UserId));
            }
            catch{}

            if (existingUser == null)
            {
                Add(user);
                return;
            }

            existingUser.FacebookAuthString = user.FacebookAuthToken;
            existingUser.GoogleAuthString = user.GoogleAuthToken;
            existingUser.MicrosoftAuthString = user.MicrosoftAuthToken;
            existingUser.TwitterAuthString = user.TwitterAuthToken;
            existingUser.UserName = user.UserName;
            existingUser.PasswordHash = user.PasswordHash;

            try
            {
                client.Update(existingUser);
                client.SubmitChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }




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




        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                return;

            if (client != null)
                client.Dispose();

            base.Dispose();
        }

        #endregion




        #region Conversion helper methods

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

        #endregion
    }
}
