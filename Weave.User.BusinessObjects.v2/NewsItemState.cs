using System;

namespace Weave.User.BusinessObjects.v2
{
    [Flags]
    public enum NewsItemState : byte
    {
        None = 0,
        Read = 1,
        Favorite = 2
    }
}
