using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;

namespace Weave.User.Service.Redis
{
    public class UserIndexCache
    {
        public Task<UserIndex> Get(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserIndex> Save(UserInfo userBO)
        {
            throw new NotImplementedException();
        }

        public Task Save(UserIndex userIndex)
        {
            throw new NotImplementedException();
        }
    }
}
