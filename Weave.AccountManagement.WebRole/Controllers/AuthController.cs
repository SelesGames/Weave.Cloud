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

        public Task<Guid> GetUserIdFromFacebookToken(string facebookToken)
        {
            return dal.GetUserIdFromFacebookToken(facebookToken);
        }

        public Task<Guid> GetUserIdFromTwitterToken(string twitterToken)
        {
            return dal.GetUserIdFromTwitterToken(twitterToken);
        }

        public Task<Guid> GetUserIdFromMicrosoftToken(string microsoftToken)
        {
            return dal.GetUserIdFromMicrosoftToken(microsoftToken);
        }

        public Task<Guid> GetUserIdFromGoogleToken(string googleToken)
        {
            return dal.GetUserIdFromGoogleToken(googleToken);
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