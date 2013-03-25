using Common.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Weave.User.DTOs;

namespace Weave.UserFeedAggregator.Role.Controllers
{
    public class UserFeedController : ApiController
    {
        IAzureBlobClient<UserInfo> userClient;

        public UserFeedController()
        {}

        public async Task Get(Guid userId)
        {
            var user = await userClient.Get(userId.ToString());
        }
    }
}
