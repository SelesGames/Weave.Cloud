using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.Paging;

namespace Test.UserPaging
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TestPageCreation().Wait();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            while (true)
                Console.ReadLine();
        }

        static async Task TestPageCreation()
        {
            Guid userId = Guid.Parse("0d13bf82-0f14-475f-9725-f97e5a123d5a");

            var userCred = new StorageCredentials("weaveuser", "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==");
            var userCsa = new CloudStorageAccount(userCred, false);

            var pagingCred = new StorageCredentials("weaveuser2", "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==");
            var pagingCsa = new CloudStorageAccount(pagingCred, false);

            var updater = new UserPagedNewsUpdater(userId, userCsa, pagingCsa);

            await updater.UpdateUsersPagedLists();
        }
    }
}
