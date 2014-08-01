using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.User.Service.Redis
{
    // redis maxes out to 16 databases by default, so don't index higher than 15
    class DatabaseNumbers
    {
        public const int INDICES_AND_NEWSCACHE = 0;
        public const int LOCK = 4;
    }
}
