using SelesGames.WebApi;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.Identity.Service.DTOs;

namespace Weave.Identity.Service.WorkerRole.Controllers
{
    public class SyncController : ApiController
    {
        IdentityLogic logic;

        public SyncController(IdentityLogic logic)
        {
            this.logic = logic;
        }

        public Task<IdentityInfo> Get(
            Guid? userId = null,
            string facebookToken = null,
            string twitterToken = null,
            string microsoftToken = null,
            string googleToken = null)
        {
            if (!userId.HasValue)
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest,
                    "You must specify a valid userId");

            var tokens = new[] { facebookToken, twitterToken, microsoftToken, googleToken };

            if (tokens.Count(o => !string.IsNullOrWhiteSpace(o)) > 1)
            {
                throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest, "You can only specify one identity provider token at a time");
            }

            if (!string.IsNullOrWhiteSpace(facebookToken))
                return logic.SyncFacebook(userId.Value, facebookToken);

            if (!string.IsNullOrWhiteSpace(twitterToken))
                return logic.SyncTwitter(userId.Value, twitterToken);

            if (!string.IsNullOrWhiteSpace(microsoftToken))
                return logic.SyncMicrosoft(userId.Value, microsoftToken);

            if (!string.IsNullOrWhiteSpace(googleToken))
                return logic.SyncGoogle(userId.Value, googleToken);

            throw ResponseHelper.CreateResponseException(HttpStatusCode.BadRequest,
                "You must specify one of the following: facebookToken, twitterToken, microsoftToken, or googleToken");
        }
    }
}
