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
        [NonAction]
        public async Task<IdentityInfo> GetUserFromFacebookToken(string facebookToken)
        {
            if (string.IsNullOrWhiteSpace(facebookToken))
                throw new ArgumentException("facebookToken in GetUserIdFromFacebookToken");

            var ids = await client.Get<AuthInfo, IdentityInfo>(o => o
                .Where(x => facebookToken.Equals(x.FacebookAuthString))
                .Select(Convert).AsQueryable());

            if (ids.Any())
                return ids.First();

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that facebookToken");
        }

        [HttpGet]
        public async Task<IdentityInfo> GetUserFromTwitterToken(string twitterToken)
        {
            if (string.IsNullOrWhiteSpace(twitterToken))
                throw new ArgumentException("twitterToken in GetUserIdFromTwitterToken");

            var ids = await client.Get<AuthInfo, IdentityInfo>(o => o
                .Where(x => twitterToken.Equals(x.TwitterAuthString))
                .Select(Convert).AsQueryable());

            if (ids.Any())
                return ids.First();

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that twitterToken");
        }

        [HttpGet]
        public async Task<IdentityInfo> GetUserFromMicrosoftToken(string microsoftToken)
        {
            if (string.IsNullOrWhiteSpace(microsoftToken))
                throw new ArgumentException("microsoftToken in GetUserIdFromMicrosoftToken");

            var ids = await client.Get<AuthInfo, IdentityInfo>(o => o
                .Where(x => microsoftToken.Equals(x.MicrosoftAuthString))
                .Select(Convert).AsQueryable());

            if (ids.Any())
                return ids.First();

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that microsoftToken");
        }

        [HttpGet]
        public async Task<IdentityInfo> GetUserFromGoogleToken(string googleToken)
        {
            if (string.IsNullOrWhiteSpace(googleToken))
                throw new ArgumentException("googleToken in GetUserIdFromGoogleToken");

            var ids = await client.Get<AuthInfo, IdentityInfo>(o => o
                .Where(x => googleToken.Equals(x.GoogleAuthString))
                .Select(Convert).AsQueryable());

            if (ids.Any())
                return ids.First();

            throw ResponseHelper.CreateResponseException(HttpStatusCode.NotFound, "No user found matching that googleToken");
        }

        [HttpGet]
        public Task<IdentityInfo> GetUserFromUserNameAndPassword(string username, string password)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task Add(IdentityInfo user)
        {
            var sqlUser = Convert(user);

            client.Insert(sqlUser);
            await client.SubmitChanges();
        }

        [HttpPut]
        public async Task Update(IdentityInfo user)
        {
            var sqlUser = Convert(user);

            client.Update(sqlUser);
            await client.SubmitChanges();
        }




        #region IDisposable

        protected override void Dispose(bool disposing)
        {
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
