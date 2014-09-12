using System;
using System.Collections;
using System.Collections.Generic;

namespace Weave.Parsing
{
    public class Images : IEnumerable<string>
    {
        HashSet<string> innerList;
        
        public Images()
        {
            innerList = new HashSet<string>();
        }

        public bool Add(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return false;

            if (imageUrl.StartsWith("http://share.feedsportal.com/share/", StringComparison.OrdinalIgnoreCase))
                return false;

            if (imageUrl.StartsWith("http://res3.feedsportal.com/social/", StringComparison.OrdinalIgnoreCase))
                return false;
            
            return innerList.Add(imageUrl);
        }




        #region IEnumerable interface implementation

        public IEnumerator<string> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        #endregion
    }
}
