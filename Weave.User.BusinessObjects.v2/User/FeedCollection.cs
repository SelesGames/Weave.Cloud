using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.User.BusinessObjects.v2
{
    public class FeedCollection : List<Feed>
    {
        public IEnumerable<Feed> FindByCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return new List<Feed>(0);

            if ("all news".Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                return this;

            return this.Where(o => categoryName.Equals(o.Category, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Feed> FindById(Guid feedId)
        {
            return this.Where(o => feedId == o.Id);
        }
    }
}
