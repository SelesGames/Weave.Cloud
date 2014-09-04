using System;
using System.Collections;
using System.Collections.Generic;

namespace Weave.Updater.BusinessObjects
{
    public class Images : IEnumerable<Image>
    {
        List<Image> innerList;

        public Images()
        {
            innerList = new List<Image>();
        }

        public bool Add(Image image)
        {
            if (image == null)
                return false;

            if (!Uri.IsWellFormedUriString(image.Url, UriKind.Absolute))
                return false;

            innerList.Add(image);
            return true;
        }




        #region IEnumerable interface implementation

        public IEnumerator<Image> GetEnumerator()
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
