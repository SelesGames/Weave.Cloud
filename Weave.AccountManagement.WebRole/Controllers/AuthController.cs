using System;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.AccountManagement.Sql;

namespace Weave.AccountManagement.WebRole.Controllers
{
    public class AuthController : ApiController
    {
        DataAccessLayer dal;

        public AuthController(DataAccessLayer dal)
        {
            this.dal = dal;
        }

        [ActionName("facebook_token")]
        public Task<Guid> GetUserIdFromFacebookToken(string token)
        {
            return dal.GetUserIdFromFacebookToken(token);
        }

        [ActionName("twitter_token")]
        public Task<Guid> GetUserIdFromTwitterToken(string token)
        {
            return dal.GetUserIdFromTwitterToken(token);
        }

        [ActionName("microsoft_token")]
        public Task<Guid> GetUserIdFromMicrosoftToken(string token)
        {
            return dal.GetUserIdFromMicrosoftToken(token);
        }

        [ActionName("google_token")]
        public Task<Guid> GetUserIdFromGoogleToken(string token)
        {
            return dal.GetUserIdFromGoogleToken(token);
        }

        [HttpPut]
        public Task Add([FromBody] AuthInfo user)
        {
            return dal.Add(user);
        }

        [HttpPost]
        public Task Update([FromBody] AuthInfo user)
        {
            return dal.Update(user);
        }
    }
}