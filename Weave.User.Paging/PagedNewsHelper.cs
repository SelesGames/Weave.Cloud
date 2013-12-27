using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;

namespace Weave.User.Paging
{
    public class PagedNewsHelper
    {
        UserInfo user;

        public async Task DoStuff()
        {
            var now = DateTime.UtcNow;

            await user.RefreshAllFeeds();

            //var feedsWithNew = user.Feeds.AllNews().Where(o => o.)
        }
    }
}
